using Microsoft.OpenApi;

namespace StudentApi.Backend.Extensions;

/// <summary>
/// OpenAPI document generation with a Bearer (JWT) security scheme so the
/// generated document/scalar UI knows the API expects an Authorization header.
/// </summary>
public static class OpenApiExtensions
{
    public static void AddOpenApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,

                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                };

                var components = document.Components ??= new OpenApiComponents();
                components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = securityScheme
                };

                var reference = new OpenApiSecuritySchemeReference("Bearer", document, null);
                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [reference] = new List<string>()
                });

                return Task.CompletedTask;
            });
        });
    }
}
