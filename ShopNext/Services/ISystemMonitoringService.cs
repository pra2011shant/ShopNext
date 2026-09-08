using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface ISystemMonitoringService
    {
        // 1. Login & Session Tracking
        Task<UserSession> RecordLoginAsync(int? userId, string userName, string role, string ipAddress, string userAgent, string? sessionId = null);
        Task RecordLoginFailureAsync(string identifier, string role, string ipAddress, string userAgent, string reason);
        Task RecordLogoutAsync(string sessionId);
        Task UpdateSessionHeartbeatAsync(string sessionId, string currentPage, string currentAction);
        Task<List<UserSession>> GetActiveSessionsAsync();
        Task<bool> ForceLogoutSessionAsync(string sessionId, string forcedByAdmin);
        Task<bool> LogoutAllUserSessionsAsync(int userId, string role, string forcedByAdmin);
        Task<List<LoginHistory>> GetLoginHistoriesAsync(int count = 100, string? role = null, bool? isSuccessful = null, string? search = null);

        // 2. User Activity Tracking
        Task LogActivityAsync(
            string action,
            string module,
            string? entity = null,
            int? entityId = null,
            string? description = null,
            int? userId = null,
            string? userName = null,
            string? role = null,
            string? ipAddress = null,
            string? device = null,
            string? browser = null);

        Task<List<UserActivity>> GetRecentActivitiesAsync(int count = 100, string? module = null, string? role = null, string? action = null, string? search = null);

        // 3. Before/After Entity Diff Auditing
        Task LogEntityChangeAsync(
            string entityName,
            int entityId,
            string action,
            string? fieldName,
            string? oldValue,
            string? newValue,
            int? userId,
            string? userName,
            string? userRole,
            string? ipAddress = null);

        Task<List<EntityChangeLog>> GetEntityDiffsAsync(string? entityName = null, int? entityId = null, int count = 100);

        // 4. Soft Delete & Restore Tracking
        Task LogSoftDeleteAsync(
            string entityName,
            int entityId,
            string? entityTitle,
            string? reason,
            string? snapshotDataJson,
            int? userId,
            string? userName,
            string? userRole,
            string? ipAddress = null);

        Task<bool> RestoreDeletedRecordAsync(int softDeleteLogId, string restoredBy);
        Task<List<SoftDeleteLog>> GetDeletedRecordsAsync(int count = 100, string? entityName = null);

        // 5. Seen/Viewed Record Tracking
        Task LogEntityViewAsync(
            string entityName,
            int entityId,
            int? userId,
            string userName,
            string userRole,
            string? ipAddress = null,
            string? extraInfo = null);

        Task<List<EntityViewLog>> GetEntityViewsAsync(string? entityName = null, int? entityId = null, int count = 100);

        // 6. Security Threat Alerts
        Task CreateSecurityAlertAsync(string alertType, string severity, string title, string description, int? affectedUserId, string? affectedUserName, string? ipAddress);
        Task<List<SecurityThreatAlert>> GetSecurityAlertsAsync(bool? isResolved = null);
        Task<bool> ResolveSecurityAlertAsync(int alertId, string resolvedBy);

        // 7. Master Dashboard Aggregation & Export
        Task<SystemMonitoringDashboardViewModel> GetMonitoringDashboardStatsAsync(string? roleFilter = null, string? moduleFilter = null, string? search = null);
        Task<byte[]> ExportToCsvAsync(string exportType);
    }
}
