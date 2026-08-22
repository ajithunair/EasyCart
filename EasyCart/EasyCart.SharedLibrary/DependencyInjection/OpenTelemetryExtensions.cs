using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddSharedOpenTelemetry(this IServiceCollection services, IConfiguration config)
        {
            var serviceName = config["OpenTelemetry:ServiceName"] ?? "EasyCart";
            var endpoint = config["OpenTelemetry:Endpoint"];
            var appInsightsConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            services.AddOpenTelemetry()
                .ConfigureResource(resource=>
                resource.AddService(serviceName))
                .WithMetrics(metrics =>
                {
                    metrics
                    .AddHttpClientInstrumentation()
                    // Instruments built-in Kestrel and ASP.NET Core metrics
                    .AddAspNetCoreInstrumentation()
                    ;
                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        metrics.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
                    }
                    if (!string.IsNullOrEmpty(appInsightsConnectionString))
                    {
                        metrics.AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnectionString;
                        });
                    }
                })
                .WithTracing(tracing =>
                {
                    tracing
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        tracing.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
                    }

                    if (!string.IsNullOrEmpty(appInsightsConnectionString))
                    {

                        tracing.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnectionString;
                        });
                    }
                });

            return services;
        }
    }
}
