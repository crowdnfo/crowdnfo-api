using Microsoft.OpenApi;
using Web.Api.OpenApi;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithCustomSchemaIds(this IServiceCollection services)
    {
        services.AddSwaggerGen(static o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            o.AddSecurityDefinition(AuthorizationDocumentFilter.BearerSchemeName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT access token only; the 'Bearer ' prefix is added automatically."
            });

            o.DocumentFilter<AuthorizationDocumentFilter>();
        });

        return services;
    }
}
