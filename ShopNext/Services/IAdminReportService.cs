using ShopNext.Models;
using System;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles Point 45 platform-wide reports and analytics across 
    /// Sales, Orders, Customers, Sellers, Products, Payments, Returns, Refunds, and Commissions.
    /// </summary>
    public interface IAdminReportService
    {
        Task<AdminReportsViewModel> GetAdminReportsAsync(string reportType, string dateFilter, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportReportCsvAsync(string reportType, string dateFilter, DateTime? startDate = null, DateTime? endDate = null);
    }
}
