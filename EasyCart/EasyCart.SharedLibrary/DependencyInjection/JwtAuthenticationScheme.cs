using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public static class JwtAuthenticationScheme
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var secretKey = config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

            var issuer = config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is not configured.");

            var audience = config["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is not configured.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Configure JWT Bearer options here (e.g., Authority, Audience, etc.)
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(secretKey))
                };
            });

            services.AddAuthorization();
            return services;
        }
    }
}
