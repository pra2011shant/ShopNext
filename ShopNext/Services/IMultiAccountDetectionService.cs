using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IMultiAccountDetectionService
    {
        Task<List<AdminMultiAccountClusterDto>> GetMultiAccountClustersAsync();
        Task<AdminMultiAccountClusterDto?> GetClusterDetailsAsync(string clusterId);
        Task<bool> UpdateClusterStatusAsync(string clusterId, string status, string? notes, bool restrictWelcomeCoupons);
    }
}
