using System;

namespace ShopNext.Models
{
    public class Notification : BaseModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string RecipientRole { get; set; } = "Customer"; // Customer, Seller, Admin
        public int? ShopId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Order"; // Order, Offer, Delivery, Payment, Product, Seller, Complaint, Return, System
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation Property
        public User? Customer { get; set; }
    }
}
