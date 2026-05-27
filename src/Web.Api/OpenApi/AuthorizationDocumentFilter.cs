using System.Text.RegularExpressions;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.OpenApi;

internal sealed partial class AuthorizationDocumentFilter : IDocumentFilter
{
    public const string BearerSchemeName = "Bearer";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (ApiDescription apiDescription in context.ApiDescriptions)
        {
            if (apiDescription.RelativePath is not { } relativePath ||
                apiDescription.HttpMethod is not { } httpMethod)
            {
                continue;
            }

            string pathKey = $"/{RouteConstraint().Replace(relativePath, "{$1}")}";

            if (!swaggerDoc.Paths.TryGetValue(pathKey, out IOpenApiPathItem? pathItem) ||
                pathItem.Operations is not { } operations ||
                !operations.TryGetValue(HttpMethod.Parse(httpMethod), out OpenApiOperation? operation))
            {
                continue;
            }

            IList<object> endpointMetadata = apiDescription.ActionDescriptor.EndpointMetadata;
            bool allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
            IAuthorizeData[] authorizeData = [.. endpointMetadata.OfType<IAuthorizeData>()];

            if (allowsAnonymous || authorizeData.Length == 0)
            {
                operation.Summary = WithBadge("Public", operation.Summary);
                continue;
            }

            bool requiresAdmin = authorizeData.Any(data => data.Policy == AuthorizationPolicies.Admin);
            operation.Summary = WithBadge(requiresAdmin ? "Admin" : "Auth", operation.Summary);

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeName, swaggerDoc, null)] = []
            });
        }
    }

    private static string WithBadge(string label, string? summary) =>
        string.IsNullOrEmpty(summary) ? $"[{label}]" : $"[{label}] {summary}";

    [GeneratedRegex(@"\{(\w+):[^}]+\}")]
    private static partial Regex RouteConstraint();
}
