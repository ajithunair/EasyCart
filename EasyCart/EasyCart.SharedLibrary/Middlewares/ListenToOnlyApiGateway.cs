using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EasyCart.SharedLibrary.Middlewares
{
    public class ListenToOnlyApiGateway(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var enabled = configuration.GetValue<bool>("GatewayProtection:Enabled");

            if (!enabled)
            {
                await next(context);
                return;
            }

            // Allow developer tooling and basic service checks to keep working without the gateway header.
            if (context.Request.Path.StartsWithSegments("/swagger") || context.Request.Path.StartsWithSegments("/health"))
            {
                await next(context);
                return;
            }

            var signedHeader = context.Request.Headers["X-Api-Gateway"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signedHeader))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable: This endpoint is accessible only through the API Gateway.");
                return;
            }

            await next(context);
        }
    }
}
