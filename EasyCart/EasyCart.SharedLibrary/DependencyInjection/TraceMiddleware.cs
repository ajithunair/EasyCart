using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCart.SharedLibrary.DependencyInjection
{
    public class TraceMiddleware(RequestDelegate next)
    {
        private const string HeaderName = "X-Correlation-Id";

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            await next(context);
        }
    }
}
