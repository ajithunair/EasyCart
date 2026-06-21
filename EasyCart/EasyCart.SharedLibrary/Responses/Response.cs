namespace EasyCart.SharedLibrary.Responses
{
    public record Response(bool Success = false,
        string Message = null
    );
}
