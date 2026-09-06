using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IAdvancedEcommerceService
    {
        // 81. Product Comparison
        Task<ProductComparisonViewModel> GetProductComparisonAsync(int[] productIds);

        // 82. Recently Searched Products
        Task<List<SearchHistoryDto>> GetSearchHistoryAsync(int? customerId, string? clientIp, int take = 10);
        Task RecordSearchAsync(string term, int? customerId, string? category, int resultCount, string? clientIp);
        Task ClearSearchHistoryAsync(int? customerId, string? clientIp);

        // 83. Smart Recommendations
        Task<SmartRecommendationsDto> GetSmartRecommendationsAsync(int productId, int take = 6);

        // 84. Product Q&A
        Task<List<ProductQnADto>> GetProductQuestionsAsync(int productId);
        Task<ProductQuestion> AskQuestionAsync(int productId, int customerId, string question);
        Task<ProductAnswer> AnswerQuestionAsync(int questionId, int responderId, string responderRole, string answer);
        Task UpvoteQuestionAsync(int questionId);
        Task MarkAnswerHelpfulAsync(int answerId);

        // 85. Seller & Customer Support Chat
        Task<List<ChatMessageDto>> GetChatMessagesAsync(string threadId, int currentUserId);
        Task<ChatMessage> SendChatMessageAsync(string threadId, int senderId, string senderRole, string message, int? recipientId = null, int? orderId = null, int? productId = null, int? shopId = null, string? attachmentUrl = null);

        // 86 & 87. Price Drop & Back-in-Stock Alerts
        Task<PriceDropAlert> SubscribePriceDropAsync(int customerId, int productId, decimal currentPrice, decimal? targetPrice = null);
        Task<StockAlert> SubscribeStockAlertAsync(int customerId, int productId, string? email = null, string? phone = null);

        // 88. Coupon Validation Engine
        Task<(bool IsValid, string Message, decimal DiscountAmount, Coupon? Coupon)> ValidateCouponAsync(string couponCode, decimal orderAmount, int customerId, List<int>? productIds = null, List<string>? categoryNames = null);

        // 89. Gift Card & Digital Wallet
        Task<WalletDashboardViewModel> GetWalletDashboardAsync(int customerId);
        Task<(bool Success, string Message, decimal RedeemedAmount)> RedeemGiftCardAsync(int customerId, string cardCode, string pin);
        Task<WalletTransaction> CreditWalletAsync(int customerId, decimal amount, string category, string description, int? orderId = null);
        Task<(bool Success, string Message, WalletTransaction? Txn)> DebitWalletAsync(int customerId, decimal amount, string description, int? orderId = null);

        // 90. Loyalty / Reward Points
        Task<RewardPointsAccount> GetRewardPointsAccountAsync(int customerId);
        Task<int> AwardRewardPointsForOrderAsync(int customerId, int orderId, decimal orderAmount);
        Task<(bool Success, string Message, decimal DiscountValue)> RedeemRewardPointsAsync(int customerId, int pointsToRedeem, int orderId);

        // 91. Payment Failure Recovery
        Task<Order?> GetFailedPaymentOrderAsync(int orderId, int customerId);
        Task<(bool Success, string Message)> RetryOrderPaymentAsync(int orderId, string paymentMode, string transactionId, decimal paidAmount);

        // 92. Payment Reconciliation
        Task<PaymentReconciliationSummaryDto> GetPaymentReconciliationSummaryAsync();
        Task<List<PaymentReconciliationDto>> GetPaymentReconciliationListAsync(DateTime? fromDate = null, DateTime? toDate = null, string? status = null);

        // 93 & 94. Stock Reservation & Idempotency
        Task<(bool Success, string Token, string Message)> ReserveStockForCheckoutAsync(int customerId, List<(int ProductId, int Quantity)> items);
        Task CommitStockReservationAsync(string token);
        Task ReleaseStockReservationAsync(string token);

        // 95 & 96. GST & Tax Calculation
        TaxBreakdownDto CalculateTax(decimal subtotal, string shopState, string customerState, string category = "General");
        Task<TaxBreakdownDto> GetOrderTaxInvoiceDetailsAsync(int orderId);

        // 97 & 98. Delivery Slots & Pincode Serviceability
        Task<List<DeliverySlotDto>> GetAvailableDeliverySlotsAsync(string pincode);
        Task<PincodeServiceability> CheckPincodeServiceabilityAsync(string pincode);

        // 99. Failed Delivery Management
        Task<FailedDeliveryLog> LogFailedDeliveryAttemptAsync(int orderId, int? riderId, string reason, string? remarks = null, string? photoUrl = null, double? lat = null, double? lng = null);
        Task<List<FailedDeliveryLog>> GetFailedDeliveryLogsAsync(string? status = null, int? orderId = null);
        Task<bool> RescheduleFailedDeliveryAsync(int failedLogId, DateTime newDate, string newSlot, string? notes = null);

        // 100. Proof of Delivery (POD)
        Task<DeliveryProof> RecordProofOfDeliveryAsync(int orderId, int? riderId, string otp, string? proofPhotoUrl = null, double? lat = null, double? lng = null, string? receivedByName = null);
        Task<DeliveryProof?> GetProofOfDeliveryAsync(int orderId);

        // Special 1 & 2: Audit Logs & Customer Risk Engine
        Task<List<AuditLog>> GetAuditLogsAsync(int limit = 100, string? module = null, string? userEmail = null);
        Task RecordAuditLogAsync(int? userId, string userName, string? userEmail, string role, string action, string module, string targetEntity, string? entityId = null, string? details = null, string? ipAddress = null);
    }
}
