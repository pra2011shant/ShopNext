using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.PasswordResetTokens
    /// Description: Cryptographically hashed time-limited authentication tokens for secure self-service password resets.
    /// </summary>
    [Table("PasswordResetTokens")]
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