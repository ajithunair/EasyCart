using EasyCart.AuthApi.DTOs;
using EasyCart.SharedLibrary.Responses;

namespace EasyCart.AuthApi.Interfaces
{
    public interface IUser
    {
        Task<Response> Register(AppUserDto appUserDto);
        Task<AuthResponseDto> Login(LoginDto loginDto, string? ipAddress, string? userAgent);
        Task<AuthResponseDto?> Refresh(string refreshToken, string? ipAddress, string? userAgent);
        Task Logout(string refreshToken);
        Task<GetUserDto> GetUser(int  userId);
    }
}
