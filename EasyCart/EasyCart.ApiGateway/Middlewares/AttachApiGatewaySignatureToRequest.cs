namespace EasyCart.ApiGateway.Middlewares
{
    public class AttachApiGatewaySignatureToRequest(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // The downstream services use this marker to confirm the request passed through the gateway.
            context.Request.Headers["X-Api-Gateway"] = "Signed";
            await next(context);
        }
    }
}
