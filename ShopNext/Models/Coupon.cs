using System;

namespace ShopNext.Models
{
    public class Coupon : BaseModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Flat"; // Flat, Percent, FreeDelivery
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; } = 0;
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int UsageLimit { get; set; } = 500;
        public int UsedCount { get; set; } = 0;
    }
}
