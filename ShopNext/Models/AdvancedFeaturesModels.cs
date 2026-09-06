using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    // ==========================================
    // POINT 81: PRODUCT COMPARISON
    // ==========================================
    public class ProductComparisonItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? Mrp { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int Stock { get; set; }
        public string StockStatus { get; set; } = "In Stock";
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    }

    public class ProductComparisonViewModel
    {
        public List<ProductComparisonItemDto> Products { get; set; } = new List<ProductComparisonItemDto>();
        public List<string> AllSpecificationKeys { get; set; } = new List<string>();
        public int BestPriceProductId { get; set; }
        public int HighestRatingProductId { get; set; }
    }

    // ==========================================
    // POINT 82: RECENTLY SEARCHED PRODUCTS
    // ==========================================
    [Table("SearchHistories")]
    public class SearchHistory : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        [Required]
        [StringLength(200)]
        public string SearchTerm { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public int ResultCount { get; set; }
        public string? ClientIp { get; set; }
    }

    public class SearchHistoryDto
    {
        public int Id { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string SearchedAt { get; set; } = string.Empty;
    }

    // ==========================================
    // POINT 83: SMART RECOMMENDATIONS
    // ==========================================
    public class SmartRecommendationsDto
    {
        public List<Product> CustomersAlsoBought { get; set; } = new List<Product>();
        public List<Product> YouMayAlsoLike { get; set; } = new List<Product>();
        public List<Product> SimilarProducts { get; set; } = new List<Product>();
    }

    // ==========================================
    // POINT 84: PRODUCT Q&A
    // ==========================================
    [Table("ProductQuestions")]
    public class ProductQuestion : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = true;
        public int Upvotes { get; set; } = 0;

        public virtual ICollection<ProductAnswer> Answers { get; set; } = new List<ProductAnswer>();
    }

    [Table("ProductAnswers")]
    public class ProductAnswer : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public virtual ProductQuestion? Question { get; set; }

        public int ResponderId { get; set; }
        public virtual User? Responder { get; set; }

        [Required]
        [StringLength(2000)]
        public string AnswerText { get; set; } = string.Empty;

        [StringLength(50)]
        public string ResponderRole { get; set; } = "Seller"; // Seller, Admin, VerifiedBuyer

        public bool IsVerifiedSellerAnswer { get; set; } = true;
        public int HelpfulCount { get; set; } = 0;
    }

    public class ProductQnADto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AskedBy { get; set; } = string.Empty;
        public string AskedDate { get; set; } = string.Empty;
        public int Upvotes { get; set; }
        public List<ProductAnswerDto> Answers { get; set; } = new List<ProductAnswerDto>();
    }

    public class ProductAnswerDto
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string AnsweredBy { get; set; } = string.Empty;
        public string ResponderRole { get; set; } = "Seller";
        public bool IsVerifiedSeller { get; set; }
        public string AnsweredDate { get; set; } = string.Empty;
        public int HelpfulCount { get; set; }
    }

    // ==========================================
    // POINT 85: SELLER CHAT / CUSTOMER SUPPORT CHAT
    // ==========================================
    [Table("ChatMessages")]
    public class ChatMessage : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(100)]
        public string ThreadId { get; set; } = string.Empty; // e.g. "ORD-1001-CHAT" or "PROD-25-CUST-10"

        public int SenderId { get; set; }
        public virtual User? Sender { get; set; }

        [StringLength(50)]
        public string SenderRole { get; set; } = "Customer"; // Customer, Seller, Admin, Support

        public int? RecipientId { get; set; }
        public virtual User? Recipient { get; set; }

        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int? ShopId { get; set; }
        public virtual Shop? Shop { get; set; }

        [Required]
        [StringLength(2000)]
        public string MessageText { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string ThreadId { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = "Customer";
        public string MessageText { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string Timestamp { get; set; } = string.Empty;
        public bool IsFromCurrentUser { get; set; }
        public bool IsRead { get; set; }
    }

    // ==========================================
    // POINT 86 & 87: PRICE DROP & BACK-IN-STOCK ALERTS
    // ==========================================
    [Table("PriceDropAlerts")]
    public class PriceDropAlert : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubscribedPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetPrice { get; set; }

        public bool IsNotified { get; set; } = false;
        public DateTime? NotifiedAt { get; set; }
        public decimal? TriggerPrice { get; set; }
    }

    [Table("StockAlerts")]
    public class StockAlert : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [StringLength(200)]
        public string? CustomerEmail { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public bool IsNotified { get; set; } = false;
        public DateTime? NotifiedAt { get; set; }
    }

    // ==========================================
    // POINT 89: GIFT CARD & DIGITAL WALLET
    // ==========================================
    [Table("WalletAccounts")]
    public class WalletAccount : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MainBalance { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundWalletBalance { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiftCardBalance { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBalance => MainBalance + RefundWalletBalance + GiftCardBalance;

        public bool IsLocked { get; set; } = false;
        public string? LockReason { get; set; }

        public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }

    [Table("WalletTransactions")]
    public class WalletTransaction : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int WalletAccountId { get; set; }
        public virtual WalletAccount? WalletAccount { get; set; }

        [StringLength(50)]
        public string TransactionType { get; set; } = "Credit"; // Credit, Debit

        [StringLength(50)]
        public string SourceCategory { get; set; } = "Refund"; // Refund, GiftCard, Cashback, OrderPayment, Topup

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? RelatedOrderId { get; set; }
        public virtual Order? RelatedOrder { get; set; }

        [StringLength(100)]
        public string? ReferenceCode { get; set; }
    }

    [Table("GiftCards")]
    public class GiftCard : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CardCode { get; set; } = string.Empty; // e.g. "GIFT-2026-X89Y"

        [StringLength(20)]
        public string Pin { get; set; } = "1234";

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddYears(1);
        public bool IsRedeemed { get; set; } = false;
        public int? RedeemedByCustomerId { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }

    public class WalletDashboardViewModel
    {
        public decimal MainBalance { get; set; }
        public decimal RefundWalletBalance { get; set; }
        public decimal GiftCardBalance { get; set; }
        public decimal TotalBalance { get; set; }
        public int RewardPoints { get; set; }
        public decimal RewardPointsCashValue { get; set; }
        public List<WalletTransactionDto> RecentTransactions { get; set; } = new List<WalletTransactionDto>();
    }

    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public string TransactionType { get; set; } = "Credit";
        public string SourceCategory { get; set; } = "Refund";
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DateFormatted { get; set; } = string.Empty;
        public string? ReferenceCode { get; set; }
    }

    // ==========================================
    // POINT 90: LOYALTY / REWARD POINTS
    // ==========================================
    [Table("RewardPoints")]
    public class RewardPointsAccount : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual User? Customer { get; set; }

        public int CurrentPoints { get; set; } = 0;
        public int LifetimeEarnedPoints { get; set; } = 0;
        public int LifetimeRedeemedPoints { get; set; } = 0;

        [StringLength(50)]
        public string Tier { get; set; } = "Silver"; // Silver, Gold, Platinum

        public virtual ICollection<RewardPointsTransaction> Transactions { get; set; } = new List<RewardPointsTransaction>();
    }

    [Table("RewardPointsTransactions")]
    public class RewardPointsTransaction : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RewardPointsAccountId { get; set; }
        public virtual RewardPointsAccount? RewardPointsAccount { get; set; }

        [StringLength(50)]
        public string TransactionType { get; set; } = "Earned"; // Earned, Redeemed, Expired, Adjusted

        public int Points { get; set; }
        public int PointsBalanceAfter { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? RelatedOrderId { get; set; }
        public virtual Order? RelatedOrder { get; set; }
    }

    // ==========================================
    // POINT 94: STOCK RESERVATION (10-Min Lock)
    // ==========================================
    [Table("StockReservations")]
    public class StockReservation : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ReservationToken { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; } = 1;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddMinutes(10);
        public bool IsCommitted { get; set; } = false;
        public bool IsReleased { get; set; } = false;
    }

    // ==========================================
    // POINT 96: GST & TAX CALCULATION
    // ==========================================
    public class TaxBreakdownDto
    {
        public decimal SubTotal { get; set; }
        public decimal GstRatePercent { get; set; } = 18.0m;
        public bool IsInterstate { get; set; } = false;
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string HsnCode { get; set; } = "8517";
        public string ShopGstin { get; set; } = "10AAAAA0000A1Z5";
    }

    // ==========================================
    // POINT 97 & 98: DELIVERY SLOTS & SERVICEABILITY
    // ==========================================
    [Table("PincodeServiceabilities")]
    public class PincodeServiceability : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = "Patna";

        [StringLength(100)]
        public string State { get; set; } = "Bihar";

        public bool IsServiceable { get; set; } = true;
        public bool IsCodAvailable { get; set; } = true;
        public int EstimatedDeliveryDays { get; set; } = 2;
        public bool IsExpressAvailable { get; set; } = true;
    }

    public class DeliverySlotDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string DateLabel { get; set; } = string.Empty; // e.g. "Tomorrow, 07 Sep"
        public string TimeWindow { get; set; } = string.Empty; // "07:00 AM - 11:00 AM (Morning)"
        public string SlotCategory { get; set; } = "Morning"; // Morning, Afternoon, Evening
        public bool IsAvailable { get; set; } = true;
        public decimal ExtraSlotFee { get; set; } = 0m;
    }

    // ==========================================
    // POINT 99: FAILED DELIVERY MANAGEMENT
    // ==========================================
    [Table("FailedDeliveryLogs")]
    public class FailedDeliveryLog : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int? RiderId { get; set; }
        public virtual Rider? Rider { get; set; }

        public int AttemptNumber { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string FailureReason { get; set; } = "Customer Unavailable"; // Customer Unavailable, Wrong Address, Customer Refused, Phone Unreachable

        [StringLength(1000)]
        public string? RiderRemarks { get; set; }

        [StringLength(500)]
        public string? DoorstepPhotoUrl { get; set; }

        public double? GeoLatitude { get; set; }
        public double? GeoLongitude { get; set; }

        [StringLength(50)]
        public string NextAction { get; set; } = "Auto Re-Attempt Scheduled"; // Auto Re-Attempt Scheduled, Return to Merchant, Admin Review

        public DateTime? RescheduledDate { get; set; }
    }

    // ==========================================
    // POINT 100: PROOF OF DELIVERY (POD)
    // ==========================================
    [Table("DeliveryProofs")]
    public class DeliveryProof : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int? RiderId { get; set; }
        public virtual Rider? Rider { get; set; }

        [StringLength(10)]
        public string HandoverOtp { get; set; } = string.Empty;

        public bool IsOtpVerified { get; set; } = true;
        public DateTime DeliveryTimestamp { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? SignatureOrPhotoUrl { get; set; }

        public double? HandoverLatitude { get; set; }
        public double? HandoverLongitude { get; set; }

        [StringLength(100)]
        public string ReceivedByName { get; set; } = string.Empty;
    }

    // ==========================================
    // POINT 92: PAYMENT RECONCILIATION
    // ==========================================
    public class PaymentReconciliationDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal OrderTotalAmount { get; set; }
        public decimal GatewayReceivedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal NetSettlementAmount { get; set; }
        public string PaymentMode { get; set; } = "UPI";
        public string PaymentStatus { get; set; } = "Success";
        public string TransactionId { get; set; } = string.Empty;
        public string ReconciliationStatus { get; set; } = "Matched"; // Matched, Discrepancy, Pending, Overpaid
        public decimal DiscrepancyAmount { get; set; } = 0m;
        public string OrderDate { get; set; } = string.Empty;
    }

    public class PaymentReconciliationSummaryDto
    {
        public decimal TotalGrossSales { get; set; }
        public decimal TotalGatewayCaptured { get; set; }
        public decimal TotalRefundsIssued { get; set; }
        public decimal TotalNetSettled { get; set; }
        public int TotalReconciledOrders { get; set; }
        public int DiscrepanciesCount { get; set; }
        public List<PaymentReconciliationDto> Records { get; set; } = new List<PaymentReconciliationDto>();
    }
}
