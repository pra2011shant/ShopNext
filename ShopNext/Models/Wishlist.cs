using System;

namespace ShopNext.Models
{
    public class Wishlist : BaseModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public User? Customer { get; set; }
        public Product? Product { get; set; }
    }
}
