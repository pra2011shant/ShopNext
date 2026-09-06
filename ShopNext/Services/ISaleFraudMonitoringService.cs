using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface ISaleFraudMonitoringService
    {
        Task<SaleFraudDashboardSummaryDto> GetSaleFraudDashboardSummaryAsync();
        Task<List<AdminSaleFraudAlertDto>> GetSuspiciousActivitiesAsync();
        Task<AdminSaleFraudAlertDto?> GetFraudAlertByIdAsync(int alertId);
        Task<(bool Success, string Message, AdminSaleFraudAlertDto? UpdatedAlert)> ReviewFraudAlertAsync(int alertId, string action, string? adminNotes);
        Task<SaleFraudCheckResultDto> EvaluateSaleTransactionAsync(int customerId, string couponCode, string ipAddress, string deviceId, decimal orderAmount);
    }
}
