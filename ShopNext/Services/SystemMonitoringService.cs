using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class SystemMonitoringService : ISystemMonitoringService
    {
        private readonly ShopNextDbContext _context;
        private readonly ILogger<SystemMonitoringService> _logger;

        public SystemMonitoringService(ShopNextDbContext context, ILogger<SystemMonitoringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region User Agent Helpers
        private (string browser, string os, string device) ParseUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return ("Chrome", "Windows", "Desktop");

            string ua = userAgent.ToLowerInvariant();
            string browser = "Chrome";
            if (ua.Contains("edg/")) browser = "Edge";
            else if (ua.Contains("firefox/")) browser = "Firefox";
            else if (ua.Contains("safari/") && !ua.Contains("chrome/")) browser = "Safari";
            else if (ua.Contains("opera") || ua.Contains("opr/")) browser = "Opera";

            string os = "Windows";
            if (ua.Contains("windows nt 10.0") || ua.Contains("windows nt 11.0") || ua.Contains("windows")) os = "Windows 11";
            else if (ua.Contains("android")) os = "Android";
            else if (ua.Contains("iphone") || ua.Contains("ipad")) os = "iOS";
            else if (ua.Contains("mac os") || ua.Contains("macintosh")) os = "macOS";
            else if (ua.Contains("linux")) os = "Linux";

            string device = "Desktop";
            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) device = "Mobile";
            else if (ua.Contains("ipad") || ua.Contains("tablet")) device = "Tablet";

            return (browser, os, device);
        }
        #endregion

        #region 1. Login & Session Tracking
        public async Task<UserSession> RecordLoginAsync(int? userId, string userName, string role, string ipAddress, string userAgent, string? sessionId = null)
        {
            var (browser, os, device) = ParseUserAgent(userAgent);
            sessionId ??= Guid.NewGuid().ToString("N");

            // Deactivate any expired or older duplicate active sessions for this user on same device if needed
            var existingSessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.Role == role && s.IsActive)
                .ToListAsync();

            var userSession = new UserSession
            {
                SessionId = sessionId,
                UserId = userId,
                UserName = userName,
                Role = role,
                IpAddress = ipAddress,
                Browser = browser,
                OperatingSystem = os,
                Device = device,
                LoginTime = DateTime.Now,
                LastSeenTime = DateTime.Now,
                LastPageVisited = $"/{role}/Dashboard",
                LastAction = "LOGIN_SUCCESS",
                IsActive = true,
                ExpiryTime = DateTime.Now.AddDays(7),
                CreatedDate = DateTime.Now
            };
            _context.UserSessions.Add(userSession);

            var loginHistory = new LoginHistory
            {
                UserId = userId,
                UserName = userName,
                Role = role,
                LoginTime = DateTime.Now,
                LastActivityTime = DateTime.Now,
                IpAddress = ipAddress,
                Browser = browser,
                Device = device,
                OperatingSystem = os,
                SessionId = sessionId,
                IsSuccessful = true,
                IsActiveSession = true,
                CreatedDate = DateTime.Now
            };
            _context.LoginHistories.Add(loginHistory);

            // Also record general activity log
            await LogActivityAsync(
                action: "LOGIN",
                module: "Security",
                entity: $"User #{userId}",
                entityId: userId,
                description: $"User {userName} ({role}) signed in successfully from {ipAddress} using {browser} on {os}.",
                userId: userId,
                userName: userName,
                role: role,
                ipAddress: ipAddress,
                device: device,
                browser: browser);

            await _context.SaveChangesAsync();
            return userSession;
        }

        public async Task RecordLoginFailureAsync(string identifier, string role, string ipAddress, string userAgent, string reason)
        {
            var (browser, os, device) = ParseUserAgent(userAgent);

            var loginHistory = new LoginHistory
            {
                UserName = identifier,
                Role = role,
                LoginTime = DateTime.Now,
                LastActivityTime = DateTime.Now,
                IpAddress = ipAddress,
                Browser = browser,
                Device = device,
                OperatingSystem = os,
                IsSuccessful = false,
                FailureReason = reason,
                IsActiveSession = false,
                CreatedDate = DateTime.Now
            };
            _context.LoginHistories.Add(loginHistory);

            // Check if there are recent failed attempts from this IP to generate Security Threat Alert
            var recentFails = await _context.LoginHistories
                .Where(l => !l.IsSuccessful && l.IpAddress == ipAddress && l.CreatedDate >= DateTime.Now.AddMinutes(15))
                .CountAsync();

            if (recentFails >= 4)
            {
                await CreateSecurityAlertAsync(
                    alertType: "FailedLogins",
                    severity: "High",
                    title: $"Multiple Failed Logins Detected ({recentFails + 1} attempts)",
                    description: $"Multiple unauthorized login attempts detected on role '{role}' for identifier '{identifier}' from IP {ipAddress}.",
                    affectedUserId: null,
                    affectedUserName: identifier,
                    ipAddress: ipAddress);
            }

            await LogActivityAsync(
                action: "LOGIN_FAILED",
                module: "Security",
                entity: identifier,
                description: $"Failed login attempt for '{identifier}' ({role}). Reason: {reason}",
                userName: identifier,
                role: role,
                ipAddress: ipAddress,
                device: device,
                browser: browser);

            await _context.SaveChangesAsync();
        }

        public async Task RecordLogoutAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;

            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session != null)
            {
                session.IsActive = false;
                session.LastSeenTime = DateTime.Now;
                session.LastAction = "LOGOUT";
            }

            var loginHistory = await _context.LoginHistories
                .Where(l => l.SessionId == sessionId)
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefaultAsync();

            if (loginHistory != null)
            {
                loginHistory.LogoutTime = DateTime.Now;
                loginHistory.IsActiveSession = false;
            }

            if (session != null)
            {
                await LogActivityAsync(
                    action: "LOGOUT",
                    module: "Security",
                    entity: $"User #{session.UserId}",
                    entityId: session.UserId,
                    description: $"User {session.UserName} ({session.Role}) signed out cleanly.",
                    userId: session.UserId,
                    userName: session.UserName,
                    role: session.Role,
                    ipAddress: session.IpAddress,
                    device: session.Device,
                    browser: session.Browser);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSessionHeartbeatAsync(string sessionId, string currentPage, string currentAction)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;

            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);
            if (session != null)
            {
                session.LastSeenTime = DateTime.Now;
                session.LastPageVisited = currentPage;
                session.LastAction = currentAction;

                var history = await _context.LoginHistories
                    .Where(l => l.SessionId == sessionId)
                    .OrderByDescending(l => l.LoginTime)
                    .FirstOrDefaultAsync();
                if (history != null)
                {
                    history.LastActivityTime = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync()
        {
            // Sessions active in the last 15 minutes
            var cutoff = DateTime.Now.AddMinutes(-30);
            return await _context.UserSessions
                .AsNoTracking()
                .Where(s => s.IsActive && s.LastSeenTime >= cutoff)
                .OrderByDescending(s => s.LastSeenTime)
                .ToListAsync();
        }

        public async Task<bool> ForceLogoutSessionAsync(string sessionId, string forcedByAdmin)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null) return false;

            session.IsActive = false;
            session.LastAction = $"FORCE_LOGOUT_BY_{forcedByAdmin}";

            var history = await _context.LoginHistories.FirstOrDefaultAsync(h => h.SessionId == sessionId);
            if (history != null)
            {
                history.IsActiveSession = false;
                history.IsForceLoggedOut = true;
                history.LogoutTime = DateTime.Now;
            }

            await LogActivityAsync(
                action: "FORCE_LOGOUT",
                module: "Security",
                entity: $"Session {sessionId}",
                description: $"Admin {forcedByAdmin} terminated active session for user {session.UserName} ({session.Role}).",
                userName: forcedByAdmin,
                role: "Admin");

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogoutAllUserSessionsAsync(int userId, string role, string forcedByAdmin)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.Role == role && s.IsActive)
                .ToListAsync();

            foreach (var s in sessions)
            {
                s.IsActive = false;
                s.LastAction = $"TERMINATED_ALL_BY_{forcedByAdmin}";
            }

            await LogActivityAsync(
                action: "FORCE_LOGOUT_ALL",
                module: "Security",
                entity: $"User #{userId}",
                entityId: userId,
                description: $"Admin {forcedByAdmin} force-terminated all active sessions for User #{userId} ({role}).",
                userName: forcedByAdmin,
                role: "Admin");

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LoginHistory>> GetLoginHistoriesAsync(int count = 100, string? role = null, bool? isSuccessful = null, string? search = null)
        {
            var query = _context.LoginHistories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
                query = query.Where(l => l.Role == role);

            if (isSuccessful.HasValue)
                query = query.Where(l => l.IsSuccessful == isSuccessful.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.UserName.Contains(search) || (l.IpAddress != null && l.IpAddress.Contains(search)) || (l.Browser != null && l.Browser.Contains(search)));

            return await query
                .OrderByDescending(l => l.LoginTime)
                .Take(count)
                .ToListAsync();
        }
        #endregion

        #region 2. User Activity Tracking
        public async Task LogActivityAsync(
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
            string? browser = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    Action = action.ToUpperInvariant(),
                    Module = module,
                    Entity = entity,
                    EntityId = entityId,
                    Description = description,
                    UserId = userId,
                    UserName = userName ?? "System",
                    Role = role ?? "System",
                    IpAddress = ipAddress,
                    Device = device ?? "Desktop",
                    Browser = browser ?? "Chrome",
                    Timestamp = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving UserActivity: {Action} on {Module}", action, module);
            }
        }

        public async Task<List<UserActivity>> GetRecentActivitiesAsync(int count = 100, string? module = null, string? role = null, string? action = null, string? search = null)
        {
            var query = _context.UserActivities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(module) && module != "All")
                query = query.Where(a => a.Module == module);

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
                query = query.Where(a => a.Role == role);

            if (!string.IsNullOrWhiteSpace(action) && action != "All")
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.UserName.Contains(search) || (a.Description != null && a.Description.Contains(search)) || (a.Entity != null && a.Entity.Contains(search)));

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        #endregion

        #region 3. Before/After Entity Diff Auditing
        public async Task LogEntityChangeAsync(
            string entityName,
            int entityId,
            string action,
            string? fieldName,
            string? oldValue,
            string? newValue,
            int? userId,
            string? userName,
            string? userRole,
            string? ipAddress = null)
        {
            try
            {
                var diff = new EntityChangeLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action.ToUpperInvariant(),
                    FieldName = fieldName,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ChangedByUserId = userId,
                    ChangedByUserName = userName ?? "System",
                    ChangedByUserRole = userRole ?? "System",
                    Timestamp = DateTime.Now,
                    IpAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.EntityChangeLogs.Add(diff);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording EntityChangeLog for {EntityName} #{EntityId}", entityName, entityId);
            }
        }

        public async Task<List<EntityChangeLog>> GetEntityDiffsAsync(string? entityName = null, int? entityId = null, int count = 100)
        {
            var query = _context.EntityChangeLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName) && entityName != "All")
                query = query.Where(c => c.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(c => c.EntityId == entityId.Value);

            return await query
                .OrderByDescending(c => c.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        #endregion

        #region 4. Soft Delete & Restore Tracking
        public async Task LogSoftDeleteAsync(
            string entityName,
            int entityId,
            string? entityTitle,
            string? reason,
            string? snapshotDataJson,
            int? userId,
            string? userName,
            string? userRole,
            string? ipAddress = null)
        {
            try
            {
                var deleteLog = new SoftDeleteLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    EntityTitle = entityTitle,
                    Reason = reason ?? "User initiated deletion",
                    SnapshotDataJson = snapshotDataJson,
                    DeletedByUserId = userId,
                    DeletedByUserName = userName ?? "System",
                    DeletedByUserRole = userRole ?? "System",
                    DeletedAt = DateTime.Now,
                    IpAddress = ipAddress,
                    IsRestored = false,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.SoftDeleteLogs.Add(deleteLog);

                // Also log to user activity
                await LogActivityAsync(
                    action: "DELETE",
                    module: entityName,
                    entity: $"{entityName} #{entityId}",
                    entityId: entityId,
                    description: $"Deleted {entityName} '{entityTitle}'. Reason: {reason}",
                    userId: userId,
                    userName: userName,
                    role: userRole,
                    ipAddress: ipAddress);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording SoftDeleteLog for {EntityName} #{EntityId}", entityName, entityId);
            }
        }

        public async Task<bool> RestoreDeletedRecordAsync(int softDeleteLogId, string restoredBy)
        {
            var log = await _context.SoftDeleteLogs.FirstOrDefaultAsync(l => l.Id == softDeleteLogId);
            if (log == null || log.IsRestored) return false;

            log.IsRestored = true;
            log.RestoredAt = DateTime.Now;
            log.RestoredBy = restoredBy;

            // Attempt restoring the actual record in DB
            if (log.EntityName.Equals("Product", StringComparison.OrdinalIgnoreCase))
            {
                var prod = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == log.EntityId);
                if (prod != null) { prod.IsDeleted = false; prod.IsActive = true; }
            }
            else if (log.EntityName.Equals("Shop", StringComparison.OrdinalIgnoreCase))
            {
                var shop = await _context.Shops.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == log.EntityId);
                if (shop != null) { shop.IsDeleted = false; shop.IsActive = true; }
            }
            else if (log.EntityName.Equals("Coupon", StringComparison.OrdinalIgnoreCase))
            {
                var coupon = await _context.Coupons.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == log.EntityId);
                if (coupon != null) { coupon.IsDeleted = false; coupon.IsActive = true; }
            }

            await LogActivityAsync(
                action: "RESTORE",
                module: log.EntityName,
                entity: $"{log.EntityName} #{log.EntityId}",
                entityId: log.EntityId,
                description: $"Restored soft-deleted {log.EntityName} '{log.EntityTitle}' by {restoredBy}.",
                userName: restoredBy,
                role: "Admin");

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SoftDeleteLog>> GetDeletedRecordsAsync(int count = 100, string? entityName = null)
        {
            var query = _context.SoftDeleteLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName) && entityName != "All")
                query = query.Where(d => d.EntityName == entityName);

            return await query
                .OrderByDescending(d => d.DeletedAt)
                .Take(count)
                .ToListAsync();
        }
        #endregion

        #region 5. Seen/Viewed Record Tracking
        public async Task LogEntityViewAsync(
            string entityName,
            int entityId,
            int? userId,
            string userName,
            string userRole,
            string? ipAddress = null,
            string? extraInfo = null)
        {
            try
            {
                var viewLog = new EntityViewLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    ViewedByUserId = userId,
                    ViewedByUserName = userName,
                    ViewedByUserRole = userRole,
                    ViewedAt = DateTime.Now,
                    IpAddress = ipAddress,
                    ExtraInfo = extraInfo,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.EntityViewLogs.Add(viewLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording EntityViewLog for {EntityName} #{EntityId}", entityName, entityId);
            }
        }

        public async Task<List<EntityViewLog>> GetEntityViewsAsync(string? entityName = null, int? entityId = null, int count = 100)
        {
            var query = _context.EntityViewLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName) && entityName != "All")
                query = query.Where(v => v.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(v => v.EntityId == entityId.Value);

            return await query
                .OrderByDescending(v => v.ViewedAt)
                .Take(count)
                .ToListAsync();
        }
        #endregion

        #region 6. Security Threat Alerts
        public async Task CreateSecurityAlertAsync(string alertType, string severity, string title, string description, int? affectedUserId, string? affectedUserName, string? ipAddress)
        {
            var alert = new SecurityThreatAlert
            {
                AlertType = alertType,
                Severity = severity,
                Title = title,
                Description = description,
                AffectedUserId = affectedUserId,
                AffectedUserName = affectedUserName,
                IpAddress = ipAddress,
                DetectedAt = DateTime.Now,
                IsResolved = false,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.SecurityThreatAlerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SecurityThreatAlert>> GetSecurityAlertsAsync(bool? isResolved = null)
        {
            var query = _context.SecurityThreatAlerts.AsNoTracking().AsQueryable();

            if (isResolved.HasValue)
                query = query.Where(a => a.IsResolved == isResolved.Value);

            return await query
                .OrderByDescending(a => a.DetectedAt)
                .ToListAsync();
        }

        public async Task<bool> ResolveSecurityAlertAsync(int alertId, string resolvedBy)
        {
            var alert = await _context.SecurityThreatAlerts.FirstOrDefaultAsync(a => a.Id == alertId);
            if (alert == null) return false;

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.Now;
            alert.ResolvedBy = resolvedBy;

            await LogActivityAsync(
                action: "RESOLVE_ALERT",
                module: "Security",
                entity: $"Alert #{alertId}",
                entityId: alertId,
                description: $"Security Alert '{alert.Title}' was marked resolved by {resolvedBy}.",
                userName: resolvedBy,
                role: "Admin");

            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region 7. Master Dashboard Aggregation & Export
        public async Task<SystemMonitoringDashboardViewModel> GetMonitoringDashboardStatsAsync(string? roleFilter = null, string? moduleFilter = null, string? search = null)
        {
            var today = DateTime.Today;
            var activeCutoff = DateTime.Now.AddMinutes(-30);

            // Total user metrics
            int totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer" && !u.IsDeleted);
            int totalSellers = await _context.Shops.CountAsync(s => !s.IsDeleted);
            int totalRiders = await _context.Riders.CountAsync(r => !r.IsDeleted);
            int totalUsers = totalCustomers + totalSellers + totalRiders;

            // Live online/offline
            int onlineUsers = await _context.UserSessions.CountAsync(s => s.IsActive && s.LastSeenTime >= activeCutoff);
            int offlineUsers = Math.Max(0, totalUsers - onlineUsers);

            // Today's metrics
            int todayLogins = await _context.LoginHistories.CountAsync(l => l.LoginTime >= today && l.IsSuccessful);
            int failedLogins = await _context.LoginHistories.CountAsync(l => l.LoginTime >= today && !l.IsSuccessful);
            int todayOrders = await _context.Orders.CountAsync(o => o.CreatedDate >= today);
            int todayActivities = await _context.UserActivities.CountAsync(a => a.Timestamp >= today);

            // Timestamps
            var lastActivity = await _context.UserActivities.OrderByDescending(a => a.Timestamp).Select(a => (DateTime?)a.Timestamp).FirstOrDefaultAsync();
            var lastLogin = await _context.LoginHistories.Where(l => l.IsSuccessful).OrderByDescending(l => l.LoginTime).Select(l => (DateTime?)l.LoginTime).FirstOrDefaultAsync();
            var lastLogout = await _context.LoginHistories.Where(l => l.LogoutTime != null).OrderByDescending(l => l.LogoutTime).Select(l => (DateTime?)l.LogoutTime).FirstOrDefaultAsync();

            // Collections
            var activeSessions = await GetActiveSessionsAsync();
            var recentActivities = await GetRecentActivitiesAsync(100, moduleFilter, roleFilter, null, search);
            var recentLogins = await GetLoginHistoriesAsync(100, roleFilter, null, search);
            var recentDiffs = await GetEntityDiffsAsync(null, null, 50);
            var deletedRecords = await GetDeletedRecordsAsync(50, null);
            var recentViewLogs = await GetEntityViewsAsync(null, null, 50);
            var securityAlerts = await GetSecurityAlertsAsync(null);

            return new SystemMonitoringDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = onlineUsers,
                OnlineUsers = onlineUsers,
                OfflineUsers = offlineUsers,
                TotalSellers = totalSellers,
                TotalRiders = totalRiders,
                TotalCustomers = totalCustomers,
                TodayLogins = todayLogins,
                FailedLoginAttempts = failedLogins,
                TodayOrders = todayOrders,
                TodayActivities = todayActivities,
                LastActivityTime = lastActivity ?? DateTime.Now,
                LastLoginTime = lastLogin ?? DateTime.Now,
                LastLogoutTime = lastLogout ?? DateTime.Now,
                ActiveSessions = activeSessions,
                RecentActivities = recentActivities,
                RecentLogins = recentLogins,
                RecentAuditDiffs = recentDiffs,
                DeletedRecords = deletedRecords,
                RecentViewLogs = recentViewLogs,
                SecurityAlerts = securityAlerts,
                RoleFilter = roleFilter,
                ModuleFilter = moduleFilter,
                SearchQuery = search
            };
        }

        public async Task<byte[]> ExportToCsvAsync(string exportType)
        {
            var sb = new StringBuilder();

            if (exportType.Equals("Logins", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("ID,User Name,Role,Login Time,Logout Time,IP Address,Browser,Device,OS,Status,Failure Reason");
                var logins = await _context.LoginHistories.OrderByDescending(l => l.LoginTime).Take(1000).ToListAsync();
                foreach (var l in logins)
                {
                    sb.AppendLine($"\"{l.Id}\",\"{l.UserName}\",\"{l.Role}\",\"{l.LoginTime:yyyy-MM-dd HH:mm:ss}\",\"{l.LogoutTime:yyyy-MM-dd HH:mm:ss}\",\"{l.IpAddress}\",\"{l.Browser}\",\"{l.Device}\",\"{l.OperatingSystem}\",\"{(l.IsSuccessful ? "Success" : "Failed")}\",\"{l.FailureReason}\"");
                }
            }
            else if (exportType.Equals("Activities", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("ID,User Name,Role,Action,Module,Entity,Description,Timestamp,IP Address,Device,Browser");
                var acts = await _context.UserActivities.OrderByDescending(a => a.Timestamp).Take(1000).ToListAsync();
                foreach (var a in acts)
                {
                    sb.AppendLine($"\"{a.Id}\",\"{a.UserName}\",\"{a.Role}\",\"{a.Action}\",\"{a.Module}\",\"{a.Entity}\",\"{a.Description?.Replace("\"", "'")}\",\"{a.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{a.IpAddress}\",\"{a.Device}\",\"{a.Browser}\"");
                }
            }
            else if (exportType.Equals("AuditDiffs", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("ID,Entity,Entity ID,Action,Field Name,Old Value,New Value,Changed By,Role,Timestamp,IP");
                var diffs = await _context.EntityChangeLogs.OrderByDescending(c => c.Timestamp).Take(1000).ToListAsync();
                foreach (var d in diffs)
                {
                    sb.AppendLine($"\"{d.Id}\",\"{d.EntityName}\",\"{d.EntityId}\",\"{d.Action}\",\"{d.FieldName}\",\"{d.OldValue?.Replace("\"", "'")}\",\"{d.NewValue?.Replace("\"", "'")}\",\"{d.ChangedByUserName}\",\"{d.ChangedByUserRole}\",\"{d.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{d.IpAddress}\"");
                }
            }
            else
            {
                sb.AppendLine("ID,Session ID,User Name,Role,Device,Browser,OS,IP,Login Time,Last Seen,Last Page,Last Action,Active");
                var sessions = await _context.UserSessions.OrderByDescending(s => s.LastSeenTime).Take(1000).ToListAsync();
                foreach (var s in sessions)
                {
                    sb.AppendLine($"\"{s.Id}\",\"{s.SessionId}\",\"{s.UserName}\",\"{s.Role}\",\"{s.Device}\",\"{s.Browser}\",\"{s.OperatingSystem}\",\"{s.IpAddress}\",\"{s.LoginTime:yyyy-MM-dd HH:mm:ss}\",\"{s.LastSeenTime:yyyy-MM-dd HH:mm:ss}\",\"{s.LastPageVisited}\",\"{s.LastAction}\",\"{s.IsActive}\"");
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
        #endregion
    }
}
