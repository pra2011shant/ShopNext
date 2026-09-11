using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNext.Models;

namespace ShopNext.Services
{
    /// <summary>
    /// Targeted Role-Based and ID-Specific Notification Dispatcher
    /// Sends notifications directly to individual CustomerId, ShopId, RiderId, or Admin.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ShopNextDbContext _context;

        public NotificationService(ShopNextDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> SendCustomerNotificationAsync(int customerId, string title, string message, string type = "Order", string? linkUrl = null)
        {
            var notif = new Notification
            {
                CustomerId = customerId,
                RecipientRole = "Customer",
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl ?? "/Customer/Account?tab=notifications",
                IsRead = false,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<Notification> SendSellerNotificationAsync(int shopId, string title, string message, string type = "Order", string? linkUrl = null)
        {
            var notif = new Notification
            {
                ShopId = shopId,
                RecipientRole = "Seller",
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl ?? "/Vendor/Dashboard#tab-notifications",
                IsRead = false,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<Notification> SendRiderNotificationAsync(int riderId, string title, string message, string type = "Delivery", string? linkUrl = null)
        {
            var notif = new Notification
            {
                RiderId = riderId,
                RecipientRole = "Rider",
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl ?? "/Rider/Dashboard",
                IsRead = false,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<Notification> SendAdminNotificationAsync(string title, string message, string type = "System", string? linkUrl = null)
        {
            var notif = new Notification
            {
                RecipientRole = "Admin",
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl ?? "/Admin/Dashboard#notifications",
                IsRead = false,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<Notification> BroadcastRoleNotificationAsync(string role, string title, string message, string type = "Offer", string? linkUrl = null)
        {
            var notif = new Notification
            {
                RecipientRole = role,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<List<Notification>> GetCustomerNotificationsAsync(int customerId, int limit = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => (n.CustomerId == customerId || (n.RecipientRole == "Customer" && n.CustomerId == null)) && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetSellerNotificationsAsync(int shopId, int limit = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => (n.ShopId == shopId || (n.RecipientRole == "Seller" && n.ShopId == null)) && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetRiderNotificationsAsync(int riderId, int limit = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => (n.RiderId == riderId || (n.RecipientRole == "Rider" && n.RiderId == null)) && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAdminNotificationsAsync(int limit = 50)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => (n.RecipientRole == "Admin" || n.RecipientRole == "System" || n.RecipientRole == "All") && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string role, int? targetId = null)
        {
            var query = _context.Notifications.AsNoTracking().Where(n => !n.IsRead && !n.IsDeleted);

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                if (targetId.HasValue && targetId.Value > 0)
                    query = query.Where(n => n.CustomerId == targetId.Value || (n.RecipientRole == "Customer" && n.CustomerId == null));
                else
                    query = query.Where(n => n.RecipientRole == "Customer");
            }
            else if (role.Equals("Seller", StringComparison.OrdinalIgnoreCase))
            {
                if (targetId.HasValue && targetId.Value > 0)
                    query = query.Where(n => n.ShopId == targetId.Value || (n.RecipientRole == "Seller" && n.ShopId == null));
                else
                    query = query.Where(n => n.RecipientRole == "Seller");
            }
            else if (role.Equals("Rider", StringComparison.OrdinalIgnoreCase))
            {
                if (targetId.HasValue && targetId.Value > 0)
                    query = query.Where(n => n.RiderId == targetId.Value || (n.RecipientRole == "Rider" && n.RiderId == null));
                else
                    query = query.Where(n => n.RecipientRole == "Rider");
            }
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(n => n.RecipientRole == "Admin" || n.RecipientRole == "System" || n.RecipientRole == "All");
            }

            return await query.CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notif == null) return false;

            notif.IsRead = true;
            notif.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string role, int? targetId = null)
        {
            var query = _context.Notifications.Where(n => !n.IsRead && !n.IsDeleted);

            if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase) && targetId.HasValue)
                query = query.Where(n => n.CustomerId == targetId.Value);
            else if (role.Equals("Seller", StringComparison.OrdinalIgnoreCase) && targetId.HasValue)
                query = query.Where(n => n.ShopId == targetId.Value);
            else if (role.Equals("Rider", StringComparison.OrdinalIgnoreCase) && targetId.HasValue)
                query = query.Where(n => n.RiderId == targetId.Value);
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                query = query.Where(n => n.RecipientRole == "Admin" || n.RecipientRole == "System");

            var items = await query.ToListAsync();
            foreach (var item in items)
            {
                item.IsRead = true;
                item.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
