using EasyCart.AuthApi.DTOs;
using EasyCart.SharedLibrary.Responses;

namespace EasyCart.AuthApi.Interfaces
{
    public interface IUser
    {
        Task<Response> Register(AppUserDto appUserDto);
        Task<Response> Login(LoginDto loginDto);
        Task<GetUserDto> GetUser(int  userId);
    }
}
