using EasyCart.SharedLibrary.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            services.AddHealthChecks();

            // Add DbContext with PostgreSQL provider
            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(config.GetConnectionString("EasyCartConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

            var logDirectory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(logDirectory))
                Directory.CreateDirectory(logDirectory);

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

            // Add OpenTelemetry
            services.AddSharedOpenTelemetry(config);

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

            // Add middleware to configure the CorrelationId 
            app.UseMiddleware< TraceMiddleware>();
            return app;
        }

        public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

            try
            {
                var context = scope.ServiceProvider.GetRequiredService<TContext>();

                context.Database.Migrate();

                logger.LogInformation("Database migration completed for {Context}",
                    typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database migration failed for {Context}",
                    typeof(TContext).Name);

                throw;
            }
        }
    }
}
