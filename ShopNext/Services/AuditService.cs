using Microsoft.EntityFrameworkCore;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class AuditService : IAuditService
    {
        private readonly ShopNextDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ShopNextDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(
            string action, 
            string? entityName = null, 
            int? entityId = null, 
            string? details = null, 
            int? userId = null, 
            string? userName = null, 
            string? userRole = null, 
            string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = details,
                    UserId = userId,
                    UserName = userName ?? "System",
                    UserRole = userRole ?? "System",
                    IpAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("AuditLog recorded: {Action} on {EntityName} (ID: {EntityId}) by {UserName} ({UserRole})", action, entityName, entityId, userName, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit log: {Action}", action);
            }
        }

        public async Task<List<AuditLog>> GetRecentLogsAsync(int count = 100, string? actionFilter = null, string? roleFilter = null)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                query = query.Where(a => a.Action.Contains(actionFilter));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(a => a.UserRole == roleFilter);
            }

            return await query
                .OrderByDescending(a => a.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetLogCountAsync()
        {
            return await _context.AuditLogs.AsNoTracking().CountAsync();
        }
    }
}
