using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Wishlists
    /// Description: Customer saved product items for quick retrieval and price/stock tracking.
    /// </summary>
    [Table("Wishlists")]
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
