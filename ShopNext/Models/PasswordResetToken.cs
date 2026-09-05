using System;

namespace ShopNext.Models
{
    public class PasswordResetToken : BaseModel
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}