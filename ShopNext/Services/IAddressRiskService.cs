using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IAddressRiskService
    {
        Task<List<AdminAddressRiskDto>> GetAddressRiskAnalyticsAsync();
        Task<AdminAddressRiskDto?> GetAddressRiskDetailsAsync(string addressKey);
        Task<bool> UpdateAddressInvestigationAsync(string addressKey, string status, string? notes, bool requireManualOtp);
    }
}
