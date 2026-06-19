using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.Interfaces;
using EasyCart.OrderApi.Repositories;
using EasyCart.OrderApi.Services;
using EasyCart.SharedLibrary.DependencyInjection;
using EasyCart.SharedLibrary.Logs;
using Polly;
using Polly.Retry;

namespace EasyCart.OrderApi.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddOrderApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSharedServices<OrderDbContext>(configuration, configuration["Serilog:FileName"]!);
            services.AddScoped<IOrder, OrderRepository>();

            //register HttpClient for OrderApi 
            services.AddHttpClient<IOrderService, OrderService>(options =>
            {
                options.BaseAddress = new Uri(configuration["ApiGateway:BaseUrl"]!);
                options.Timeout = TimeSpan.FromSeconds(5);
            });

            //create retry strategy
            var retryStrategy =  new RetryStrategyOptions()
            {
                MaxRetryAttempts = 3,
                ShouldHandle = new PredicateBuilder().Handle<TaskCanceledException>(),
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromMicroseconds(500),
                UseJitter = true,
                OnRetry = args =>
                {
                    string message = $"Retrying... Attempt {args.AttemptNumber}. Outcome: {args.Outcome}";
                    LogException.LogToConsole(message);
                    LogException.LogToDebugger(message);
                    return ValueTask.CompletedTask;
                }
            };

            //Use retry strategy
            services.AddResiliencePipeline("my-retry-pipeline", builder =>
            {
                builder.AddRetry(retryStrategy);
            });

            return services;
        }

        public static WebApplication UserOrderApiMiddlewares(this WebApplication app)
        {
            app.UseSharedServices();
            return app;
        }
    }
}
