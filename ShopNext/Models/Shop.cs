using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Shops
    /// Description: Merchant shops and physical store locations, including geolocation, bank settlement, and approval status.
    /// </summary>
    [Table("Shops")]
    public class Shop : BaseModel
    {
        public int Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsApproved { get; set; }
        public string? OwnerName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IfscCode { get; set; }
        public string? Password { get; set; }
    }
}
