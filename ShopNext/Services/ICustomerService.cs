using ShopNext.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles customer identity, profiles, wishlist, notifications, and addresses.
    /// </summary>
    public interface ICustomerService
    {
        // User & Customer Operations
        Task<int> CreateUserAsync(User user);
        Task<int> RegisterCustomerAsync(User user);
        Task<User?> CustomerLoginAsync(string loginIdentifier, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateCustomerProfileAsync(int customerId, string name, string? email, string? profilePhoto, string? gender, DateTime? dob);
        Task<bool> ChangePasswordAsync(int customerId, string currentPassword, string newPassword);
        Task<bool> DeleteUserAsync(int id, int? updatedById = null);

        // Customer Saved Addresses Operations
        Task<IEnumerable<CustomerAddress>> GetCustomerAddressesAsync(int customerId);
        Task<int> InsertCustomerAddressAsync(CustomerAddress address);
        Task<bool> DeleteCustomerAddressAsync(int addressId, int customerId);

        // Wishlist Operations
        Task<IEnumerable<Wishlist>> GetWishlistAsync(int customerId);
        Task<bool> ToggleWishlistAsync(int customerId, int productId);
        Task<bool> RemoveFromWishlistAsync(int customerId, int productId);

        // Point 44: Notifications & Coupons
        Task<IEnumerable<Notification>> GetNotificationsAsync(int customerId);
        Task<IEnumerable<Notification>> GetNotificationsForRoleAsync(string role, int? shopId = null, int? customerId = null);
        Task<int> GetUnreadNotificationCountAsync(string role, int? shopId = null, int? customerId = null);
        Task<int> CreateNotificationAsync(Notification notification);
        Task<bool> MarkNotificationReadAsync(int notificationId, int? customerId = null);
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync();

        // Point 43: Complaints & Support Tickets
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<List<Complaint>> GetComplaintsByCustomerIdAsync(int customerId);
        Task<List<Complaint>> GetComplaintsByOrderIdAsync(int orderId);
        Task<List<Complaint>> GetAllComplaintsForAdminAsync();
        Task<Complaint?> GetComplaintByIdAsync(int id);
        Task<bool> UpdateComplaintStatusAsync(int id, string status, string? resolutionNotes, int? updatedById = null);
    }
}
