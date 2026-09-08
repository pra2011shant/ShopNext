using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopNext.Models
{
    /// <summary>
    /// Database Table Mapping: dbo.LoginHistories
    /// Tracks all authentication attempts (successful and failed), devices, IP, and session durations.
    /// </summary>
    [Table("LoginHistories")]
    public class LoginHistory : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Customer"; // Admin, Seller, Customer, Rider

        public DateTime LoginTime { get; set; } = DateTime.Now;

        public DateTime? LogoutTime { get; set; }

        public DateTime LastActivityTime { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? Browser { get; set; } // Chrome, Edge, Safari, Firefox

        [StringLength(100)]
        public string? Device { get; set; } // Desktop, Mobile, Tablet

        [StringLength(100)]
        public string? OperatingSystem { get; set; } // Windows 11, macOS, Android, iOS

        [StringLength(150)]
        public string? SessionId { get; set; }

        public bool IsSuccessful { get; set; } = true;

        [StringLength(500)]
        public string? FailureReason { get; set; }

        public bool IsActiveSession { get; set; } = true;

        public bool IsForceLoggedOut { get; set; } = false;
    }

    /// <summary>
    /// Database Table Mapping: dbo.UserSessions
    /// Real-time live connected user sessions with heartbeat, last seen, and force logout controls.
    /// </summary>
    [Table("UserSessions")]
    public class UserSession : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        public int? UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Customer";

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? Device { get; set; } = "Desktop";

        [StringLength(100)]
        public string? Browser { get; set; } = "Chrome";

        [StringLength(100)]
        public string? OperatingSystem { get; set; } = "Windows";

        public DateTime LoginTime { get; set; } = DateTime.Now;

        public DateTime LastSeenTime { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? LastPageVisited { get; set; } = "/";

        [StringLength(250)]
        public string? LastAction { get; set; } = "Page View";

        public DateTime? ExpiryTime { get; set; }
    }

    /// <summary>
    /// Database Table Mapping: dbo.UserActivities
    /// Comprehensive granular stream of all actions performed across the entire platform.
    /// </summary>
    [Table("UserActivities")]
    public class UserActivity : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Customer";

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // LOGIN, LOGOUT, CREATE, UPDATE, DELETE, APPROVE, REJECT, ASSIGN, CANCEL, REFUND, RETURN, PAYMENT, STATUS_CHANGE, PASSWORD_CHANGE, VIEW

        [Required]
        [StringLength(100)]
        public string Module { get; set; } = string.Empty; // Seller, Product, Order, Rider, Customer, Payment, Wallet, Return, System, Security

        [StringLength(100)]
        public string? Entity { get; set; } // Order #1024, Product #55, Store #12

        public int? EntityId { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? Device { get; set; }

        [StringLength(100)]
        public string? Browser { get; set; }
    }

    /// <summary>
    /// Database Table Mapping: dbo.EntityChangeLogs
    /// Enterprise-grade field-level Before & After data delta tracking.
    /// </summary>
    [Table("EntityChangeLogs")]
    public class EntityChangeLog : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Order, Product, Shop, User, Inventory

        public int EntityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = "UPDATE"; // CREATE, UPDATE, DELETE, STATUS_CHANGE

        [StringLength(100)]
        public string? FieldName { get; set; } // Status, Price, RiderId, Stock, IsApproved

        [StringLength(2000)]
        public string? OldValue { get; set; }

        [StringLength(2000)]
        public string? NewValue { get; set; }

        public int? ChangedByUserId { get; set; }

        [StringLength(200)]
        public string? ChangedByUserName { get; set; }

        [StringLength(50)]
        public string? ChangedByUserRole { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Database Table Mapping: dbo.SoftDeleteLogs
    /// Records deleted items with data snapshots, deletion reason, and one-click restore capabilities.
    /// </summary>
    [Table("SoftDeleteLogs")]
    public class SoftDeleteLog : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Product, Shop, Coupon, Category, User

        public int EntityId { get; set; }

        [StringLength(250)]
        public string? EntityTitle { get; set; }

        public int? DeletedByUserId { get; set; }

        [StringLength(200)]
        public string? DeletedByUserName { get; set; }

        [StringLength(50)]
        public string? DeletedByUserRole { get; set; }

        public DateTime DeletedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        public string? SnapshotDataJson { get; set; }

        public bool IsRestored { get; set; } = false;

        public DateTime? RestoredAt { get; set; }

        [StringLength(200)]
        public string? RestoredBy { get; set; }
    }

    /// <summary>
    /// Database Table Mapping: dbo.EntityViewLogs
    /// Seen/Viewed tracking to monitor who inspected an order, product, notification, or return dispute.
    /// </summary>
    [Table("EntityViewLogs")]
    public class EntityViewLog : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // Order, Notification, ReturnRequest, Product, SellerKYC

        public int EntityId { get; set; }

        public int? ViewedByUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string ViewedByUserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ViewedByUserRole { get; set; } = "Admin"; // Admin, Seller, Rider, Customer

        public DateTime ViewedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? ExtraInfo { get; set; }
    }

    /// <summary>
    /// Database Table Mapping: dbo.SecurityThreatAlerts
    /// Real-time threat detection alerts (failed logins, multi-accounts, COD abuse, suspicious refunds).
    /// </summary>
    [Table("SecurityThreatAlerts")]
    public class SecurityThreatAlert : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AlertType { get; set; } = string.Empty; // FailedLogins, MultiAccount, HighCodRejection, SuspiciousRefund, VelocitySpike

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public int? AffectedUserId { get; set; }

        [StringLength(200)]
        public string? AffectedUserName { get; set; }

        [StringLength(100)]
        public string? IpAddress { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.Now;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        [StringLength(200)]
        public string? ResolvedBy { get; set; }
    }

    /// <summary>
    /// Aggregate ViewModel for the System Monitoring Command Center
    /// </summary>
    public class SystemMonitoringDashboardViewModel
    {
        // KPI Overview Counts
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int OfflineUsers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalRiders { get; set; }
        public int TotalCustomers { get; set; }
        public int TodayLogins { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int TodayOrders { get; set; }
        public int TodayActivities { get; set; }
        public DateTime? LastActivityTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLogoutTime { get; set; }

        // Data Collections
        public List<UserSession> ActiveSessions { get; set; } = new();
        public List<UserActivity> RecentActivities { get; set; } = new();
        public List<LoginHistory> RecentLogins { get; set; } = new();
        public List<EntityChangeLog> RecentAuditDiffs { get; set; } = new();
        public List<SoftDeleteLog> DeletedRecords { get; set; } = new();
        public List<EntityViewLog> RecentViewLogs { get; set; } = new();
        public List<SecurityThreatAlert> SecurityAlerts { get; set; } = new();

        // Filters
        public string? RoleFilter { get; set; }
        public string? ModuleFilter { get; set; }
        public string? SearchQuery { get; set; }
    }
}
