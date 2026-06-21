using EasyCart.AuthApi.Data;
using EasyCart.AuthApi.Interfaces;
using EasyCart.AuthApi.Repositories;
using EasyCart.SharedLibrary.DependencyInjection;

namespace EasyCart.AuthApi.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddAuthenticationApiService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSharedServices<AuthenticationDbContext>(config, config["Serilog:FileName"]!); 

            services.AddScoped<IUser, UserRepository>();
            
            return services;  
        }

        public static WebApplication UseAuthenticationService(this WebApplication app)
        {
            app.UseSharedServices();
            return app;
        }
    }
}
