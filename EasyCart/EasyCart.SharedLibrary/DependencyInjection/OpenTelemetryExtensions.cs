using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddSharedOpenTelemetry(this IServiceCollection services, IConfiguration config)
        {
            var serviceName = config["OpenTelemetry:ServiceName"];
            var appInsightsConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            services.AddOpenTelemetry()
                .ConfigureResource(resource=>
                resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    /*.AddAzureMonitorTraceExporter(options=>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    })*/
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint=new Uri(config["OpenTelemetry:Endpoint"]!.ToString());
                    });
                });

            return services;
        }
    }
}
