using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.Complaints
    /// Description: Multi-party dispute tickets, 3-way arbitration statements, support escalation tiers, and chat threads.
    /// </summary>
    [Table("Complaints")]
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

        // Point 43: Complainant Information (Rider against Customer / Customer against Order)
        [StringLength(50)]
        public string ComplainantRole { get; set; } = "Customer"; // Customer, Rider, Seller

        public int? RiderId { get; set; }

        [StringLength(150)]
        public string? ComplainantName { get; set; }

        [StringLength(100)]
        public string? ReasonCategory { get; set; }

        // Point 45: Dispute Management 3-Way Statements
        [StringLength(1000)]
        public string? SellerStatement { get; set; }
        public DateTime? SellerStatementDate { get; set; }

        [StringLength(1000)]
        public string? RiderStatement { get; set; }
        public DateTime? RiderStatementDate { get; set; }

        // Point 78: Customer Support Ticket Threaded Conversation & Replies
        public string? ThreadMessagesJson { get; set; }

        // Point 79: Multi-Tier Support Escalation & High-Value Priority (Customer -> Support -> Seller -> Admin)
        public int EscalationLevel { get; set; } = 1; // 1 = Customer, 2 = Support Desk, 3 = Seller Arbitration, 4 = Executive Admin Escalation
        [StringLength(50)]
        public string EscalationStage { get; set; } = "Customer"; // Customer, Support, Seller, Admin
        public bool IsHighValueOrder { get; set; } = false;
        public decimal OrderAmount { get; set; } = 0;
        public DateTime? EscalatedToAdminDate { get; set; }
        [StringLength(500)]
        public string? EscalationReason { get; set; }

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }

    public class TicketMessageItem
    {
        public int Id { get; set; }
        public string SenderRole { get; set; } = "Customer"; // Customer, Admin, Support
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string SentAt { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
    }
}
