using EasyCart.AuthApi.Data;
using EasyCart.AuthApi.DTOs;
using EasyCart.AuthApi.Entities;
using EasyCart.AuthApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        public async Task<AuthResponseDto> Login(LoginDto loginDto, string? ipAddress, string? userAgent)
        {
            var user = await GetUserByEmail(loginDto.Email);
            if (user is null)
            {
                return new AuthResponseDto(false, "Invalid credentials", null);
            }

            bool passwordCheck = BCrypt.Net.BCrypt.Verify(loginDto.Password,user.Password);
            if (!passwordCheck)
            {
                return new AuthResponseDto(false, "Invalid credentials", null);
            }

            var accessToken = GenerateAccessToken(user);
            var rawrRefreshToken = CreateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashRefreshToken(rawrRefreshToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(2),
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            };

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();


            return new AuthResponseDto(
                true,
                "Login successful",
                new TokenPair(
                    accessToken,
                    rawrRefreshToken,
                    DateTime.UtcNow.AddMinutes(5)
                )
            );
        }

        public async Task<AuthResponseDto?> Refresh(
            string rawRefreshToken,
            string? ipAddress,
            string? userAgent)
        {
            var tokenHash = HashRefreshToken(rawRefreshToken);

            var storedToken = await context.RefreshTokens
                .Include(token => token.User)
                .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

            if (storedToken is null || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
                return null;

            if (storedToken.RevokedAtUtc is not null)
            {
                await RevokeAllUserTokens(storedToken.UserId);
                return null;
            }

            var now = DateTime.UtcNow;
            var newRawRefreshToken = CreateRefreshToken();
            var newHash = HashRefreshToken(newRawRefreshToken);

            storedToken.RevokedAtUtc = now;
            storedToken.ReplacedByTokenHash = newHash;

            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = storedToken.UserId,
                TokenHash = newHash,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(2),
                CreatedByIp = ipAddress,
                UserAgent = userAgent
            });

            await context.SaveChangesAsync();

            return new AuthResponseDto(
                true,
                "Token refreshed",
                new TokenPair(
                    GenerateAccessToken(storedToken.User),
                    newRawRefreshToken,
                    now.AddMinutes(5)));
        }

        public async Task Logout(string rawRefreshToken)
        {
            var tokenHash = HashRefreshToken(rawRefreshToken);
            var storedToken = await context.RefreshTokens
                .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

            if (storedToken is null || storedToken.RevokedAtUtc is not null)
                return;

            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        private async Task RevokeAllUserTokens(int userId)
        {
            var activeTokens = await context.RefreshTokens
                .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
                .ToListAsync();

            foreach (var token in activeTokens)
                token.RevokedAtUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();
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
                // Public registration always creates a customer. Admin accounts must be provisioned separately.
                Role = "Customer",
                Password=BCrypt.Net.BCrypt.HashPassword(appUserDto.Password)
            });

            await context.SaveChangesAsync();

            return result.Entity.Id > 0 ? new Response(true, "User Registered") :
                new Response(false, "Invalid data provided");
        }

        private string GenerateAccessToken(AppUser user)
        {
            var secretKey = config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

            var issuer = config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is not configured.");

            var audience = config["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var key = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            if (!string.IsNullOrWhiteSpace(user.Role))
            {
                claims.Add(new(ClaimTypes.Role, user.Role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(hash);
        }
    }
}
