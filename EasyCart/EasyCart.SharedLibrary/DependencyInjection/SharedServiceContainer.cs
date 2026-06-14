using EasyCart.SharedLibrary.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services, IConfiguration config, string fileName) where TContext : DbContext
        {
            // Register shared services here
            // Example: services.Add

            // Add DbContext with PostgreSQL provider
            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(config.GetConnectionString("EasyCartConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

            //Add serilog logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.File(fileName,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Add JWT Authentication scheme
            services.AddJwtAuthentication(config);

            return services;
        }

        public static IApplicationBuilder UseSharedServices(this IApplicationBuilder app)
        {
            // Use shared services here
            // Example: app.UseMiddleware<YourMiddleware>();

            // Add Global Exception Handling Middleware
            app.UseMiddleware<GlobalException>();

            // Add Middleware to listen only to API Gateway
            app.UseMiddleware<ListenToOnlyApiGateway>();
            return app;
        }
    }
}
