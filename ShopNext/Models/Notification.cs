using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Notifications
    /// Description: System & targeted transactional alerts for Customers, Sellers, Fleet Riders, and Platform Admins.
    /// </summary>
    [Table("Notifications")]
    public class Notification : BaseModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? ShopId { get; set; }
        public int? RiderId { get; set; }
        public string RecipientRole { get; set; } = "Customer"; // Customer, Seller, Rider, Admin, All
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Order"; // Order, Offer, Delivery, Payment, Product, Seller, Complaint, Return, KYC, System
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation Properties
        public User? Customer { get; set; }
        public Shop? Shop { get; set; }
        public Rider? Rider { get; set; }
    }
}
