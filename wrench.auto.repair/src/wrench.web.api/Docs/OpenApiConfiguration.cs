using Microsoft.OpenApi;

namespace wrench.web.api.Docs
{
    public static class OpenApiConfiguration
    {
        public static void ConfigureOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, ct) =>
                {
                    document.Components ??= new();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Description = "Bearer {token}"
                    };

                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, context, ct) =>
                {
                    var hasAuthorize =
                        context.Description.ActionDescriptor.EndpointMetadata
                            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                            .Any();

                    if (!hasAuthorize)
                        return Task.CompletedTask;

                    operation.Security ??= [];

                    var scheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    };

                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer")] = []
                    });

                    return Task.CompletedTask;
                });
            });
        }
    }
}
