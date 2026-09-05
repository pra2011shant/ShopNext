using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    public class Rider : BaseModel
    {
        public int Id { get; set; }
        public string RiderName { get; set; } = string.Empty;
        [NotMapped]
        public string Name { get => RiderName; set => RiderName = value; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? VehicleNumber { get; set; } = "BR-01-AB-1234";
        public decimal CurrentLatitude { get; set; }
        public decimal CurrentLongitude { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
