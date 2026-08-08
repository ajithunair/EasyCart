namespace EasyCart.AuthApi.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
        public string TokenHash { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public bool IsActive => RevokedAtUtc == null && ExpiresAtUtc > DateTime.UtcNow;
    }
}
