using EasyCart.AuthApi.Data;
using EasyCart.AuthApi.DTOs;
using EasyCart.AuthApi.Entities;
using EasyCart.AuthApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EasyCart.AuthApi.Repositories
{
    public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
    {
        private async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user is null ? null! : user;
        }
        public async Task<GetUserDto> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is null ? null! : new GetUserDto(
                user.Id,
                user.Name,
                user.Email,
                user.Address,
                user.PhoneNumber,
                user.Role
                );
        }

        public async Task<Response> Login(LoginDto loginDto)
        {
            var user = await GetUserByEmail(loginDto.Email);
            if (user is null)
            {
                return new Response(false, "Invalid credentials");
            }

            bool passwordCheck = BCrypt.Net.BCrypt.Verify(loginDto.Password,user.Password);
            if (!passwordCheck)
            {
                return new Response(false, "Invalid credentials");
            }

            string token = GenerateToken(user);
            return new Response(true, token);
        }

        private string GenerateToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!.ToString());
            var securityKey = new SymmetricSecurityKey(key);
            var credentials=new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name)
            };
            if (!string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
            {
                claims.Add(new(ClaimTypes.Role, user.Role));
            }

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Response> Register(AppUserDto appUserDto)
        {
            var user = await GetUserByEmail(appUserDto.Email);
            if (user is not null)
            {
                return new Response(false, "This email is already registered");
            }

            var result = await context.Users.AddAsync(new AppUser()
            {
                Name= appUserDto.Name,
                Email= appUserDto.Email,
                Address= appUserDto.Address,
                PhoneNumber= appUserDto.PhoneNumber,
                Role= appUserDto.Role,
                Password=BCrypt.Net.BCrypt.HashPassword(appUserDto.Password)
            });

            await context.SaveChangesAsync();

            return result.Entity.Id > 0 ? new Response(true, "User Registered") :
                new Response(false, "Invalid data provided");
        }
    }
}
