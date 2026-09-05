using System;

namespace ShopNext.Models
{
    public class OrderEvidence : BaseModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? OrderItemId { get; set; }

        // PackingPhoto, OriginalProductPhoto, DeliveryPhoto, CustomerReturnPhoto, ReturnPickupPhoto, SellerEvidence, RiderEvidence
        public string EvidenceType { get; set; } = "PackingPhoto";

        // File path or remote URL (e.g. /uploads/evidence/...)
        public string PhotoUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Seller, Rider, Customer, Admin
        public string UploadedByRole { get; set; } = "Seller";
        public string? UploadedByName { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;
        public bool IsVerified { get; set; } = false;
        public string? VerifiedBy { get; set; }

        // Metadata JSON for extra telemetry (GPS, camera timestamp, device info)
        public string? MetadataJson { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public OrderItem? OrderItem { get; set; }
    }
}
