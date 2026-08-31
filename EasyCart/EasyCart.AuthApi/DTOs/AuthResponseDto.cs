namespace EasyCart.AuthApi.DTOs
{
    public record AuthResponseDto
    (
        bool Success,
        string Message,
        TokenPair? TokenPair 
    );

    public record TokenPair(
        string AccessToken,
        [property: System.Text.Json.Serialization.JsonIgnore]
        string RefreshToken,
        DateTime AccessTokenExpiresAtUtc
    );
}
