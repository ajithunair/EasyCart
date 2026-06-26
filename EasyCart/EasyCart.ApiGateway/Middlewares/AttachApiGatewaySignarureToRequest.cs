namespace EasyCart.ApiGateway.Middlewares
{
    public class AttachApiGatewaySignarureToRequest(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers["X-Api-Gateway"] = "Signed";
            await next(context);
        }
    }
}
