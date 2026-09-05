using System;
using System.ComponentModel.DataAnnotations;

namespace ShopNext.Models
{
    public class Category : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Icon { get; set; } = "fa-solid fa-layer-group";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }
}
