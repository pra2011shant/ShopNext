using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string? entityName = null, int? entityId = null, string? details = null, int? userId = null, string? userName = null, string? userRole = null, string? ipAddress = null);
        Task<List<AuditLog>> GetRecentLogsAsync(int count = 100, string? actionFilter = null, string? roleFilter = null);
        Task<int> GetLogCountAsync();
    }
}
