using System.Collections.Generic;

namespace ShopNext.Models
{
    /// <summary>
    /// Point 30: Seller Shop Profile ViewModel
    /// Encapsulates customer-facing merchant store profile, statistics, ratings, catalog, and reviews.
    /// </summary>
    public class SellerShopProfileViewModel
    {
        public Shop Shop { get; set; } = null!;
        public string ShopName { get; set; } = string.Empty;
        public double Rating { get; set; } = 4.5;
        public int TotalProducts { get; set; } = 120;
        public int TotalOrders { get; set; } = 500;
        public string Location { get; set; } = "Patna";
        public string City { get; set; } = "Patna";
        public string? Address { get; set; }
        public string? Category { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OwnerName { get; set; }
        public int ReviewCount { get; set; } = 1;
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double Distance { get; set; } = -1.0;
        public bool IsVerifiedPartner { get; set; } = true;
    }
}
