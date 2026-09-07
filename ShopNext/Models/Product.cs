using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Products
    /// Description: Master catalogue of store products, SKU inventory, multi-angle imagery, and approval workflow status.
    /// </summary>
    [Table("Products")]
    public class Product : BaseModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? SubCategory { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; } // Selling Price
        public decimal? Mrp { get; set; }
        public decimal? Discount { get; set; }
        public int Stock { get; set; } = 0;
        public string? Sku { get; set; }
        public string StockStatus { get; set; } = "InStock";
        public string ImageUrl { get; set; } = string.Empty; // Main Image
        public string? ImageFront { get; set; } // Front View
        public string? ImageBack { get; set; }  // Back View
        public string? ImageSide { get; set; }  // Side View
        public string? ImagePackaging { get; set; } // Packaging View

        // 22. Product Variants (Size, Color, Price, Stock, SKU)
        public bool HasVariants { get; set; } = false;
        public List<ProductVariant> Variants { get; set; } = new();

        // 37. Product Approval Lifecycle (Seller Adds -> Admin Review -> Approved -> Customer Visible)
        public bool IsApproved { get; set; } = true;
        public string ApprovalStatus { get; set; } = "Approved"; // "Pending", "Approved", "Rejected"
        public string? RejectionReason { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // Navigation Property
        public Shop? Shop { get; set; }
    }
}
