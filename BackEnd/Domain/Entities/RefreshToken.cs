using OnlineShop.Domain.Common;


namespace OnlineShop.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        private RefreshToken() { }

        public string Token { get; private set; } = string.Empty;               // خود RefreshToken
        public string AccessToken { get; private set; } = string.Empty;          // AccessToken مرتبط
        public DateTime AccessTokenExpiryDate { get; private set; }              // تاریخ انقضای AccessToken
        public DateTime ExpiryDate { get; private set; }                          // تاریخ انقضای RefreshToken
        public bool IsRevoked { get; private set; } = false;                       // وضعیت باطل شدن
        public string? ReplacedByToken { get; private set; }                     // اگر rotate شد، توکن جدید

        public string CreatedByIp { get; private set; } = string.Empty;        // IP ایجاد کننده
        public string? CreatedByUserAgent { get; private set; }                  // User-Agent / Device

        public int UserId { get; private set; }
        public User User { get; private set; }

        // ===== Factory Method =====
        public static RefreshToken Create(
            string token,
            string accessToken,
            DateTime accessTokenExpiry,
            DateTime expiry,
            int userId,
            string createdByIp,
            string? createdByUserAgent,
            int currentUserId)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                AccessToken = accessToken,
                AccessTokenExpiryDate = accessTokenExpiry,
                ExpiryDate = expiry,
                UserId = userId,
                CreatedByIp = createdByIp,
                CreatedByUserAgent = createdByUserAgent
            };
            refreshToken.MarkCreated(currentUserId);
            return refreshToken;
        }

        // ===== Behavior Methods =====
        public void Revoke(string? replacedByToken, int currentUserId)
        {
            if (IsRevoked)
                throw new DomainException("این نشست قبلاً باطل شده است.");

            IsRevoked = true;
            ReplacedByToken = replacedByToken;
            MarkUpdated(currentUserId);
        }

        public bool IsActive() => !IsRevoked && DateTime.UtcNow < ExpiryDate;
    }
}
