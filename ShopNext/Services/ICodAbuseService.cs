using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface ICodAbuseService
    {
        Task<List<AdminCodAbuseDto>> GetCodAbuseAnalyticsAsync();
        Task<AdminCodAbuseDto?> GetCustomerCodDetailsAsync(int customerId);
        Task<bool> UpdateCustomerCodPolicyAsync(int customerId, string policyAction, string? reason);
    }
}
