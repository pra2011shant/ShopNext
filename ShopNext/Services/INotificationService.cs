using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface INotificationService
    {
        Task<Notification> SendCustomerNotificationAsync(int customerId, string title, string message, string type = "Order", string? linkUrl = null);
        Task<Notification> SendSellerNotificationAsync(int shopId, string title, string message, string type = "Order", string? linkUrl = null);
        Task<Notification> SendRiderNotificationAsync(int riderId, string title, string message, string type = "Delivery", string? linkUrl = null);
        Task<Notification> SendAdminNotificationAsync(string title, string message, string type = "System", string? linkUrl = null);
        Task<Notification> BroadcastRoleNotificationAsync(string role, string title, string message, string type = "Offer", string? linkUrl = null);

        Task<List<Notification>> GetCustomerNotificationsAsync(int customerId, int limit = 50);
        Task<List<Notification>> GetSellerNotificationsAsync(int shopId, int limit = 50);
        Task<List<Notification>> GetRiderNotificationsAsync(int riderId, int limit = 50);
        Task<List<Notification>> GetAdminNotificationsAsync(int limit = 50);

        Task<int> GetUnreadCountAsync(string role, int? targetId = null);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string role, int? targetId = null);
    }
}
