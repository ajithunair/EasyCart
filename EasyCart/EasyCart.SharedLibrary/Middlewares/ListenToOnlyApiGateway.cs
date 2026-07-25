using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await next(context);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await next(context);
                return;
            }

            var signedHeader = context.Request.Headers["X-Api-Gateway"].FirstOrDefault();
            if (signedHeader is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable: This endpoint is accessible only through the API Gateway.");
                return;
            }
            await next(context);
        }
    }
}
