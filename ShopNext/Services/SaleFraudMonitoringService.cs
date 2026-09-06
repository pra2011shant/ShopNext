using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class SaleFraudMonitoringService : ISaleFraudMonitoringService
    {
        private readonly ShopNextDbContext _context;
        private readonly IAuditService _auditService;

        // In-memory persistent state for simulated live fraud alerts & admin actions
        private static readonly ConcurrentDictionary<int, AdminSaleFraudAlertDto> _alertsStore = new();

        static SaleFraudMonitoringService()
        {
            SeedInitialFraudAlerts();
        }

        public SaleFraudMonitoringService(ShopNextDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private static void SeedInitialFraudAlerts()
        {
            // Alert 1: Customer A (Same coupon repeatedly misuse)
            _alertsStore[1] = new AdminSaleFraudAlertDto
            {
                Id = 1,
                CustomerId = 101,
                CustomerDisplayName = "Customer A",
                RealCustomerName = "Customer A (Rahul Sharma)",
                CustomerEmail = "rahul.dealhunter@gmail.com",
                CustomerPhone = "+91 98765 43210",
                RiskScore = 94,
                RiskLevel = "Critical",
                AbuseCategory = "Same coupon repeatedly misuse",
                TriggerSummary = "Same coupon BIGSALE500 repeatedly misused across 14 linked accounts",
                Details = "High-frequency coupon exploitation during Mega Shopping Sale. Single device fingerprint created 14 linked accounts to bypass the 1-per-user coupon limitation.",
                DetectedAt = DateTime.Now.AddMinutes(-25),
                Status = "Under Review",
                IpAddress = "103.21.244.18",
                DeviceFingerprint = "DEV-FP-98A1B2-CHROME-WIN",
                Location = "Patna, Bihar",
                TotalOrdersDuringSale = 14,
                CancellationsDuringSale = 0,
                ReturnsDuringSale = 0,
                CouponAttemptsCount = 28,
                CouponsUsedCount = 14,
                LinkedAccountsCount = 14,
                PreventedFraudAmount = 7000,
                PreventedFraudAmountFormatted = "₹7,000",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "10:45 AM", EventName = "Coupon Applied", Severity = "Info", Message = "Coupon BIGSALE500 redeemed on Order #ORD-7811 (User A1)" },
                    new SaleFraudIncidentLogDto { Timestamp = "10:48 AM", EventName = "Linked Device Match", Severity = "Warning", Message = "Coupon BIGSALE500 redeemed on Order #ORD-7819 (User A2, Same Device FP)" },
                    new SaleFraudIncidentLogDto { Timestamp = "10:52 AM", EventName = "Multi-Account Pattern", Severity = "Warning", Message = "Coupon BIGSALE500 redeemed on Order #ORD-7835 (User A3, Same IP Subnet)" },
                    new SaleFraudIncidentLogDto { Timestamp = "11:05 AM", EventName = "🚨 Threshold Exceeded", Severity = "Critical", Message = "14th coupon redemption detected on identical device fingerprint cluster. Flagged for Admin Review." }
                }
            };

            // Alert 2: Customer B (Excessive coupon attempts)
            _alertsStore[2] = new AdminSaleFraudAlertDto
            {
                Id = 2,
                CustomerId = 102,
                CustomerDisplayName = "Customer B",
                RealCustomerName = "Customer B (Amit Kumar)",
                CustomerEmail = "amit.k.bots@outlook.com",
                CustomerPhone = "+91 98112 33445",
                RiskScore = 88,
                RiskLevel = "High Risk",
                AbuseCategory = "Excessive coupon attempts",
                TriggerSummary = "48 rapid coupon brute-force attempts in under 3 minutes",
                Details = "Automated bot script detected attempting high-speed brute-force variations of flash sale promo codes (MEGASALE500, MEGASALE1000, FLASHOFF70). 48 failed attempts in 180 seconds.",
                DetectedAt = DateTime.Now.AddMinutes(-40),
                Status = "Under Review",
                IpAddress = "45.118.60.22",
                DeviceFingerprint = "DEV-FP-BOT-PYTHON-REQUESTS",
                Location = "Delhi NCR",
                TotalOrdersDuringSale = 2,
                CancellationsDuringSale = 1,
                ReturnsDuringSale = 0,
                CouponAttemptsCount = 48,
                CouponsUsedCount = 1,
                LinkedAccountsCount = 3,
                PreventedFraudAmount = 15000,
                PreventedFraudAmountFormatted = "₹15,000",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "11:15 AM", EventName = "Rapid Query Failure", Severity = "Warning", Message = "Tried code MEGASALE500 (Failed - Invalid format)" },
                    new SaleFraudIncidentLogDto { Timestamp = "11:15 AM", EventName = "Rapid Query Failure", Severity = "Warning", Message = "Tried code MEGASALE1000 (Failed - Non-existent)" },
                    new SaleFraudIncidentLogDto { Timestamp = "11:16 AM", EventName = "Rate Limit Warning", Severity = "Warning", Message = "25 promo queries in 60 seconds from same IP address" },
                    new SaleFraudIncidentLogDto { Timestamp = "11:17 AM", EventName = "🚨 Bot Attack Triggered", Severity = "Critical", Message = "48 failed coupon API requests within 3 minutes. Bot protection triggered." }
                }
            };

            // Alert 3: Customer C (Abnormal cancellations & inventory locking)
            _alertsStore[3] = new AdminSaleFraudAlertDto
            {
                Id = 3,
                CustomerId = 103,
                CustomerDisplayName = "Customer C",
                RealCustomerName = "Customer C (Pooja Mishra)",
                CustomerEmail = "pooja.mishra.99@gmail.com",
                CustomerPhone = "+91 97234 56789",
                RiskScore = 82,
                RiskLevel = "High Risk",
                AbuseCategory = "Abnormal cancellations",
                TriggerSummary = "12 consecutive flash deal orders placed & cancelled to hoard inventory",
                Details = "Customer repeatedly locked 12 units of Samsung Galaxy S24 during flash sale window by placing orders and cancelling within 90 seconds, causing inventory denial-of-service for legitimate buyers.",
                DetectedAt = DateTime.Now.AddMinutes(-55),
                Status = "Under Review",
                IpAddress = "117.200.12.89",
                DeviceFingerprint = "DEV-FP-33C4D5-ANDROID-APP",
                Location = "Bengaluru, Karnataka",
                TotalOrdersDuringSale = 12,
                CancellationsDuringSale = 12,
                ReturnsDuringSale = 0,
                CouponAttemptsCount = 6,
                CouponsUsedCount = 0,
                LinkedAccountsCount = 2,
                PreventedFraudAmount = 396000,
                PreventedFraudAmountFormatted = "₹3.96 Lakh",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "11:30 AM", EventName = "Order Placed", Severity = "Info", Message = "Order #ORD-8012 (2 units Samsung Mobile) reserved." },
                    new SaleFraudIncidentLogDto { Timestamp = "11:31 AM", EventName = "Immediate Cancellation", Severity = "Warning", Message = "Order #ORD-8012 cancelled after 75s (Inventory restored)." },
                    new SaleFraudIncidentLogDto { Timestamp = "11:32 AM", EventName = "Order Placed", Severity = "Info", Message = "Order #ORD-8025 (2 units Samsung Mobile) reserved." },
                    new SaleFraudIncidentLogDto { Timestamp = "11:35 AM", EventName = "🚨 Inventory Locking Detected", Severity = "Critical", Message = "12th consecutive lock-and-cancel cycle detected. 100% cancellation rate." }
                }
            };

            // Alert 4: Customer D (Multiple suspicious accounts)
            _alertsStore[4] = new AdminSaleFraudAlertDto
            {
                Id = 4,
                CustomerId = 104,
                CustomerDisplayName = "Customer D",
                RealCustomerName = "Customer D (Vikram Singh)",
                CustomerEmail = "vikram.syndicate@tempmail.io",
                CustomerPhone = "+91 96543 21987",
                RiskScore = 79,
                RiskLevel = "Suspicious",
                AbuseCategory = "Multiple suspicious accounts",
                TriggerSummary = "6 synthetic profiles created in 5 mins from same subnet",
                Details = "Cluster of 6 synthetic customer accounts created within 5 minutes using disposable temporary emails and matching delivery address 'Flat 402, Lotus Residency, Pune'.",
                DetectedAt = DateTime.Now.AddHours(-1.5),
                Status = "Under Review",
                IpAddress = "182.74.19.45",
                DeviceFingerprint = "DEV-FP-SYNTH-CLUSTER-6",
                Location = "Pune, Maharashtra",
                TotalOrdersDuringSale = 6,
                CancellationsDuringSale = 1,
                ReturnsDuringSale = 0,
                CouponAttemptsCount = 12,
                CouponsUsedCount = 6,
                LinkedAccountsCount = 6,
                PreventedFraudAmount = 18000,
                PreventedFraudAmountFormatted = "₹18,000",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "10:10 AM", EventName = "Rapid Registration", Severity = "Warning", Message = "6 accounts registered from same /24 IP subnet within 300 seconds." },
                    new SaleFraudIncidentLogDto { Timestamp = "10:15 AM", EventName = "Address Duplication", Severity = "Warning", Message = "All 6 accounts saved identical delivery address." },
                    new SaleFraudIncidentLogDto { Timestamp = "10:20 AM", EventName = "🚨 Multi-Account Risk", Severity = "Critical", Message = "Synthetic account ring identified aiming to scalp flash sale stock." }
                }
            };

            // Alert 5: Customer E (Abnormal returns)
            _alertsStore[5] = new AdminSaleFraudAlertDto
            {
                Id = 5,
                CustomerId = 105,
                CustomerDisplayName = "Customer E",
                RealCustomerName = "Customer E (Neha Verma)",
                CustomerEmail = "neha.v.wardrobe@gmail.com",
                CustomerPhone = "+91 91234 56780",
                RiskScore = 76,
                RiskLevel = "Suspicious",
                AbuseCategory = "Abnormal returns",
                TriggerSummary = "9 out of 10 sale orders marked for return within 24 hours",
                Details = "Customer ordered 10 discounted premium designer fashion items during Mega Sale and filed return pickup requests for 9 items immediately upon delivery, exhibiting tag swapping risk pattern.",
                DetectedAt = DateTime.Now.AddHours(-2),
                Status = "Under Review",
                IpAddress = "49.36.140.78",
                DeviceFingerprint = "DEV-FP-IOS-SAFARI-77",
                Location = "Jaipur, Rajasthan",
                TotalOrdersDuringSale = 10,
                CancellationsDuringSale = 0,
                ReturnsDuringSale = 9,
                CouponAttemptsCount = 4,
                CouponsUsedCount = 2,
                LinkedAccountsCount = 1,
                PreventedFraudAmount = 32000,
                PreventedFraudAmountFormatted = "₹32,000",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "09:00 AM", EventName = "High Value Purchases", Severity = "Info", Message = "10 fashion items purchased with 30% Mega Sale discount." },
                    new SaleFraudIncidentLogDto { Timestamp = "03:00 PM", EventName = "Bulk Return Requests", Severity = "Warning", Message = "9 return pickup requests filed in single session." },
                    new SaleFraudIncidentLogDto { Timestamp = "03:15 PM", EventName = "🚨 Wardrobing Pattern", Severity = "Critical", Message = "90% return rate on festival apparel. Audit recommended before refund." }
                }
            };

            // Alert 6: Customer F (Unusual order patterns)
            _alertsStore[6] = new AdminSaleFraudAlertDto
            {
                Id = 6,
                CustomerId = 106,
                CustomerDisplayName = "Customer F",
                RealCustomerName = "Customer F (Rajesh Gupta)",
                CustomerEmail = "rajesh.traders.bulk@gmail.com",
                CustomerPhone = "+91 99887 76655",
                RiskScore = 74,
                RiskLevel = "Suspicious",
                AbuseCategory = "Unusual order patterns",
                TriggerSummary = "High-velocity bulk purchase spike (₹3.4 Lakhs in 45 seconds)",
                Details = "Placed 5 concurrent orders for high-value Smart 4K QLED TVs within 45 seconds using multiple automated browser tabs, circumventing B2C retail sale limits.",
                DetectedAt = DateTime.Now.AddHours(-3),
                Status = "Under Review",
                IpAddress = "115.112.88.19",
                DeviceFingerprint = "DEV-FP-CHROME-HEADLESS-99",
                Location = "Ahmedabad, Gujarat",
                TotalOrdersDuringSale = 5,
                CancellationsDuringSale = 0,
                ReturnsDuringSale = 0,
                CouponAttemptsCount = 5,
                CouponsUsedCount = 5,
                LinkedAccountsCount = 1,
                PreventedFraudAmount = 25000,
                PreventedFraudAmountFormatted = "₹25,000",
                IncidentLogs = new List<SaleFraudIncidentLogDto>
                {
                    new SaleFraudIncidentLogDto { Timestamp = "08:15 AM", EventName = "Concurrent Checkout", Severity = "Warning", Message = "5 checkout requests dispatched in 45 seconds." },
                    new SaleFraudIncidentLogDto { Timestamp = "08:16 AM", EventName = "Commercial B2B Reselling", Severity = "Warning", Message = "High-volume consumer deal exploit for offline resale." },
                    new SaleFraudIncidentLogDto { Timestamp = "08:20 AM", EventName = "🚨 Unusual Velocity", Severity = "Critical", Message = "Order velocity 20x higher than normal consumer baseline." }
                }
            };
        }

        public async Task<SaleFraudDashboardSummaryDto> GetSaleFraudDashboardSummaryAsync()
        {
            var alerts = await GetSuspiciousActivitiesAsync();
            var totalCount = alerts.Count;
            var criticalCount = alerts.Count(a => a.RiskLevel == "Critical");
            var highRiskCount = alerts.Count(a => a.RiskLevel == "High Risk");
            var underReviewCount = alerts.Count(a => a.Status == "Under Review");
            var blockedCount = alerts.Count(a => a.Status == "Blocked");
            var clearedCount = alerts.Count(a => a.Status == "Cleared");
            var preventedLoss = alerts.Sum(a => a.PreventedFraudAmount);

            return new SaleFraudDashboardSummaryDto
            {
                TotalSuspiciousActivitiesCount = totalCount,
                CriticalCount = criticalCount,
                HighRiskCount = highRiskCount,
                UnderReviewCount = underReviewCount,
                BlockedCount = blockedCount,
                ClearedCount = clearedCount,
                PreventedLossAmount = preventedLoss,
                PreventedLossFormatted = $"₹{preventedLoss:N0}",
                SuspiciousAlerts = alerts
            };
        }

        public Task<List<AdminSaleFraudAlertDto>> GetSuspiciousActivitiesAsync()
        {
            var list = _alertsStore.Values.OrderByDescending(a => a.RiskScore).ToList();
            return Task.FromResult(list);
        }

        public Task<AdminSaleFraudAlertDto?> GetFraudAlertByIdAsync(int alertId)
        {
            _alertsStore.TryGetValue(alertId, out var alert);
            return Task.FromResult(alert);
        }

        public async Task<(bool Success, string Message, AdminSaleFraudAlertDto? UpdatedAlert)> ReviewFraudAlertAsync(int alertId, string action, string? adminNotes)
        {
            if (!_alertsStore.TryGetValue(alertId, out var alert))
            {
                return (false, "Suspicious activity alert not found.", null);
            }

            action = action?.ToLowerInvariant() ?? "restrict_coupons";
            string statusMsg = "";

            switch (action)
            {
                case "block":
                    alert.Status = "Blocked";
                    alert.AdminActionTaken = "Account Suspended & Banned";
                    statusMsg = $"🚫 Account for {alert.CustomerDisplayName} ({alert.RealCustomerName}) has been BLOCKED and banned from future sale events.";
                    break;

                case "restrict_coupons":
                    alert.Status = "Coupons Restricted";
                    alert.AdminActionTaken = "Sale Coupon Privileges Revoked";
                    statusMsg = $"🎟️ Coupon privileges revoked for {alert.CustomerDisplayName}. Customer cannot apply sale coupons.";
                    break;

                case "block_cod":
                    alert.Status = "COD Disabled";
                    alert.AdminActionTaken = "Cash on Delivery Disabled (Prepaid Only)";
                    statusMsg = $"💵 COD disabled for {alert.CustomerDisplayName}. User must pay prepaid for all future orders.";
                    break;

                case "clear":
                case "dismiss":
                    alert.Status = "Cleared";
                    alert.AdminActionTaken = "Flag Cleared (Verified Legitimate)";
                    statusMsg = $"✅ Suspicious activity flag cleared for {alert.CustomerDisplayName}. Marked as legitimate.";
                    break;

                default:
                    alert.Status = "Under Review";
                    alert.AdminActionTaken = "Audit in Progress";
                    statusMsg = $"Audit status updated for {alert.CustomerDisplayName}.";
                    break;
            }

            alert.AdminNotes = adminNotes ?? $"Admin reviewed alert on {DateTime.Now:dd MMM yyyy, hh:mm tt}. Action: {alert.AdminActionTaken}.";
            alert.IncidentLogs.Add(new SaleFraudIncidentLogDto
            {
                Timestamp = DateTime.Now.ToString("hh:mm tt"),
                EventName = "Admin Action",
                Severity = alert.Status == "Cleared" ? "Info" : "Critical",
                Message = $"Admin took action: '{alert.AdminActionTaken}'. Note: {adminNotes ?? "No extra notes"}"
            });

            _alertsStore[alertId] = alert;

            await _auditService.LogAsync("SaleFraudMonitoring", "AdminReview", 
                alertId, 
                $"Admin reviewed fraud alert #{alertId} for {alert.CustomerDisplayName} ({alert.RealCustomerName}). Action: {alert.AdminActionTaken}", 
                alert.CustomerId, "Admin", "Admin", null);

            return (true, statusMsg, alert);
        }

        public Task<SaleFraudCheckResultDto> EvaluateSaleTransactionAsync(int customerId, string couponCode, string ipAddress, string deviceId, decimal orderAmount)
        {
            // Real-time evaluation against known abuse signatures
            var matchingAlert = _alertsStore.Values.FirstOrDefault(a => a.CustomerId == customerId || a.IpAddress == ipAddress || a.DeviceFingerprint == deviceId);
            
            if (matchingAlert != null && (matchingAlert.Status == "Blocked" || matchingAlert.Status == "Coupons Restricted"))
            {
                return Task.FromResult(new SaleFraudCheckResultDto
                {
                    IsSuspicious = true,
                    RiskScore = matchingAlert.RiskScore,
                    RiskLevel = matchingAlert.RiskLevel,
                    AbuseCategory = matchingAlert.AbuseCategory,
                    TriggerReason = matchingAlert.TriggerSummary,
                    ShouldBlockTransaction = matchingAlert.Status == "Blocked"
                });
            }

            return Task.FromResult(new SaleFraudCheckResultDto
            {
                IsSuspicious = false,
                RiskScore = 15,
                RiskLevel = "Low",
                ShouldBlockTransaction = false
            });
        }
    }
}
