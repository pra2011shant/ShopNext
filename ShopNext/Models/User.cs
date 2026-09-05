using System;

namespace ShopNext.Models
{
    public class User : BaseModel
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; } = "Customer";
        public string? ProfilePhoto { get; set; }
        public string? Gender { get; set; } // Male, Female, Other
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Point 36: Suspicious Customer Detection & Risk Scoring
        public int RiskScore { get; set; } = 15; // 0-100 score
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
        public string? RiskFactorsJson { get; set; } // JSON list of active risk factor reasons
        public bool IsCodDisabled { get; set; } = false; // Admin flag to disable Cash on Delivery
        public bool IsFlaggedForReview { get; set; } = false; // Flagged for manual order inspection
        public DateTime? RiskLastEvaluatedDate { get; set; }
    }
}
