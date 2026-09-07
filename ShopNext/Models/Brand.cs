using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Brands
    /// Description: Brand catalogue, official manufacturer logos, category classification, and brand ratings.
    /// </summary>
    [Table("Brands")]
    public class Brand : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? LogoUrl { get; set; }

        public double Rating { get; set; } = 4.8;
    }
}

