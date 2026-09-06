using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class SaleCampaignService : ISaleCampaignService
    {
        private readonly ShopNextDbContext _context;
        private readonly IAuditService _auditService;

        // In-memory campaign state with thread-safe access
        private static readonly object _lock = new object();
        private static readonly List<AdminSaleCampaignDto> _inMemoryCampaigns = new List<AdminSaleCampaignDto>();
        private static int _nextId = 101;

        static SaleCampaignService()
        {
            InitializeSeedCampaigns();
        }

        public SaleCampaignService(ShopNextDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private static void InitializeSeedCampaigns()
        {
            lock (_lock)
            {
                if (_inMemoryCampaigns.Any()) return;

                var now = DateTime.Now;

                // 1. Scheduled Mega Sale - Starts in exactly 2 Days, 8 Hours, 25 Minutes (Matching Point 51 Prompt Example)
                var megaStart = now.AddDays(2).AddHours(8).AddMinutes(25);
                var megaEnd = megaStart.AddDays(5);

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 1,
                    CampaignName = "🔥 Mega Shopping Sale",
                    Tagline = "Biggest Super Saver Sale of the Season!",
                    BannerTheme = "flame-red",
                    StartDateTime = megaStart,
                    EndDateTime = megaEnd,
                    StartDate = megaStart.ToString("dd MMM yyyy"),
                    StartTime = megaStart.ToString("hh:mm tt"),
                    EndDate = megaEnd.ToString("dd MMM yyyy"),
                    EndTime = megaEnd.ToString("hh:mm tt"),
                    Status = "Upcoming",
                    IsActive = false,
                    IsAutoScheduled = true,
                    AutoLifecycleStatus = "Auto-Scheduled (Starts Automatically)",
                    CountdownLabel = "Sale Starts In:",
                    CountdownFormatted = "02 Days 08 Hours 25 Minutes",
                    CountdownDays = 2,
                    CountdownHours = 8,
                    CountdownMinutes = 25,
                    CountdownSeconds = 0,
                    TotalSecondsRemaining = (long)(megaStart - now).TotalSeconds,
                    DefaultDiscountPct = 25,
                    CouponCode = "MEGASALE",
                    MinOrderAmount = 999,
                    MaxDiscountAmount = 2500,
                    ProductsScope = "All Catalog Products in Categories",
                    SellersScope = "All 250 Verified Sellers",
                    ParticipatingSellersCount = 250,
                    TotalOrdersGenerated = 1845,
                    TotalGrossRevenue = 5420000,
                    Description = "Major festival shopping event with category-tier discounts across Mobiles, Fashion, Electronics, and Home.",
                    CreatedDate = now.AddDays(-2).ToString("dd MMM yyyy"),
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 20, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 40, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 30, IconClass = "fa-laptop", BadgeColor = "warning" },
                        new CampaignCategoryDiscountDto { CategoryName = "Home & Living", DiscountPercentage = 35, IconClass = "fa-house", BadgeColor = "success" }
                    }
                });

                // 2. Active Flash Sale - Started 3 hours ago, Ends in 21 hours
                var flashStart = now.AddHours(-3);
                var flashEnd = now.AddHours(21);

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 2,
                    CampaignName = "🌙 Midnight Super Flash Sale",
                    Tagline = "24-Hour Exclusive Night Blowout",
                    BannerTheme = "midnight-violet",
                    StartDateTime = flashStart,
                    EndDateTime = flashEnd,
                    StartDate = flashStart.ToString("dd MMM yyyy"),
                    StartTime = flashStart.ToString("hh:mm tt"),
                    EndDate = flashEnd.ToString("dd MMM yyyy"),
                    EndTime = flashEnd.ToString("hh:mm tt"),
                    Status = "Live Now",
                    IsActive = true,
                    IsAutoScheduled = true,
                    AutoLifecycleStatus = "Auto-Active (Live Now)",
                    CountdownLabel = "Sale Ends In:",
                    CountdownFormatted = "00 Days 21 Hours 00 Minutes",
                    CountdownDays = 0,
                    CountdownHours = 21,
                    CountdownMinutes = 0,
                    CountdownSeconds = 0,
                    TotalSecondsRemaining = (long)(flashEnd - now).TotalSeconds,
                    DefaultDiscountPct = 30,
                    CouponCode = "MIDNIGHT",
                    MinOrderAmount = 499,
                    MaxDiscountAmount = 1200,
                    ProductsScope = "Flash Sale Catalog Items",
                    SellersScope = "Express Dispatch Sellers",
                    ParticipatingSellersCount = 85,
                    TotalOrdersGenerated = 412,
                    TotalGrossRevenue = 890000,
                    Description = "High-urgency midnight flash sale with steep clearance discounts on apparel and smartphone accessories.",
                    CreatedDate = now.AddDays(-1).ToString("dd MMM yyyy"),
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 15, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 60, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 25, IconClass = "fa-laptop", BadgeColor = "warning" }
                    }
                });

                // 3. Upcoming Grand Autumn Festival - Starts in 14 days
                var festStart = now.AddDays(14);
                var festEnd = festStart.AddDays(8);

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 3,
                    CampaignName = "⚡ Great Indian Festival Days",
                    Tagline = "India's Grand Autumn Festive Extravaganza",
                    BannerTheme = "festive-gold",
                    StartDateTime = festStart,
                    EndDateTime = festEnd,
                    StartDate = festStart.ToString("dd MMM yyyy"),
                    StartTime = festStart.ToString("hh:mm tt"),
                    EndDate = festEnd.ToString("dd MMM yyyy"),
                    EndTime = festEnd.ToString("hh:mm tt"),
                    Status = "Upcoming",
                    IsActive = false,
                    IsAutoScheduled = true,
                    AutoLifecycleStatus = "Auto-Scheduled (Starts Automatically)",
                    CountdownLabel = "Sale Starts In:",
                    CountdownFormatted = "14 Days 00 Hours 00 Minutes",
                    CountdownDays = 14,
                    CountdownHours = 0,
                    CountdownMinutes = 0,
                    CountdownSeconds = 0,
                    TotalSecondsRemaining = (long)(festStart - now).TotalSeconds,
                    DefaultDiscountPct = 35,
                    CouponCode = "FESTIVAL500",
                    MinOrderAmount = 1499,
                    MaxDiscountAmount = 5000,
                    ProductsScope = "All Verified Flagships & Deals",
                    SellersScope = "All 250 Verified Sellers",
                    ParticipatingSellersCount = 250,
                    TotalOrdersGenerated = 0,
                    TotalGrossRevenue = 0,
                    Description = "Pre-festive shopping festival with massive discounts on high-ticket appliances and premium fashion.",
                    CreatedDate = now.ToString("dd MMM yyyy"),
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 25, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 50, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 45, IconClass = "fa-laptop", BadgeColor = "warning" },
                        new CampaignCategoryDiscountDto { CategoryName = "Groceries", DiscountPercentage = 20, IconClass = "fa-basket-shopping", BadgeColor = "success" }
                    }
                });

                // 4. Past / Expired Campaign
                var expStart = now.AddDays(-6);
                var expEnd = now.AddDays(-1);

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 4,
                    CampaignName = "🎉 Weekend Clearance Super Sale",
                    Tagline = "End of Month Clearance",
                    BannerTheme = "electric-blue",
                    StartDateTime = expStart,
                    EndDateTime = expEnd,
                    StartDate = expStart.ToString("dd MMM yyyy"),
                    StartTime = expStart.ToString("hh:mm tt"),
                    EndDate = expEnd.ToString("dd MMM yyyy"),
                    EndTime = expEnd.ToString("hh:mm tt"),
                    Status = "Expired",
                    IsActive = false,
                    IsAutoScheduled = true,
                    AutoLifecycleStatus = "Auto-Expired (Completed)",
                    CountdownLabel = "Campaign Expired",
                    CountdownFormatted = "Campaign Expired",
                    CountdownDays = 0,
                    CountdownHours = 0,
                    CountdownMinutes = 0,
                    CountdownSeconds = 0,
                    TotalSecondsRemaining = 0,
                    DefaultDiscountPct = 40,
                    CouponCode = "CLEARANCE",
                    MinOrderAmount = 499,
                    MaxDiscountAmount = 1000,
                    ProductsScope = "Clearance Overstock SKUs",
                    SellersScope = "All 250 Verified Sellers",
                    ParticipatingSellersCount = 180,
                    TotalOrdersGenerated = 960,
                    TotalGrossRevenue = 1850000,
                    Description = "Clearance event concluded successfully with zero manual intervention required.",
                    CreatedDate = now.AddDays(-7).ToString("dd MMM yyyy"),
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 60, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Beauty & Care", DiscountPercentage = 45, IconClass = "fa-sparkles", BadgeColor = "primary" }
                    }
                });
            }
        }

        public Task<List<AdminSaleCampaignDto>> GetAllCampaignsAsync()
        {
            lock (_lock)
            {
                SyncDynamicTimersAndStatuses();
                return Task.FromResult(_inMemoryCampaigns.OrderByDescending(c => c.IsActive).ThenBy(c => c.Id).ToList());
            }
        }

        public Task<AdminSaleCampaignDto?> GetCampaignByIdAsync(int id)
        {
            lock (_lock)
            {
                SyncDynamicTimersAndStatuses();
                var campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == id);
                return Task.FromResult(campaign);
            }
        }

        public async Task<AdminSaleCampaignDto> CreateCampaignAsync(CreateOrEditSaleCampaignRequest request)
        {
            var categoryDiscounts = ParseCategoryDiscounts(request.CategoryDiscountsJson);
            if (!categoryDiscounts.Any())
            {
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 20, IconClass = "fa-mobile-screen", BadgeColor = "info" });
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 40, IconClass = "fa-shirt", BadgeColor = "danger" });
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 30, IconClass = "fa-laptop", BadgeColor = "warning" });
            }

            var startDt = ParseDateTime(request.StartDate, request.StartTime) ?? DateTime.Now.AddDays(2).AddHours(8).AddMinutes(25);
            var endDt = ParseDateTime(request.EndDate, request.EndTime) ?? startDt.AddDays(5);

            var newCampaign = new AdminSaleCampaignDto
            {
                CampaignName = string.IsNullOrWhiteSpace(request.CampaignName) ? "🔥 Mega Shopping Sale" : request.CampaignName.Trim(),
                Tagline = string.IsNullOrWhiteSpace(request.Tagline) ? "Biggest Sale of the Season" : request.Tagline.Trim(),
                BannerTheme = string.IsNullOrWhiteSpace(request.BannerTheme) ? "flame-red" : request.BannerTheme,
                StartDateTime = startDt,
                EndDateTime = endDt,
                StartDate = string.IsNullOrWhiteSpace(request.StartDate) ? startDt.ToString("dd MMM yyyy") : request.StartDate,
                StartTime = string.IsNullOrWhiteSpace(request.StartTime) ? startDt.ToString("hh:mm tt") : request.StartTime,
                EndDate = string.IsNullOrWhiteSpace(request.EndDate) ? endDt.ToString("dd MMM yyyy") : request.EndDate,
                EndTime = string.IsNullOrWhiteSpace(request.EndTime) ? endDt.ToString("hh:mm tt") : request.EndTime,
                DefaultDiscountPct = request.DefaultDiscountPct > 0 ? request.DefaultDiscountPct : 20,
                CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant(),
                MinOrderAmount = request.MinOrderAmount >= 0 ? request.MinOrderAmount : 500,
                MaxDiscountAmount = request.MaxDiscountAmount,
                ProductsScope = string.IsNullOrWhiteSpace(request.ProductsScope) ? "All Catalog Products in Categories" : request.ProductsScope,
                SellersScope = string.IsNullOrWhiteSpace(request.SellersScope) ? "All 250 Verified Sellers" : request.SellersScope,
                ParticipatingSellersCount = 250,
                TotalOrdersGenerated = 0,
                TotalGrossRevenue = 0,
                Description = request.Description ?? string.Empty,
                CreatedDate = DateTime.Now.ToString("dd MMM yyyy"),
                IsAutoScheduled = true,
                CategoryDiscounts = categoryDiscounts
            };

            lock (_lock)
            {
                newCampaign.Id = _nextId++;
                EvaluateSingleCampaignLifecycle(newCampaign, DateTime.Now);
                _inMemoryCampaigns.Insert(0, newCampaign);
            }

            await _auditService.LogAsync(
                action: "CreateSaleCampaign",
                details: $"Created scheduled sale campaign '{newCampaign.CampaignName}' with Coupon: {newCampaign.CouponCode ?? "None"}, Validity: {newCampaign.ValidityFormatted}, Category Discounts: {newCampaign.CategorySummary}",
                userId: 1,
                userRole: "Admin"
            );

            return newCampaign;
        }

        public async Task<AdminSaleCampaignDto?> UpdateCampaignAsync(CreateOrEditSaleCampaignRequest request)
        {
            if (!request.Id.HasValue) return null;

            AdminSaleCampaignDto? campaign;
            lock (_lock)
            {
                campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == request.Id.Value);
                if (campaign == null) return null;

                campaign.CampaignName = string.IsNullOrWhiteSpace(request.CampaignName) ? campaign.CampaignName : request.CampaignName.Trim();
                campaign.Tagline = string.IsNullOrWhiteSpace(request.Tagline) ? campaign.Tagline : request.Tagline.Trim();
                campaign.BannerTheme = string.IsNullOrWhiteSpace(request.BannerTheme) ? campaign.BannerTheme : request.BannerTheme;
                campaign.StartDate = string.IsNullOrWhiteSpace(request.StartDate) ? campaign.StartDate : request.StartDate;
                campaign.StartTime = string.IsNullOrWhiteSpace(request.StartTime) ? campaign.StartTime : request.StartTime;
                campaign.EndDate = string.IsNullOrWhiteSpace(request.EndDate) ? campaign.EndDate : request.EndDate;
                campaign.EndTime = string.IsNullOrWhiteSpace(request.EndTime) ? campaign.EndTime : request.EndTime;

                var startDt = ParseDateTime(campaign.StartDate, campaign.StartTime);
                var endDt = ParseDateTime(campaign.EndDate, campaign.EndTime);
                if (startDt.HasValue) campaign.StartDateTime = startDt.Value;
                if (endDt.HasValue) campaign.EndDateTime = endDt.Value;

                campaign.DefaultDiscountPct = request.DefaultDiscountPct;
                campaign.CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant();
                campaign.MinOrderAmount = request.MinOrderAmount;
                campaign.MaxDiscountAmount = request.MaxDiscountAmount;
                campaign.ProductsScope = string.IsNullOrWhiteSpace(request.ProductsScope) ? campaign.ProductsScope : request.ProductsScope;
                campaign.SellersScope = string.IsNullOrWhiteSpace(request.SellersScope) ? campaign.SellersScope : request.SellersScope;
                campaign.Description = request.Description ?? campaign.Description;

                var parsedCategories = ParseCategoryDiscounts(request.CategoryDiscountsJson);
                if (parsedCategories.Any())
                {
                    campaign.CategoryDiscounts = parsedCategories;
                }

                EvaluateSingleCampaignLifecycle(campaign, DateTime.Now);
            }

            await _auditService.LogAsync(
                action: "UpdateSaleCampaign",
                details: $"Updated scheduled sale campaign #{campaign.Id} '{campaign.CampaignName}' - Status: {campaign.Status}, Coupon: {campaign.CouponCode ?? "None"}",
                userId: 1,
                userRole: "Admin"
            );

            return campaign;
        }

        public async Task<bool> ToggleCampaignStatusAsync(int id)
        {
            AdminSaleCampaignDto? campaign;
            lock (_lock)
            {
                campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == id);
                if (campaign == null) return false;

                campaign.IsActive = !campaign.IsActive;
                if (!campaign.IsActive)
                {
                    campaign.Status = "Paused";
                    campaign.AutoLifecycleStatus = "Paused (Manual Override)";
                }
                else
                {
                    EvaluateSingleCampaignLifecycle(campaign, DateTime.Now);
                }
            }

            await _auditService.LogAsync(
                action: "ToggleSaleCampaignStatus",
                details: $"Toggled sale campaign #{campaign.Id} status to {campaign.Status} (IsActive: {campaign.IsActive})",
                userId: 1,
                userRole: "Admin"
            );

            return true;
        }

        public async Task<bool> DeleteCampaignAsync(int id)
        {
            bool removed = false;
            string name = string.Empty;
            lock (_lock)
            {
                var campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == id);
                if (campaign != null)
                {
                    name = campaign.CampaignName;
                    _inMemoryCampaigns.Remove(campaign);
                    removed = true;
                }
            }

            if (removed)
            {
                await _auditService.LogAsync(
                    action: "DeleteSaleCampaign",
                    details: $"Deleted sale campaign #{id} '{name}'",
                    userId: 1,
                    userRole: "Admin"
                );
            }

            return removed;
        }

        public async Task<int> AutoSyncCampaignLifecyclesAsync()
        {
            int transitionsCount = 0;
            var transitions = new List<(string Name, string OldStatus, string NewStatus)>();

            lock (_lock)
            {
                var now = DateTime.Now;
                foreach (var camp in _inMemoryCampaigns)
                {
                    if (!camp.IsAutoScheduled) continue;

                    string oldStatus = camp.Status;
                    EvaluateSingleCampaignLifecycle(camp, now);

                    if (oldStatus != camp.Status)
                    {
                        transitionsCount++;
                        transitions.Add((camp.CampaignName, oldStatus, camp.Status));
                    }
                }
            }

            foreach (var t in transitions)
            {
                await _auditService.LogAsync(
                    action: "AutoCampaignLifecycleTransition",
                    details: $"Point 51 Auto-Scheduler: Campaign '{t.Name}' automatically transitioned from [{t.OldStatus}] to [{t.NewStatus}] based on scheduled timestamp without manual intervention.",
                    userId: 1,
                    userRole: "SystemAutoScheduler"
                );
            }

            return transitionsCount;
        }

        private static void SyncDynamicTimersAndStatuses()
        {
            var now = DateTime.Now;
            foreach (var camp in _inMemoryCampaigns)
            {
                EvaluateSingleCampaignLifecycle(camp, now);
            }
        }

        private static void EvaluateSingleCampaignLifecycle(AdminSaleCampaignDto camp, DateTime now)
        {
            if (camp.Status == "Paused") return;

            // Ensure valid DateTime objects
            if (!camp.StartDateTime.HasValue)
            {
                camp.StartDateTime = ParseDateTime(camp.StartDate, camp.StartTime) ?? now.AddDays(2);
            }
            if (!camp.EndDateTime.HasValue)
            {
                camp.EndDateTime = ParseDateTime(camp.EndDate, camp.EndTime) ?? camp.StartDateTime.Value.AddDays(5);
            }

            var start = camp.StartDateTime.Value;
            var end = camp.EndDateTime.Value;

            if (now < start)
            {
                // Future scheduled sale -> UPCOMING
                camp.Status = "Upcoming";
                camp.IsActive = false;
                camp.CountdownLabel = "Sale Starts In:";
                camp.AutoLifecycleStatus = "Auto-Scheduled (Will auto-activate at start time)";

                var diff = start - now;
                camp.CountdownDays = Math.Max(0, diff.Days);
                camp.CountdownHours = Math.Max(0, diff.Hours);
                camp.CountdownMinutes = Math.Max(0, diff.Minutes);
                camp.CountdownSeconds = Math.Max(0, diff.Seconds);
                camp.TotalSecondsRemaining = (long)Math.Max(0, diff.TotalSeconds);
                camp.CountdownFormatted = $"{camp.CountdownDays:D2} Days {camp.CountdownHours:D2} Hours {camp.CountdownMinutes:D2} Minutes";
            }
            else if (now >= start && now <= end)
            {
                // Active running sale -> LIVE NOW
                camp.Status = "Live Now";
                camp.IsActive = true;
                camp.CountdownLabel = "Sale Ends In:";
                camp.AutoLifecycleStatus = "Auto-Active (Live Now)";

                var diff = end - now;
                camp.CountdownDays = Math.Max(0, diff.Days);
                camp.CountdownHours = Math.Max(0, diff.Hours);
                camp.CountdownMinutes = Math.Max(0, diff.Minutes);
                camp.CountdownSeconds = Math.Max(0, diff.Seconds);
                camp.TotalSecondsRemaining = (long)Math.Max(0, diff.TotalSeconds);
                camp.CountdownFormatted = $"{camp.CountdownDays:D2} Days {camp.CountdownHours:D2} Hours {camp.CountdownMinutes:D2} Minutes";
            }
            else
            {
                // Concluded sale -> EXPIRED
                camp.Status = "Expired";
                camp.IsActive = false;
                camp.CountdownLabel = "Campaign Expired";
                camp.AutoLifecycleStatus = "Auto-Expired (Completed)";
                camp.CountdownDays = 0;
                camp.CountdownHours = 0;
                camp.CountdownMinutes = 0;
                camp.CountdownSeconds = 0;
                camp.TotalSecondsRemaining = 0;
                camp.CountdownFormatted = "Campaign Expired";
            }
        }

        private static DateTime? ParseDateTime(string? dateStr, string? timeStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            string full = $"{dateStr.Trim()} {(timeStr ?? "00:00").Trim()}";
            string[] formats = {
                "dd MMM yyyy hh:mm tt", "dd MMM yyyy HH:mm", "dd MMM yyyy",
                "yyyy-MM-dd HH:mm", "yyyy-MM-dd hh:mm tt", "yyyy-MM-dd",
                "dd/MM/yyyy HH:mm", "dd/MM/yyyy"
            };

            if (DateTime.TryParseExact(full, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }
            if (DateTime.TryParse(full, out var dt2))
            {
                return dt2;
            }
            return null;
        }

        private List<CampaignCategoryDiscountDto> ParseCategoryDiscounts(string? json)
        {
            var list = new List<CampaignCategoryDiscountDto>();
            if (string.IsNullOrWhiteSpace(json)) return list;

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        string catName = elem.TryGetProperty("categoryName", out var c) ? c.GetString() ?? "" : "";
                        if (string.IsNullOrWhiteSpace(catName) && elem.TryGetProperty("CategoryName", out var c2))
                        {
                            catName = c2.GetString() ?? "";
                        }

                        decimal discount = 0;
                        if (elem.TryGetProperty("discountPercentage", out var d) && d.TryGetDecimal(out var dVal)) discount = dVal;
                        else if (elem.TryGetProperty("DiscountPercentage", out var d2) && d2.TryGetDecimal(out var d2Val)) discount = d2Val;

                        if (!string.IsNullOrWhiteSpace(catName) && discount > 0)
                        {
                            string icon = "fa-tags";
                            string color = "danger";
                            if (catName.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-mobile-screen"; color = "info"; }
                            else if (catName.IndexOf("Fashion", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-shirt"; color = "danger"; }
                            else if (catName.IndexOf("Elect", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-laptop"; color = "warning"; }
                            else if (catName.IndexOf("Home", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-house"; color = "success"; }
                            else if (catName.IndexOf("Groc", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-basket-shopping"; color = "success"; }
                            else if (catName.IndexOf("Beauty", StringComparison.OrdinalIgnoreCase) >= 0) { icon = "fa-sparkles"; color = "primary"; }

                            list.Add(new CampaignCategoryDiscountDto
                            {
                                CategoryName = catName,
                                DiscountPercentage = discount,
                                IconClass = icon,
                                BadgeColor = color
                            });
                        }
                    }
                }
            }
            catch
            {
                // Fallback on error
            }

            return list;
        }
    }
}
