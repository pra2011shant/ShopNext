using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.CustomerAddresses
    /// Description: Customer shipping and billing delivery addresses, geolocation landmarks, and default selections.
    /// </summary>
    [Table("CustomerAddresses")]
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
