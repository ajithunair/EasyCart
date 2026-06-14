using EasyCart.ProductApi.Data;
using EasyCart.ProductApi.Interfaces;
using EasyCart.ProductApi.Repositories;
using EasyCart.SharedLibrary.DependencyInjection;

namespace EasyCart.ProductApi.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddProductApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<ProductDbContext>(services, configuration, configuration["Serilog:FileName"]);
            services.AddScoped<IProduct, ProductRepository>();
            return services;
        }

        public static WebApplication UseProductApiMiddlewares(this WebApplication app)
        {
            app.UseSharedServices();
            return app;
        }
    }
}
