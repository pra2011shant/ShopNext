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
    /// Point 40: Coupon Management DTO
    /// Columns: Coupon Code, Discount, Minimum Order, Maximum Discount, Start Date, End Date, Usage Limit, Status
    /// Example: WELCOME100, ₹100 OFF, Minimum Order ₹999
    /// Actions: [Add Coupon], [Edit Coupon], [Delete Coupon], [Toggle Status]
    /// </summary>
    public class AdminCouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty; // e.g. WELCOME100
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "Flat"; // Flat, Percentage, FreeDelivery
        public decimal DiscountValue { get; set; } = 100;
        public string Discount { get; set; } = "₹100 OFF"; // e.g. ₹100 OFF, 20% OFF
        public decimal MinOrder { get; set; } = 999;
        public decimal? MaxDiscount { get; set; }
        public string StartDate { get; set; } = "01 Sep 2026";
        public string EndDate { get; set; } = "31 Dec 2026";
        public int UsageLimit { get; set; } = 500;
        public int UsedCount { get; set; } = 42;
        public string Status { get; set; } = "Active"; // Active, Inactive, Expired
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
}
