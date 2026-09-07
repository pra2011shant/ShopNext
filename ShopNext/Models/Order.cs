using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Orders
    /// Description: Core customer order transactions, fulfillment states, delivery OTP tracking, and return/dispute workflows.
    /// </summary>
    [Table("Orders")]
    public class Order : BaseModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ShopId { get; set; }
        public int? RiderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public string PaymentMode { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";
        public string? CancelReason { get; set; }
        public string? ReturnReason { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? DeliveryAddress { get; set; }

        // Point 34: Return Workflow & Swap Protection
        public string? ReturnStatus { get; set; } // Return_Requested, Under_Verification, Approved, Rejected, Product_Swapped_Fraud
        public string? ReturnVerificationNotes { get; set; }
        public DateTime? ReturnRequestedDate { get; set; }
        public DateTime? ReturnVerifiedDate { get; set; }

        // Point 41: Customer Delivery OTP & Handover Verification
        public string? DeliveryOtp { get; set; }
        public bool IsOtpVerified { get; set; } = false;
        public DateTime? OtpVerifiedDate { get; set; }
        public string? DeliveredByRiderName { get; set; }

        // Additional display properties
        public string? ShopName { get; set; }

        // Navigation Properties
        public User? Customer { get; set; }
        public Shop? Shop { get; set; }
        public Rider? Rider { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
