using System;

namespace ShopNext.Models
{
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
