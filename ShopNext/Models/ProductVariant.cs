using System;

namespace ShopNext.Models
{
    public class ProductVariant : BaseModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Size { get; set; }      // S, M, L, XL, XXL, etc.
        public string? Color { get; set; }     // Black, White, Blue, etc.
        public decimal Price { get; set; }     // Variant specific selling price
        public int Stock { get; set; } = 0;    // Variant specific stock
        public string? Sku { get; set; }       // Variant specific SKU
        public string? ImageUrl { get; set; }  // Optional variant-specific photo

        // Navigation Property
        public Product? Product { get; set; }
    }

    public class ProductVariantInputDto
    {
        public string? Size { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Sku { get; set; }
    }
}
