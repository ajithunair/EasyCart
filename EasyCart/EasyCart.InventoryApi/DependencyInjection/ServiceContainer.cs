using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.Interfaces;
using EasyCart.InventoryApi.Repositories;
using EasyCart.SharedLibrary.DependencyInjection;

namespace EasyCart.InventoryApi.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInventoryApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSharedServices<InventoryDbContext>(configuration, configuration["Serilog:FileName"]!, configuration.GetConnectionString("inventorydb")!);
            services.AddScoped<IInventory, InventoryRepository>();
            return services;
        }

        public static WebApplication UseInventoryApiMiddlewares(this WebApplication app)
        {
            app.UseSharedServices();
            return app;
        }
    }
}
