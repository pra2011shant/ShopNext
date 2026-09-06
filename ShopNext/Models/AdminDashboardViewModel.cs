using System.Collections.Generic;

namespace ShopNext.Models
{
    /// <summary>
    /// Point 31 & 32: Admin Dashboard & Sidebar ViewModel
    /// Encapsulates platform-wide KPIs, full sidebar navigation data (Customers, Sellers, Products,
    /// Categories, Brands, Orders, Payments, Returns, Refunds, Coupons, Offers, Reviews,
    /// Complaints, Reports, Notifications, Settings).
    /// </summary>
    public class AdminDashboardViewModel
    {
        // Core Target Platform KPIs (Point 31)
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; } = 250;
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }

        // Daily Revenue Highlight (Point 31)
        public decimal TodaySales { get; set; }

        // Pending Attention Backlog (Point 31 & 37)
        public int PendingSellerRequests { get; set; } = 10;
        public int PendingProductApprovals { get; set; } = 3;
        public int PendingReturns { get; set; } = 8;
        public int PendingComplaints { get; set; } = 5;

        // Active Collections for Management & Approvals
        public List<Shop> PendingShops { get; set; } = new List<Shop>();
        public List<Shop> ApprovedShops { get; set; } = new List<Shop>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> TopProducts { get; set; } = new List<Product>();

        // Point 32 & 37: Products Management & Approval Tab Dataset
        public List<AdminProductDto> ProductsList { get; set; } = new List<AdminProductDto>();

        // Point 32 & 34: Sellers Management Tab Dataset
        public List<AdminSellerDto> SellersList { get; set; } = new List<AdminSellerDto>();

        // Point 32 & 38: Orders Management Tab Dataset
        public List<AdminOrderDto> OrdersList { get; set; } = new List<AdminOrderDto>();

        // Point 32 Sidebar Tab Datasets
        public List<AdminCustomerDto> CustomersList { get; set; } = new List<AdminCustomerDto>();
        public List<AdminCategoryDto> CategoriesList { get; set; } = new List<AdminCategoryDto>();
        public List<AdminBrandDto> BrandsList { get; set; } = new List<AdminBrandDto>();
        public List<AdminPaymentDto> PaymentsList { get; set; } = new List<AdminPaymentDto>();
        public List<AdminReturnDto> ReturnsList { get; set; } = new List<AdminReturnDto>();
        public List<AdminRefundDto> RefundsList { get; set; } = new List<AdminRefundDto>();
        public List<AdminCouponDto> CouponsList { get; set; } = new List<AdminCouponDto>();
        public List<AdminOfferDto> OffersList { get; set; } = new List<AdminOfferDto>();
        public List<AdminReviewDto> ReviewsList { get; set; } = new List<AdminReviewDto>();
        public List<AdminComplaintDto> ComplaintsList { get; set; } = new List<AdminComplaintDto>();
        public List<AdminNotificationDto> NotificationsList { get; set; } = new List<AdminNotificationDto>();

        // Point 45: Reports & Analytics Suite Dataset
        public AdminReportsViewModel Reports { get; set; } = new AdminReportsViewModel();

        // Point 47: Address Risk Management Dataset
        public List<AdminAddressRiskDto> AddressRiskList { get; set; } = new List<AdminAddressRiskDto>();

        // Point 48: Multiple Account Detection Dataset
        public List<AdminMultiAccountClusterDto> MultiAccountClustersList { get; set; } = new List<AdminMultiAccountClusterDto>();

        // Point 49: COD Abuse Detection Dataset
        public List<AdminCodAbuseDto> CodAbuseList { get; set; } = new List<AdminCodAbuseDto>();

        // Point 50: Sale / Festival Campaign Management Dataset
        public List<AdminSaleCampaignDto> SaleCampaignsList { get; set; } = new List<AdminSaleCampaignDto>();

        // Point 52: Flash Sale Limited Unit Lightning Deals Dataset
        public List<AdminFlashSaleDto> FlashSalesList { get; set; } = new List<AdminFlashSaleDto>();

        // Point 54: Sale Inventory Protection & Concurrency Handling
        public InventoryProtectionDashboardDto InventoryProtection { get; set; } = new InventoryProtectionDashboardDto();

        // Point 55: Seller Sale Participation & Admin Approval Workflow
        public List<AdminSellerSaleParticipationDto> SellerSaleParticipationsList { get; set; } = new List<AdminSellerSaleParticipationDto>();

        // Point 56: Sale Analytics & Post-Campaign Report
        public SaleAnalyticsReportDto SaleAnalytics { get; set; } = new SaleAnalyticsReportDto();
        public List<SaleAnalyticsReportDto> AvailableCampaignReports { get; set; } = new List<SaleAnalyticsReportDto>();
    }

    /// <summary>
    /// Point 37: Product Approval DTO
    /// Workflow: Seller Adds Product -> Admin Review -> Approved / Rejected -> Customer Visibility
    /// Columns: Product Details & SKU, Seller / Shop Name, Category & Brand, Price & MRP, Stock, Status (Approved, Pending, Rejected)
    /// Actions: [Approve], [Reject], [View]
    /// </summary>
    public class AdminProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? SubCategory { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Mrp { get; set; }
        public decimal? Discount { get; set; }
        public int Stock { get; set; }
        public string? Sku { get; set; }
        public string StockStatus { get; set; } = "InStock";
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = true;
        public string ApprovalStatus { get; set; } = "Approved"; // "Pending", "Approved", "Rejected"
        public string? RejectionReason { get; set; }
        public string CreatedDate { get; set; } = "05 Sep 2026";
        public bool HasVariants { get; set; }
        public int VariantCount { get; set; }
    }

    /// <summary>
    /// Point 34: Seller Management DTO
    /// Columns: Shop Name, Owner, Email, Mobile, Status (Approved, Pending, Rejected, Blocked)
    /// Actions: [Approve], [Reject], [Block], [Unblock], [View]
    /// </summary>
    public class AdminSellerDto
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Status { get; set; } = "Approved"; // Approved, Pending, Rejected, Blocked
        public string Category { get; set; } = "Electronics";
        public string Address { get; set; } = "Boring Road, Patna, Bihar";
        public string City { get; set; } = "Patna";
        public int TotalProducts { get; set; } = 120;
        public int TotalOrders { get; set; } = 500;
        public double Rating { get; set; } = 4.5;
        public string RegistrationDate { get; set; } = "15 Jan 2026";
        public string? Remark { get; set; }
    }

    public class AdminCustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Blocked
        public bool IsActive { get; set; } = true;
        public string RegistrationDate { get; set; } = "05 Sep 2026";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string City { get; set; } = "Patna";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string Address { get; set; } = "Boring Road, Patna, Bihar";
        
        // Point 33 Metrics
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal ReturnRate { get; set; }
        public decimal CancellationRate { get; set; }

        // Point 36 Risk Metrics
        public int RiskScore { get; set; } = 15;
        public string RiskLevel { get; set; } = "Low"; // Low (🟢), Medium (🟡), High (🔴)
        public bool IsCodDisabled { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();

        // Point 46 Restriction System
        public string RestrictionLevel { get; set; } = "Normal";
        public string? RestrictionReason { get; set; }
        public string? RestrictionAppliedDate { get; set; }
        public string? SuspendedUntilDate { get; set; }
        public bool IsReturnDisabled { get; set; }
        public bool IsAccountSuspended { get; set; }
    }

    /// <summary>
    /// Point 33 & 36: Customer Order & Return History & Risk Profile DTO
    /// Displays complete lifetime history: Total Orders, Delivered, Cancelled, Returned, Refunded,
    /// Total Amount, Return Rate %, Cancellation Rate %, Risk Score & Factors, and order items breakdown.
    /// </summary>
    public class AdminCustomerHistoryDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = "Patna";
        public string Address { get; set; } = "Patna, Bihar";
        public string Status { get; set; } = "Active";
        public string RegistrationDate { get; set; } = "05 Sep 2026";

        // Metrics for Point 33
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int RefundedOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReturnRate { get; set; } // Percentage
        public decimal CancellationRate { get; set; } // Percentage

        // Metrics for Point 36
        public int RiskScore { get; set; } = 15;
        public string RiskLevel { get; set; } = "Low";
        public bool IsCodDisabled { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public string? RiskLastEvaluated { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();

        // Point 46 Restriction System
        public string RestrictionLevel { get; set; } = "Normal";
        public string? RestrictionReason { get; set; }
        public string? RestrictionAppliedDate { get; set; }
        public string? SuspendedUntilDate { get; set; }
        public bool IsReturnDisabled { get; set; }
        public bool IsAccountSuspended { get; set; }

        public List<AdminCustomerOrderItemDto> Orders { get; set; } = new List<AdminCustomerOrderItemDto>();
    }

    public class AdminCustomerOrderItemDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Delivered";
        public string PaymentMode { get; set; } = "COD";
        public string OrderDate { get; set; } = "05 Sep 2026";
        public string? ReturnReason { get; set; }
        public int ItemCount { get; set; } = 1;
    }

    /// <summary>
    /// Point 35: Category Management DTO
    /// Categories: Electronics, Fashion, Grocery, Mobiles, Computers
    /// Actions: [Add], [Edit], [Delete]
    /// </summary>
    public class AdminCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-solid fa-layer-group";
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalSales { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive
    }

    /// <summary>
    /// Point 36: Brand Management DTO
    /// Brands: Samsung, Apple, Nike, Adidas, HP, Dell, Lenovo
    /// Actions: [Add], [Edit], [Delete], [View]
    /// </summary>
    public class AdminBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? LogoUrl { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public double Rating { get; set; } = 4.8;
        public string Status { get; set; } = "Active"; // Active, Inactive
        public string RegistrationDate { get; set; } = "05 Sep 2026";
    }

    /// <summary>
    /// Point 38: Admin Order Management DTO
    /// Columns: Order ID, Customer, Seller, Amount, Payment, Status, Date
    /// Filters: Today, Yesterday, This Month, Status, Seller, Customer
    /// Actions: [View], [Update Status]
    /// </summary>
    public class AdminOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty; // #ORD-1001
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string SellerOwner { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMode { get; set; } = "UPI"; // UPI, COD, Card, NetBanking
        public string PaymentStatus { get; set; } = "Paid"; // Paid, Pending, Failed, Refunded
        public string OrderStatus { get; set; } = "Pending"; // Pending, Accepted, Preparing, OutForDelivery, Delivered, Cancelled
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FormattedDate { get; set; } = string.Empty;
        public string DateFilterCategory { get; set; } = "Today"; // Today, Yesterday, ThisMonth, Older
        public string DeliveryAddress { get; set; } = string.Empty;
        public int ItemCount { get; set; } = 1;
        public string? RiderName { get; set; }
        public string? RiderMobile { get; set; }
    }

    /// <summary>
    /// Point 39: Payment Management DTO
    /// Columns: Payment ID, Order ID, Customer, Amount, Payment Method, Transaction ID, Status, Date
    /// Statuses: Pending, Success, Failed, Refunded
    /// Actions: [View Details], [Process Refund]
    /// </summary>
    public class AdminPaymentDto
    {
        public string PaymentId { get; set; } = string.Empty; // PAY-1001
        public string OrderId { get; set; } = string.Empty; // #ORD-1001
        public int RawOrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal NetPayout { get; set; }
        public string PaymentMethod { get; set; } = "UPI"; // UPI, COD, Card, NetBanking, Wallet
        public string TransactionId { get; set; } = string.Empty; // UPI-984920491
        public string Status { get; set; } = "Success"; // Pending, Success, Failed, Refunded
        public string Date { get; set; } = "05 Sep 2026, 10:30 PM";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string GatewayReference { get; set; } = "HDFC-PG-849284";
    }

    /// <summary>
    /// Point 40 & 53: Coupon Management & Campaign Sale Coupons DTO
    /// Columns: Coupon Code, Discount, Minimum Order, Maximum Discount, Start Date, End Date, Usage Limit, Status, Campaign, Real-time Usage Telemetry
    /// Example (Point 53): BIGSALE500 | ₹500 OFF | Minimum Order ₹2,999 | Valid only during Sale | Total Usage: 10,000 | Used: 7,845 | Remaining: 2,155
    /// </summary>
    public class AdminCouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // e.g. BIGSALE500, WELCOME100
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Flat"; // Flat, Percentage, FreeDelivery
        public decimal DiscountValue { get; set; } = 500;
        public string Discount { get; set; } = "₹500 OFF"; // e.g. ₹500 OFF, 20% OFF
        public decimal MinOrder { get; set; } = 2999;
        public decimal? MaxDiscount { get; set; }
        public string StartDate { get; set; } = "10 Sep 2026";
        public string EndDate { get; set; } = "15 Sep 2026";
        public int UsageLimit { get; set; } = 10000;
        public int TotalUsageLimit { get => UsageLimit; set => UsageLimit = value; }
        public int UsedCount { get; set; } = 7845;
        public int RemainingCount => Math.Max(0, UsageLimit - UsedCount);
        public double UsagePercentage => UsageLimit > 0 ? Math.Min(100, Math.Round(((double)UsedCount / UsageLimit) * 100, 1)) : 0;
        public bool IsUsageExhausted => RemainingCount <= 0;

        // Point 53: Campaign-Specific Sale Coupon Fields
        public string? CampaignName { get; set; } = "🔥 Mega Shopping Sale";
        public int? CampaignId { get; set; }
        public bool IsValidOnlyDuringSale { get; set; } = true;
        public string SaleValidityBadge => IsValidOnlyDuringSale ? "Valid only during Sale" : "Standard Promo";
        public string Status { get; set; } = "Active"; // Active, Inactive, Expired, Exhausted
        public bool IsActive { get; set; } = true;
    }

    public class AdminReturnDto
    {
        public string ReturnId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int RawOrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = "SKU-SAMS-M34";
        public string? SerialNumber { get; set; } = "SN-8492049182"; // Dispatched Serial/IMEI
        public string? ReturnReceivedSerial { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 1499m;
        public string Status { get; set; } = "Return_Requested"; // Return_Requested, Under_Verification, Approved, Rejected, Product_Swapped_Fraud
        public string Date { get; set; } = "05 Sep 2026";
        public string ReturnStatusBadge { get; set; } = "Return Requested";

        // Verification Checklist Flags (Point 34)
        public bool IsProductVerified { get; set; } = true;
        public bool IsQuantityVerified { get; set; } = true;
        public bool IsSkuVerified { get; set; } = true;
        public bool IsPackagingVerified { get; set; } = true;

        // Inbound Return Inspection Flags
        public bool? IsReturnSkuMatched { get; set; }
        public bool? IsReturnSerialMatched { get; set; }
        public bool? IsReturnConditionMatched { get; set; }
        public string? VerificationRemarks { get; set; }
    }

    public class AdminRefundDto
    {
        public string RefundId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "UPI";
        public string Gateway { get => PaymentMethod; set => PaymentMethod = value; }
        public string Status { get; set; } = "Processed"; // Pending, Processed, Failed
        public string Date { get; set; } = "05 Sep 2026";
        public string ProcessedDate { get => Date; set => Date = value; }
    }

    /// <summary>
    /// Point 41: Offers Management DTO
    /// Columns: Product, MRP, Selling Price, Discount, Start Date, End Date, Status
    /// </summary>
    public class AdminOfferDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ShopName { get; set; } = string.Empty;
        public decimal Mrp { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; } = "Percentage";
        public string DiscountFormatted { get; set; } = "30% OFF";
        public string StartDate { get; set; } = "05 Sep 2026";
        public string EndDate { get; set; } = "12 Sep 2026";
        public string StartDateRaw { get; set; } = "2026-09-05";
        public string EndDateRaw { get; set; } = "2026-09-12";
        public string? BannerUrl { get; set; }
        public string? Tagline { get; set; } = "Deal of the Day";
        public string Status { get; set; } = "Active"; // Active, Expired, Inactive
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Point 42: Review Management DTO
    /// Columns: Product, Customer, Rating, Review, Date, Status
    /// Actions: [Hide], [Delete]
    /// </summary>
    public class AdminReviewDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; } = "General Item";
        public string ProductImage { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "Customer";
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShopName { get; set; } = "Store Partner";
        public int Rating { get; set; } = 5; // 1 to 5
        public string Comment { get; set; } = string.Empty;
        public string Date { get; set; } = "05 Sep 2026";
        public bool IsHidden { get; set; } = false;
        public string Status { get; set; } = "Visible"; // Visible, Hidden
        public string? ModerationReason { get; set; }
    }

    public class AdminComplaintDto
    {
        public int Id { get; set; }
        public string TicketId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public string? ReasonCategory { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public string Priority { get; set; } = "High"; // Low, Medium, High, Urgent
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed
        public string Date { get; set; } = "05 Sep 2026";
        public string? ResolutionNotes { get; set; }
        public string? ResolvedDate { get; set; }
    }

    public class AdminNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = "10 mins ago";
        public string Type { get; set; } = "Alert"; // Seller, Complaint, Return, Order, Delivery, Payment, Product
        public string RecipientRole { get; set; } = "Admin";
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; } = false;
    }

    #region Point 45: Reports & Analytics DTOs

    /// <summary>
    /// Point 45: Master Admin Reports Container
    /// Holds active date filter states, summary aggregate KPIs, and dataset lists for all 9 report types.
    /// </summary>
    public class AdminReportsViewModel
    {
        public string ActiveReportType { get; set; } = "Sales"; // Sales, Order, Customer, Seller, Product, Payment, Return, Refund, Commission
        public string ActiveDateFilter { get; set; } = "30Days"; // Today, 7Days, 30Days, Custom
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }

        // Filtered Aggregate Summary Cards
        public decimal TotalGrossSales { get; set; }
        public decimal TotalNetSales { get; set; }
        public int TotalOrdersCount { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal TotalRefundsIssued { get; set; }
        public int TotalActiveCustomers { get; set; }
        public int TotalActiveSellers { get; set; }
        public decimal AverageOrderValue { get; set; }

        // The 9 Specific Report Datasets
        public AdminSalesReportDto SalesReport { get; set; } = new AdminSalesReportDto();
        public List<AdminOrderReportItemDto> OrderReports { get; set; } = new List<AdminOrderReportItemDto>();
        public List<AdminCustomerReportItemDto> CustomerReports { get; set; } = new List<AdminCustomerReportItemDto>();
        public List<AdminSellerReportItemDto> SellerReports { get; set; } = new List<AdminSellerReportItemDto>();
        public List<AdminProductReportItemDto> ProductReports { get; set; } = new List<AdminProductReportItemDto>();
        public List<AdminPaymentReportItemDto> PaymentReports { get; set; } = new List<AdminPaymentReportItemDto>();
        public List<AdminReturnReportItemDto> ReturnReports { get; set; } = new List<AdminReturnReportItemDto>();
        public List<AdminRefundReportItemDto> RefundReports { get; set; } = new List<AdminRefundReportItemDto>();
        public List<AdminCommissionReportItemDto> CommissionReports { get; set; } = new List<AdminCommissionReportItemDto>();
    }

    public class AdminSalesReportDto
    {
        public decimal GrossRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal DeliveryFees { get; set; }
        public decimal PlatformCommission { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal AverageOrderValue { get; set; }

        public List<CategorySalesSummaryDto> CategoryBreakdown { get; set; } = new List<CategorySalesSummaryDto>();
        public List<DailySalesPointDto> DailyTrend { get; set; } = new List<DailySalesPointDto>();
    }

    public class CategorySalesSummaryDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class DailySalesPointDto
    {
        public string DateLabel { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal GrossSales { get; set; }
        public decimal NetSales { get; set; }
    }

    public class AdminOrderReportItemDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMode { get; set; } = "UPI";
        public string PaymentStatus { get; set; } = "Paid";
        public string OrderStatus { get; set; } = "Delivered";
        public string OrderDate { get; set; } = string.Empty;
        public DateTime RawDate { get; set; }
        public string? DeliveryDate { get; set; }
    }

    public class AdminCustomerReportItemDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string City { get; set; } = "Patna";
        public string JoinedDate { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string LastOrderDate { get; set; } = "N/A";
        public string Status { get; set; } = "Active";
    }

    public class AdminSellerReportItemDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string City { get; set; } = "Patna";
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal GrossSales { get; set; }
        public decimal CommissionPaid { get; set; }
        public decimal NetPayout { get; set; }
        public double Rating { get; set; }
        public string ApprovalStatus { get; set; } = "Approved";
        public string JoinedDate { get; set; } = string.Empty;
    }

    public class AdminProductReportItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? Brand { get; set; }
        public decimal Mrp { get; set; }
        public decimal SellingPrice { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CurrentStock { get; set; }
        public string StockStatus { get; set; } = "InStock";
        public string ApprovalStatus { get; set; } = "Approved";
    }

    public class AdminPaymentReportItemDto
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "UPI";
        public string TransactionId { get; set; } = string.Empty;
        public string GatewayRef { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public string TransactionDate { get; set; } = string.Empty;
        public DateTime RawDate { get; set; }
    }

    public class AdminReturnReportItemDto
    {
        public string ReturnId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Approved";
        public string RequestDate { get; set; } = string.Empty;
        public string? ResolvedDate { get; set; }
    }

    public class AdminRefundReportItemDto
    {
        public string RefundId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "UPI";
        public string GatewayRef { get; set; } = string.Empty;
        public string Reason { get; set; } = "Customer Return / Cancellation";
        public string Status { get; set; } = "Processed";
        public string ProcessedDate { get; set; } = string.Empty;
    }

    public class AdminCommissionReportItemDto
    {
        public string CommissionId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderDate { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal CommissionRate { get; set; } = 10m; // 10%
        public decimal CommissionAmount { get; set; }
        public decimal NetSellerPayout { get; set; }
        public string PayoutStatus { get; set; } = "Settled";
        public string SettlementDate { get; set; } = string.Empty;
    }

    #endregion

    #region Point 47: Address Risk Management DTOs

    /// <summary>
    /// Point 47: Address Risk Management DTO
    /// Aggregates orders, returns, cancellations, delivered counts, and customer accounts for a normalized address/pincode cluster.
    /// Non-punitive policy: Strictly an investigation/telemetry tool to detect abnormal spikes without auto-blocking customers.
    /// </summary>
    public class AdminAddressRiskDto
    {
        public string AddressKey { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public string City { get; set; } = "Patna";
        public string State { get; set; } = "Bihar";
        public string Pincode { get; set; } = "800001";
        
        // Volume telemetry
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReturnRate { get; set; }
        public decimal CancellationRate { get; set; }
        public int DistinctCustomersCount { get; set; }
        
        // Risk & Alert classification
        public string RiskLevel { get; set; } = "Normal"; // Normal (🟢), Medium Risk (🟡), High Risk (🔴)
        public string AlertBadge { get; set; } = "Normal Activity"; // "Normal Activity", "⚠ High Activity", "High Return Rate", "Multi-Account Cluster"
        
        // Investigation & Safeguard status (Non-punitive workflow)
        public string InvestigationStatus { get; set; } = "Monitoring"; // "Monitoring", "Flagged for Review", "Under Investigation", "Verified / Clean", "High Risk Area"
        public string? AdminNotes { get; set; }
        public string? LastEvaluatedDate { get; set; }
        public bool RequireManualOtpVerification { get; set; } = false; // Flag high-value shipments for manual pre-dispatch call
        
        // Associated details for drilldown
        public List<AdminAddressLinkedCustomerDto> LinkedCustomers { get; set; } = new List<AdminAddressLinkedCustomerDto>();
        public List<AdminAddressOrderItemDto> RecentOrders { get; set; } = new List<AdminAddressOrderItemDto>();
    }

    public class AdminAddressLinkedCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public int ReturnsCount { get; set; }
        public string RestrictionLevel { get; set; } = "Normal";
        public string Status { get; set; } = "Active";
    }

    public class AdminAddressOrderItemDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Delivered";
        public string PaymentMode { get; set; } = "COD";
        public string OrderDate { get; set; } = string.Empty;
        public string? ReturnReason { get; set; }
        public string? CancelReason { get; set; }
    }

    public class UpdateAddressInvestigationRequest
    {
        public string AddressKey { get; set; } = string.Empty;
        public string InvestigationStatus { get; set; } = "Under Investigation";
        public string? AdminNotes { get; set; }
        public bool RequireManualOtpVerification { get; set; }
    }

    #endregion

    #region Point 48: Multiple Account Detection DTOs

    /// <summary>
    /// Point 48: Multiple Account Detection Cluster DTO
    /// Identifies clusters of accounts sharing the same delivery address, phone pattern, or privacy-safe client token.
    /// Privacy compliance: Uses minimal, privacy-respecting client hash tokens (no invasive tracking).
    /// </summary>
    public class AdminMultiAccountClusterDto
    {
        public string ClusterId { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public string PrimaryMatchFactor { get; set; } = "Same Delivery Address"; // "Same Delivery Address", "Similar Phone Pattern", "Device Token Hash"
        public string CommonSignalValue { get; set; } = string.Empty;
        public int TotalAccountsCount { get; set; }
        public int TotalCombinedOrders { get; set; }
        public decimal TotalCombinedSpent { get; set; }
        public string AlertBadge { get; set; } = "⚠ Possible Multiple Accounts";
        public string MatchConfidence { get; set; } = "High"; // High (🔴), Medium (🟡), Low (🟢)
        public string ClusterStatus { get; set; } = "Under Review"; // "Under Review", "Flagged for Verification", "Legitimate Household / Family", "Promo Abuse Restricted", "Resolved / Verified"
        public string? AdminNotes { get; set; }
        public string CreatedDate { get; set; } = string.Empty;
        public bool RestrictFirstOrderCoupons { get; set; } = false;
        public List<AdminClusterAccountMemberDto> Accounts { get; set; } = new List<AdminClusterAccountMemberDto>();
    }

    public class AdminClusterAccountMemberDto
    {
        public int CustomerId { get; set; }
        public string AccountLabel { get; set; } = "Account A"; // Account A, Account B, Account C, Account D
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string RegistrationDate { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public int ReturnsCount { get; set; }
        public decimal TotalSpent { get; set; }
        public string RestrictionLevel { get; set; } = "Normal";
        public string Status { get; set; } = "Active";
        public string DeviceHashMasked { get; set; } = "client_hash_7f***";
        public bool UsedFirstOrderCoupon { get; set; } = false;
    }

    public class UpdateClusterStatusRequest
    {
        public string ClusterId { get; set; } = string.Empty;
        public string Status { get; set; } = "Under Review";
        public string? AdminNotes { get; set; }
        public bool RestrictFirstOrderCoupons { get; set; }
    }

    #endregion

    #region Point 49: COD Abuse Detection DTOs

    /// <summary>
    /// Point 49: COD Abuse Detection DTO
    /// Tracks customer Cash-on-Delivery failure metrics (Orders, Delivered, Rejected / Refused at doorstep).
    /// Enforces business policy: Transition high-failure customers to Prepaid-Only (COD Restricted) mode.
    /// </summary>
    public class AdminCodAbuseDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = "Patna";
        public string Address { get; set; } = "Patna, Bihar";
        
        // COD Volume telemetry
        public int TotalCodOrders { get; set; }
        public int CodDelivered { get; set; }
        public int CodRejected { get; set; } // Doorstep cancelled / rejected
        public int CodPending { get; set; }
        public decimal TotalCodAmount { get; set; }
        public decimal CodRejectionRate { get; set; } // Percentage rejected
        public decimal CodDeliveryRate { get; set; } // Percentage delivered
        
        // COD Risk classification
        public string CodRiskLevel { get; set; } = "Low"; // High (🔴), Medium (🟡), Low (🟢)
        public string AlertBadge { get; set; } = "Healthy COD History"; // "⚠ High COD Failure", "Elevated COD Rejection", "Healthy COD History"
        
        // Policy enforcement
        public string PolicyEnforcement { get; set; } = "Normal (COD Allowed)"; // "Normal (COD Allowed)", "COD Restricted (Prepaid Only)", "Warning Issued"
        public bool IsCodDisabled { get; set; } = false;
        public string? RestrictionReason { get; set; }
        public string? LastRejectionReason { get; set; }
        public string? LastEvaluatedDate { get; set; }
        
        public List<AdminCodOrderHistoryItemDto> CodOrders { get; set; } = new List<AdminCodOrderHistoryItemDto>();
    }

    public class AdminCodOrderHistoryItemDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderStatus { get; set; } = "Delivered"; // Delivered, Cancelled, Returned, Pending
        public string OrderDate { get; set; } = string.Empty;
        public string? CancelOrRejectionReason { get; set; }
        public string? RiderDeliveryNote { get; set; }
        public bool IsDoorstepRejection { get; set; } = false;
    }

    public class UpdateCustomerCodPolicyRequest
    {
        public int CustomerId { get; set; }
        public string PolicyAction { get; set; } = "COD Restricted"; // "COD Restricted", "Restore COD", "Warning"
        public string? Reason { get; set; }
    }

    #endregion

    #region Point 50: Sale / Festival Campaign Management DTOs

    /// <summary>
    /// Point 50: Sale / Festival Campaign Management (e.g., Big Billion Days, Mega Shopping Sale)
    /// Fields: Campaign Name, Start Date, End Date, Start Time, End Time, Discount, Coupon,
    /// Products, Categories, Sellers, Minimum Order, Maximum Discount.
    /// Category Discounts Example: Mobiles -> 20% OFF, Fashion -> 40% OFF, Electronics -> 30% OFF.
    /// </summary>
    public class AdminSaleCampaignDto
    {
        public int Id { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Tagline { get; set; } = "Biggest Sale of the Season";
        public string BannerTheme { get; set; } = "flame-red"; // "flame-red", "festive-gold", "midnight-violet", "electric-blue"
        public string StartDate { get; set; } = string.Empty; // e.g. "2026-09-10" or "10 Sep 2026"
        public string StartTime { get; set; } = "00:00";
        public string EndDate { get; set; } = string.Empty; // e.g. "2026-09-15" or "15 Sep 2026"
        public string EndTime { get; set; } = "23:59";
        public string Status { get; set; } = "Live Now"; // "Live Now", "Upcoming", "Ended", "Paused", "Draft"
        public bool IsActive { get; set; } = true;
        public decimal DefaultDiscountPct { get; set; } = 25;
        public string? CouponCode { get; set; } = "MEGASALE";
        public decimal MinOrderAmount { get; set; } = 999;
        public decimal? MaxDiscountAmount { get; set; } = 2500;
        public string ProductsScope { get; set; } = "All Catalog Products"; // "All Catalog Products", "Featured Flagships", "Selected SKUs"
        public string SellersScope { get; set; } = "All 250 Verified Sellers"; // "All 250 Verified Sellers", "Top Rated Sellers"
        public int ParticipatingSellersCount { get; set; } = 250;
        public int TotalOrdersGenerated { get; set; } = 1420;
        public decimal TotalGrossRevenue { get; set; } = 4850000;
        public string Description { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;

        // Point 51: Scheduled Sale & Auto-Lifecycle Telemetry
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsAutoScheduled { get; set; } = true;
        public string AutoLifecycleStatus { get; set; } = "Auto-Scheduled"; // "Auto-Scheduled", "Auto-Active (Live)", "Auto-Expired"
        public string CountdownLabel { get; set; } = "Sale Starts In:"; // "Sale Starts In:", "Sale Ends In:", "Campaign Expired"
        public string CountdownFormatted { get; set; } = "02 Days 08 Hours 25 Minutes";
        public int CountdownDays { get; set; } = 2;
        public int CountdownHours { get; set; } = 8;
        public int CountdownMinutes { get; set; } = 25;
        public int CountdownSeconds { get; set; } = 0;
        public long TotalSecondsRemaining { get; set; } = 199500; // Total countdown in seconds for JS timers
        public string AutoStatusBadgeClass => Status == "Live Now" ? "bg-danger" : (Status == "Upcoming" ? "bg-warning" : "bg-secondary");

        // Dynamic multi-category discounts
        public List<CampaignCategoryDiscountDto> CategoryDiscounts { get; set; } = new List<CampaignCategoryDiscountDto>();

        // Formatting helpers
        public string CategorySummary => CategoryDiscounts != null && CategoryDiscounts.Any()
            ? string.Join(", ", CategoryDiscounts.Select(c => $"{c.CategoryName} → {c.DiscountPercentage:N0}% OFF"))
            : "Storewide Flat Discount";

        public string ValidityFormatted => $"{StartDate} {StartTime} - {EndDate} {EndTime}";
    }

    public class CampaignCategoryDiscountDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string IconClass { get; set; } = "fa-tags";
        public string BadgeColor { get; set; } = "danger";
    }

    public class CreateOrEditSaleCampaignRequest
    {
        public int? Id { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Tagline { get; set; } = "Biggest Sale of the Season";
        public string BannerTheme { get; set; } = "flame-red";
        public string StartDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = "00:00";
        public string EndDate { get; set; } = string.Empty;
        public string EndTime { get; set; } = "23:59";
        public decimal DefaultDiscountPct { get; set; } = 20;
        public string? CouponCode { get; set; }
        public decimal MinOrderAmount { get; set; } = 500;
        public decimal? MaxDiscountAmount { get; set; }
        public string ProductsScope { get; set; } = "All Catalog Products";
        public string SellersScope { get; set; } = "All 250 Verified Sellers";
        public string? CategoryDiscountsJson { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Point 52: Flash Sale Limited Unit Lightning Deals DTOs

    /// <summary>
    /// Point 52: Flash Sale (Limited-time, limited-quantity lightning deals)
    /// Example: Samsung Mobile ₹40,000 -> ₹32,999 | Only 50 Units | Ends in: 00:25:10 | Stock khatam: SOLD OUT
    /// </summary>
    public class AdminFlashSaleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "Samsung Mobile";
        public string Subtitle { get; set; } = "Galaxy S24 5G Flagship Edition";
        public string ProductName { get => Subtitle; set => Subtitle = value; }
        public string ProductImage { get; set; } = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400";
        public string ProductImageUrl { get => ProductImage; set => ProductImage = value; }
        public string Category { get; set; } = "Mobiles";
        public string Brand { get; set; } = "Samsung";
        public decimal OriginalPrice { get; set; } = 40000;
        public decimal FlashPrice { get; set; } = 32999;
        public decimal DiscountAmount => Math.Max(0, OriginalPrice - FlashPrice);
        public decimal DiscountPercentage => OriginalPrice > 0 ? Math.Round((OriginalPrice - FlashPrice) / OriginalPrice * 100) : 0;
        public int TotalUnits { get; set; } = 50;
        public int TotalStockUnits { get => TotalUnits; set => TotalUnits = value; }
        public int ClaimedUnits { get; set; } = 48;
        public int RemainingUnits => Math.Max(0, TotalUnits - ClaimedUnits);
        public int ClaimedPercentage => TotalUnits > 0 ? Math.Min(100, (int)((double)ClaimedUnits / TotalUnits * 100)) : 100;
        public double ClaimPercentage => ClaimedPercentage;
        public bool IsSoldOut => RemainingUnits <= 0;
        public bool IsExpired => Status == "Expired" || RemainingSeconds <= 0;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public long RemainingSeconds { get; set; } = 1510; // 00:25:10 = 25*60 + 10 = 1510 seconds
        public string TimerFormatted { get; set; } = "00:25:10";
        public string FormattedRemainingTime { get => TimerFormatted; set => TimerFormatted = value; }
        public string Status { get; set; } = "Live Flash Deal"; // "Live Flash Deal", "Sold Out", "Upcoming", "Expired"
        public int MaxPerUser { get; set; } = 1;
        public int MaxUnitsPerCustomer { get => MaxPerUser; set => MaxPerUser = value; }
        public string ShopName { get; set; } = "Samsung Official Flagship Store";
        public string CreatedDate { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateOrEditFlashSaleRequest
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ProductName { get => Subtitle; set => Subtitle = value; }
        public string ProductImage { get; set; } = string.Empty;
        public string ProductImageUrl { get => ProductImage; set => ProductImage = value; }
        public string Category { get; set; } = "Mobiles";
        public string Brand { get; set; } = "Samsung";
        public decimal OriginalPrice { get; set; } = 40000;
        public decimal FlashPrice { get; set; } = 32999;
        public int TotalUnits { get; set; } = 50;
        public int TotalStockUnits { get => TotalUnits; set => TotalUnits = value; }
        public int ClaimedUnits { get; set; } = 0;
        public int DurationMinutes { get; set; } = 30;
        public int MaxPerUser { get; set; } = 1;
        public int MaxUnitsPerCustomer { get => MaxPerUser; set => MaxPerUser = value; }
        public string ShopName { get; set; } = "Official Store";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ClaimFlashSaleRequest
    {
        public int FlashSaleId { get; set; }
        public int Quantity { get; set; } = 1;
        public int CustomerId { get; set; } = 1;
    }

    #endregion

    #region Point 55: Seller Sale Participation & Admin Approval DTOs

    /// <summary>
    /// Point 55: Seller Sale Participation & Admin Approval DTO
    /// Example from Prompt:
    /// Seller Products:
    /// - Samsung Mobile: ☑ Participate in Mega Sale (Approved)
    /// - Laptop: ☐ Participate (Opted Out)
    /// - Headphone: ☑ Participate (Pending Approval / Approved)
    /// Admin final approval de sakta hai.
    /// </summary>
    public class AdminSellerSaleParticipationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // "Samsung Mobile", "Dell XPS Laptop", "Sony Headphone"
        public string ProductImage { get; set; } = string.Empty;
        public string Category { get; set; } = "Mobiles";
        public int ShopId { get; set; }
        public string ShopName { get; set; } = "Samsung Official Flagship Store";
        public int CampaignId { get; set; } = 1;
        public string CampaignName { get; set; } = "🔥 Mega Shopping Sale";
        public decimal RegularPrice { get; set; } = 40000;
        public decimal SalePrice { get; set; } = 32999;
        public decimal DiscountPct => RegularPrice > 0 ? Math.Max(0, Math.Round(((RegularPrice - SalePrice) / RegularPrice) * 100, 1)) : 0;
        public int AllocatedSaleStock { get; set; } = 50;
        public bool IsParticipating { get; set; } = true; // ☑ Participate in Mega Sale (true/false)
        public string ApprovalStatus { get; set; } = "Approved"; // "Approved", "Pending", "Rejected", "OptedOut"
        public string StatusBadgeClass => ApprovalStatus == "Approved" ? "bg-success" : (ApprovalStatus == "Pending" ? "bg-warning" : (ApprovalStatus == "Rejected" ? "bg-danger" : "bg-secondary"));
        public string AdminRemarks { get; set; } = "Approved for Mega Sale banner placement.";
        public string SubmittedDate { get; set; } = "05 Sep 2026";
        public string? ApprovedDate { get; set; } = "06 Sep 2026";
        public string? ApprovedBy { get; set; } = "Super Admin";
    }

    public class UpdateSellerSaleParticipationRequest
    {
        public int ParticipationId { get; set; }
        public int ProductId { get; set; }
        public int ShopId { get; set; } = 1;
        public int CampaignId { get; set; } = 1;
        public bool IsParticipating { get; set; }
        public decimal? SalePrice { get; set; }
        public int? AllocatedStock { get; set; }
        public string? Remarks { get; set; }
    }

    public class AdminSaleParticipationApprovalRequest
    {
        public int ParticipationId { get; set; }
        public bool Approve { get; set; } = true;
        public string? Remarks { get; set; }
    }

    #endregion

    #region Point 56: Sale Analytics & Post-Campaign Report DTOs

    /// <summary>
    /// Point 56: Sale Analytics & Post-Campaign Report DTO
    /// Example from Prompt:
    /// Mega Sale Report:
    /// Total Orders: 25,500
    /// Products Sold: 38,200
    /// Revenue: ₹2.5 Crore
    /// Top Category: Electronics
    /// Top Product: Samsung Mobile
    /// Top Seller: ABC Electronics
    /// Returns: 1,250
    /// Cancellations: 850
    /// </summary>
    public class SaleAnalyticsReportDto
    {
        public int CampaignId { get; set; } = 1;
        public string CampaignName { get; set; } = "🔥 Mega Shopping Sale";
        public string DateRange { get; set; } = "10 Sep 2026 - 15 Sep 2026";
        public string CampaignStatus { get; set; } = "Concluded (Post-Sale Analysis)";
        
        // Core KPIs matching Prompt Exactly
        public int TotalOrders { get; set; } = 25500;
        public int ProductsSold { get; set; } = 38200;
        public decimal Revenue { get; set; } = 25000000; // ₹2.5 Crore
        public string RevenueFormatted { get; set; } = "₹2.50 Crore";
        public decimal NetRevenue => Revenue - (Returns * 2500) - (Cancellations * 1800);
        public string NetRevenueFormatted { get; set; } = "₹2.38 Crore";
        public decimal AverageOrderValue => TotalOrders > 0 ? Math.Round(Revenue / TotalOrders, 2) : 0;
        
        // Top Performers matching Prompt Exactly
        public string TopCategory { get; set; } = "Electronics";
        public decimal TopCategoryRevenue { get; set; } = 12000000; // ₹1.2 Crore
        public string TopCategoryShareFormatted { get; set; } = "48.0% of Total GMV";
        
        public string TopProduct { get; set; } = "Samsung Mobile";
        public string TopProductImage { get; set; } = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=200";
        public int TopProductUnitsSold { get; set; } = 12400;
        public decimal TopProductRevenue { get; set; } = 40918760;
        
        public string TopSeller { get; set; } = "ABC Electronics";
        public int TopSellerOrdersFulfilled { get; set; } = 8200;
        public decimal TopSellerRevenue { get; set; } = 8200000;
        public double TopSellerRating { get; set; } = 4.9;
        
        // Post-Sale Return & Cancellation Risk Metrics
        public int Returns { get; set; } = 1250;
        public double ReturnRatePct => TotalOrders > 0 ? Math.Round(((double)Returns / TotalOrders) * 100, 1) : 4.9;
        public int Cancellations { get; set; } = 850;
        public double CancellationRatePct => TotalOrders > 0 ? Math.Round(((double)Cancellations / TotalOrders) * 100, 1) : 3.3;
        public int SuccessfulDeliveries => TotalOrders - Returns - Cancellations;
        public double FulfillmentRatePct => TotalOrders > 0 ? Math.Round(((double)SuccessfulDeliveries / TotalOrders) * 100, 1) : 91.8;

        // Breakdown Collections
        public List<CampaignCategoryShareDto> CategoryBreakdown { get; set; } = new();
        public List<CampaignProductPerformanceDto> TopProductsList { get; set; } = new();
        public List<CampaignSellerPerformanceDto> TopSellersList { get; set; } = new();
    }

    public class CampaignCategoryShareDto
    {
        public string CategoryName { get; set; } = "Electronics";
        public decimal Revenue { get; set; } = 12000000;
        public string RevenueFormatted { get; set; } = "₹1.20 Cr";
        public int UnitsSold { get; set; } = 18400;
        public double PercentageShare { get; set; } = 48.0;
        public string ColorClass { get; set; } = "warning";
        public string IconClass { get; set; } = "fa-laptop";
    }

    public class CampaignProductPerformanceDto
    {
        public int Rank { get; set; } = 1;
        public string ProductName { get; set; } = "Samsung Mobile (Galaxy S24 Flagship)";
        public string Category { get; set; } = "Mobiles";
        public string SellerName { get; set; } = "ABC Electronics";
        public int UnitsSold { get; set; } = 12400;
        public decimal SalePrice { get; set; } = 32999;
        public decimal GrossRevenue { get; set; } = 40918760;
        public string RevenueFormatted { get; set; } = "₹4.09 Cr";
        public int ReturnsCount { get; set; } = 280;
    }

    public class CampaignSellerPerformanceDto
    {
        public int Rank { get; set; } = 1;
        public string SellerName { get; set; } = "ABC Electronics";
        public string ShopCity { get; set; } = "Mumbai, Maharashtra";
        public int OrdersCount { get; set; } = 8200;
        public int UnitsSold { get; set; } = 14500;
        public decimal Revenue { get; set; } = 8200000;
        public string RevenueFormatted { get; set; } = "₹82.0 Lakhs";
        public double Rating { get; set; } = 4.9;
        public double FulfillmentRate { get; set; } = 97.8;
    }

    #endregion
}
