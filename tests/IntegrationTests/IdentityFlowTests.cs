using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.ApiKeys.Create;
using Application.UserApplications.List;
using Application.Users;
using Application.Users.GetCurrentUser;
using Application.Users.Register;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace IntegrationTests;

[Collection(IntegrationCollection.Name)]
public sealed class IdentityFlowTests(CrowdNfoApiFactory factory) : IAsyncLifetime
{
    private const string Root = "/api/v1";

    Task IAsyncLifetime.InitializeAsync() => factory.ResetDatabaseAsync();

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_Approve_Login_Refresh_Flow()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";
        string password = "applicant-password-0123";

        int applicationId = await RegisterAsync(client, username, password);
        applicationId.ShouldBeGreaterThan(0);

        HttpResponseMessage prematureLogin = await client.PostAsJsonAsync(
            $"{Root}/auth/login",
            new { username, password });
        prematureLogin.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        AuthResponse adminAuth = await LoginAsync(client, CrowdNfoApiFactory.AdminUsername, CrowdNfoApiFactory.AdminPassword);
        int listedApplicationId = await FindApplicationIdAsync(client, adminAuth.AccessToken, username);
        listedApplicationId.ShouldBe(applicationId);

        HttpResponseMessage approve = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/applications/{applicationId}/approve", adminAuth.AccessToken, new { comment = "ok" }));
        approve.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        AuthResponse auth = await LoginAsync(client, username, password);
        auth.User.Username.ShouldBe(username);
        auth.User.Role.ShouldBe(RoleNames.User);
        auth.ExpiresIn.ShouldBeGreaterThan(0);

        HttpResponseMessage refresh = await client.PostAsJsonAsync(
            $"{Root}/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        AuthResponse refreshed = (await refresh.Content.ReadFromJsonAsync<AuthResponse>())!;
        refreshed.RefreshToken.ShouldNotBe(auth.RefreshToken);

        HttpResponseMessage reusedOldToken = await client.PostAsJsonAsync(
            $"{Root}/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        reusedOldToken.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        HttpResponseMessage refreshWithRotatedToken = await client.PostAsJsonAsync(
            $"{Root}/auth/refresh",
            new { refreshToken = refreshed.RefreshToken });
        refreshWithRotatedToken.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        HttpResponseMessage me = await client.SendAsync(
            Authorized(HttpMethod.Get, $"{Root}/users/me", refreshed.AccessToken));
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
        CurrentUserResponse current = (await me.Content.ReadFromJsonAsync<CurrentUserResponse>())!;
        current.Username.ShouldBe(username);
        current.IsActivated.ShouldBeTrue();
    }

    [Fact]
    public async Task Reject_DeletesUser_PreservesApplication()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";

        int applicationId = await RegisterAsync(client, username, "applicant-password-0123");

        Guid applicantUserId;
        using (IServiceScope readScope = factory.Services.CreateScope())
        {
            ApplicationDbContext readContext = readScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            UserApplication pending = await readContext.UserApplications.SingleAsync(a => a.Id == applicationId);
            applicantUserId = pending.UserId!.Value;
        }

        AuthResponse adminAuth = await LoginAsync(client, CrowdNfoApiFactory.AdminUsername, CrowdNfoApiFactory.AdminPassword);

        HttpResponseMessage reject = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/applications/{applicationId}/reject", adminAuth.AccessToken, new { comment = "no" }));
        reject.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        bool userExists = await dbContext.Users.AnyAsync(user => user.Id == applicantUserId);
        userExists.ShouldBeFalse();

        UserApplication application = await dbContext.UserApplications.SingleAsync(a => a.Id == applicationId);
        application.Status.ShouldBe(ApplicationStatus.Rejected);
        application.UserId.ShouldBeNull();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"{Root}/users/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Lock_BlocksLoginWithReason_AndUnlockRestoresIt()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";
        string password = "applicant-password-0123";
        const string reason = "Spamming submissions";

        Guid userId = await RegisterAndApproveAsync(client, username, password);
        string adminToken = (await LoginAsync(client, CrowdNfoApiFactory.AdminUsername, CrowdNfoApiFactory.AdminPassword)).AccessToken;

        HttpResponseMessage lockResponse = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/users/{userId}/lock", adminToken, new { reason }));
        lockResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage lockedLogin = await client.PostAsJsonAsync($"{Root}/auth/login", new { username, password });
        lockedLogin.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await lockedLogin.Content.ReadAsStringAsync()).ShouldContain(reason);

        HttpResponseMessage unlockResponse = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/users/{userId}/unlock", adminToken));
        unlockResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage loginAfterUnlock = await client.PostAsJsonAsync($"{Root}/auth/login", new { username, password });
        loginAfterUnlock.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_RevokesSessions_AndInvalidatesOldPassword()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";
        const string oldPassword = "applicant-password-0123";
        const string newPassword = "fresh-password-9876";

        _ = await RegisterAndApproveAsync(client, username, oldPassword);
        AuthResponse auth = await LoginAsync(client, username, oldPassword);

        HttpResponseMessage change = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/users/me/change-password", auth.AccessToken,
                new { currentPassword = oldPassword, newPassword }));
        change.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage staleRefresh = await client.PostAsJsonAsync(
            $"{Root}/auth/refresh",
            new { refreshToken = auth.RefreshToken });
        staleRefresh.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        HttpResponseMessage oldPasswordLogin = await client.PostAsJsonAsync(
            $"{Root}/auth/login",
            new { username, password = oldPassword });
        oldPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        HttpResponseMessage newPasswordLogin = await client.PostAsJsonAsync(
            $"{Root}/auth/login",
            new { username, password = newPassword });
        newPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdmin_CallingAdminEndpoint_Returns403()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";
        string password = "applicant-password-0123";

        _ = await RegisterAndApproveAsync(client, username, password);
        AuthResponse auth = await LoginAsync(client, username, password);

        HttpResponseMessage response = await client.SendAsync(
            Authorized(HttpMethod.Get, $"{Root}/applications", auth.AccessToken));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApiKey_OnGeneralEndpoint_IsRejected()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";
        string password = "applicant-password-0123";

        _ = await RegisterAndApproveAsync(client, username, password);
        AuthResponse auth = await LoginAsync(client, username, password);

        HttpResponseMessage create = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/users/me/api-keys", auth.AccessToken, new { name = "tooling" }));
        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        CreateApiKeyResponse apiKey = (await create.Content.ReadFromJsonAsync<CreateApiKeyResponse>())!;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{Root}/users/me");
        request.Headers.Add("X-Api-Key", apiKey.Key);
        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DuplicateUsername_Returns409()
    {
        HttpClient client = factory.CreateClient();
        string username = $"user-{Guid.NewGuid():N}";

        await RegisterAsync(client, username, "applicant-password-0123");

        HttpResponseMessage duplicate = await client.PostAsJsonAsync($"{Root}/applications", new
        {
            username,
            email = $"different-{Guid.NewGuid():N}@crowdnfo.test",
            password = "applicant-password-0123",
            applicationData = "{}"
        });

        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    private async Task<Guid> RegisterAndApproveAsync(HttpClient client, string username, string password)
    {
        int applicationId = await RegisterAsync(client, username, password);
        string adminToken = (await LoginAsync(client, CrowdNfoApiFactory.AdminUsername, CrowdNfoApiFactory.AdminPassword)).AccessToken;

        HttpResponseMessage approve = await client.SendAsync(
            Authorized(HttpMethod.Post, $"{Root}/applications/{applicationId}/approve", adminToken, new { comment = "ok" }));
        approve.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        UserApplication application = await dbContext.UserApplications.SingleAsync(a => a.Id == applicationId);

        return application.UserId!.Value;
    }

    private static async Task<int> RegisterAsync(HttpClient client, string username, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"{Root}/applications", new
        {
            username,
            email = $"{username}@crowdnfo.test",
            password,
            applicationData = "{}"
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        RegisterResponse? application = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        application.ShouldNotBeNull();

        return application.Id;
    }

    private static async Task<AuthResponse> LoginAsync(HttpClient client, string username, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"{Root}/auth/login", new { username, password });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        AuthResponse? auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.ShouldNotBeNull();

        return auth;
    }

    private static async Task<int> FindApplicationIdAsync(HttpClient client, string adminToken, string username)
    {
        HttpResponseMessage response = await client.SendAsync(Authorized(HttpMethod.Get, $"{Root}/applications", adminToken));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        List<ApplicationResponse>? applications = await response.Content.ReadFromJsonAsync<List<ApplicationResponse>>();
        applications.ShouldNotBeNull();

        return applications.Single(application => application.Username == username).Id;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }
}
