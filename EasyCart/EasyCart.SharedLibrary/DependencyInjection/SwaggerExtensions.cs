using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerWithBearerAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Expose a bearer auth scheme so Swagger UI can send JWTs to protected endpoints.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter a JWT token in the format: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
