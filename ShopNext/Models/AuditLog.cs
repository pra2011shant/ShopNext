using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    [Table("AuditLogs")]
    public class AuditLog : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(100)]
        public string? UserName { get; set; }

        [StringLength(50)]
        public string? UserRole { get; set; } // Admin, Seller, Customer, Rider, System

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // e.g. "Product_Created", "Seller_Approved", "Order_Cancelled", "User_Blocked", "Exception"

        [StringLength(100)]
        public string? EntityName { get; set; } // e.g. "Product", "Shop", "Order", "User"

        public int? EntityId { get; set; }

        [StringLength(2000)]
        public string? Details { get; set; }

        [StringLength(100)]
        public string? IpAddress { get; set; }
    }
}
