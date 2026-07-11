namespace EasyCart.ApiGateway.Middlewares
{
    // Backward-compatible shim so existing Program.cs references keep working without a config-only change.
    public class AttachApiGatewaySignarureToRequest(RequestDelegate next) : AttachApiGatewaySignatureToRequest(next)
    {
    }
}
