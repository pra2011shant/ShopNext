using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    public class Complaint : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(150)]
        public string Issue { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AttachmentUrl { get; set; }

        [StringLength(50)]
        public string Priority { get; set; } = "High"; // Low, Medium, High, Urgent

        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed

        public string? ResolutionNotes { get; set; }

        public DateTime? ResolvedDate { get; set; }

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
