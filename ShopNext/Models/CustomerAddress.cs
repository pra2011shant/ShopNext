using System;

namespace ShopNext.Models
{
    public class CustomerAddress : BaseModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string AddressType { get; set; } = "Home"; // Home, Work, Other
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = "New Delhi";
        public string State { get; set; } = "Delhi";
        public string Pincode { get; set; } = string.Empty;
        public string? Landmark { get; set; }
        public bool IsDefault { get; set; } = false;

        // Navigation Property
        public User? Customer { get; set; }
    }
}
