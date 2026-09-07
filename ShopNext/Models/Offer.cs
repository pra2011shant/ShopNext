using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Offers
    /// Description: Point 41: Offers & Today's Deals Entity
    /// Fields: Product, MRP, Selling Price, Discount, Start Date, End Date, Status
    /// </summary>
    [Table("Offers")]
    public class Offer : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Mrp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [StringLength(50)]
        public string DiscountType { get; set; } = "Percentage"; // "Percentage", "Flat"

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        [StringLength(500)]
        public string? BannerUrl { get; set; }

        [StringLength(200)]
        public string? Tagline { get; set; } = "Deal of the Day";

        // Navigation Property
        public Product? Product { get; set; }
    }
}
