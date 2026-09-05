using System;

namespace ShopNext.Models
{
    public class OrderItem : BaseModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Point 34: Wrong Product Return & Swap Protection
        public string? Sku { get; set; }
        public string? SerialNumber { get; set; } // Serial Number / IMEI tag
        public string? DispatchCondition { get; set; } = "Brand New / Sealed";
        public bool IsProductVerified { get; set; } = true;
        public bool IsQuantityVerified { get; set; } = true;
        public bool IsSkuVerified { get; set; } = true;
        public bool IsPackagingVerified { get; set; } = true;

        // Inbound Return Inspection
        public string? ReturnReceivedSerial { get; set; }
        public bool? IsReturnSkuMatched { get; set; }
        public bool? IsReturnSerialMatched { get; set; }
        public bool? IsReturnConditionMatched { get; set; }
        public string? ReturnVerificationRemarks { get; set; }

        // Navigation Properties
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
