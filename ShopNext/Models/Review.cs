using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Reviews
    /// Description: Customer product and shop ratings, comments, feedback moderation, and visibility flags.
    /// </summary>
    [Table("Reviews")]
    public class Review : BaseModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? ProductId { get; set; }
        public int ShopId { get; set; }
        public int? OrderId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public bool IsHidden { get; set; } = false;
        public string? ModerationReason { get; set; }

        // Navigation & Display Properties
        public User? Customer { get; set; }
        public Product? Product { get; set; }
        public Shop? Shop { get; set; }
        public string? CustomerName { get; set; }
    }
}
