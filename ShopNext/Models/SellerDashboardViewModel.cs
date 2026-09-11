using System.Collections.Generic;

namespace ShopNext.Models
{
    public class SellerDashboardViewModel
    {
        public Shop Shop { get; set; } = new Shop();
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int PendingReturns { get; set; }
        public int LowStock { get; set; }
        public int AvailableStock { get; set; }
        public int OutOfStock { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Coupon> Coupons { get; set; } = new List<Coupon>();

        // 28. Seller Sales (Today, Weekly, Monthly, Yearly)
        public decimal TodaySales { get; set; }
        public int TodayOrdersCount { get; set; }
        public decimal WeeklySales { get; set; }
        public int WeeklyOrdersCount { get; set; }
        public decimal MonthlySales { get; set; }
        public int MonthlyOrdersCount { get; set; }
        public decimal YearlySales { get; set; }
        public int YearlyOrdersCount { get; set; }

        // 29. Seller Earnings (Total Sales - Commission - Refund = Net Earnings)
        public decimal PlatformCommissionRate { get; set; } = 5.0m; // 5%
        public decimal PlatformCommission { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal NetEarnings { get; set; }
        public decimal SettledAmount { get; set; }
        public decimal PendingPayoutAmount { get; set; }

        // Targeted Seller Notifications
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public int UnreadNotificationsCount { get; set; }
    }
}
