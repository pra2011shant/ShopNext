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
        private static readonly List<AdminSellerSaleParticipationDto> _inMemoryParticipations = new List<AdminSellerSaleParticipationDto>();
        private static int _nextId = 101;
        private static int _nextParticipationId = 201;

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

                // Point 55 Seed Data (Matching Prompt: Samsung Mobile ☑, Laptop ☐, Headphone ☑)
                if (!_inMemoryParticipations.Any())
                {
                    _inMemoryParticipations.Add(new AdminSellerSaleParticipationDto
                    {
                        Id = 1,
                        ProductId = 1,
                        ProductName = "Samsung Mobile",
                        ProductImage = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=200",
                        Category = "Mobiles",
                        ShopId = 1,
                        ShopName = "Samsung Official Flagship Store",
                        CampaignId = 1,
                        CampaignName = "🔥 Mega Shopping Sale",
                        RegularPrice = 40000,
                        SalePrice = 32999,
                        AllocatedSaleStock = 50,
                        IsParticipating = true, // ☑ Participate in Mega Sale
                        ApprovalStatus = "Approved",
                        AdminRemarks = "Approved for Mega Sale flagship spotlight banner.",
                        SubmittedDate = now.AddDays(-2).ToString("dd MMM yyyy"),
                        ApprovedDate = now.AddDays(-1).ToString("dd MMM yyyy"),
                        ApprovedBy = "Super Admin"
                    });

                    _inMemoryParticipations.Add(new AdminSellerSaleParticipationDto
                    {
                        Id = 2,
                        ProductId = 2,
                        ProductName = "Laptop",
                        ProductImage = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=200",
                        Category = "Electronics",
                        ShopId = 2,
                        ShopName = "TechTrend Electronics",
                        CampaignId = 1,
                        CampaignName = "🔥 Mega Shopping Sale",
                        RegularPrice = 85000,
                        SalePrice = 85000,
                        AllocatedSaleStock = 0,
                        IsParticipating = false, // ☐ Participate (Opted out)
                        ApprovalStatus = "OptedOut",
                        AdminRemarks = "Seller opted out from sale pricing.",
                        SubmittedDate = now.AddDays(-2).ToString("dd MMM yyyy"),
                        ApprovedDate = null,
                        ApprovedBy = null
                    });

                    _inMemoryParticipations.Add(new AdminSellerSaleParticipationDto
                    {
                        Id = 3,
                        ProductId = 3,
                        ProductName = "Headphone",
                        ProductImage = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=200",
                        Category = "Electronics",
                        ShopId = 3,
                        ShopName = "AudioPhile India",
                        CampaignId = 1,
                        CampaignName = "🔥 Mega Shopping Sale",
                        RegularPrice = 29990,
                        SalePrice = 22999,
                        AllocatedSaleStock = 40,
                        IsParticipating = true, // ☑ Participate in Mega Sale
                        ApprovalStatus = "Pending",
                        AdminRemarks = "Awaiting Admin final review and discount verification.",
                        SubmittedDate = now.ToString("dd MMM yyyy"),
                        ApprovedDate = null,
                        ApprovedBy = null
                    });

                    _inMemoryParticipations.Add(new AdminSellerSaleParticipationDto
                    {
                        Id = 4,
                        ProductId = 4,
                        ProductName = "Apple AirPods Pro",
                        ProductImage = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=200",
                        Category = "Electronics",
                        ShopId = 4,
                        ShopName = "Apple Authorized Reseller",
                        CampaignId = 1,
                        CampaignName = "🔥 Mega Shopping Sale",
                        RegularPrice = 24900,
                        SalePrice = 18499,
                        AllocatedSaleStock = 30,
                        IsParticipating = true, // ☑ Participate in Mega Sale
                        ApprovalStatus = "Approved",
                        AdminRemarks = "Approved with 26% festival discount.",
                        SubmittedDate = now.AddDays(-3).ToString("dd MMM yyyy"),
                        ApprovedDate = now.AddDays(-2).ToString("dd MMM yyyy"),
                        ApprovedBy = "Super Admin"
                    });

                    _inMemoryParticipations.Add(new AdminSellerSaleParticipationDto
                    {
                        Id = 5,
                        ProductId = 5,
                        ProductName = "Smart 4K QLED TV (55-Inch)",
                        ProductImage = "https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=200",
                        Category = "Electronics",
                        ShopId = 5,
                        ShopName = "VisionWorld Appliances",
                        CampaignId = 1,
                        CampaignName = "🔥 Mega Shopping Sale",
                        RegularPrice = 55000,
                        SalePrice = 44999,
                        AllocatedSaleStock = 15,
                        IsParticipating = true, // ☑ Participate in Mega Sale
                        ApprovalStatus = "Pending",
                        AdminRemarks = "Seller requested 18% special festival discount.",
                        SubmittedDate = now.ToString("dd MMM yyyy"),
                        ApprovedDate = null,
                        ApprovedBy = null
                    });
                }
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

        #region Point 55: Seller Sale Participation Methods

        public Task<List<AdminSellerSaleParticipationDto>> GetAllSellerSaleParticipationsAsync(int? campaignId = null, int? shopId = null)
        {
            lock (_lock)
            {
                var query = _inMemoryParticipations.AsEnumerable();
                if (campaignId.HasValue && campaignId.Value > 0)
                {
                    query = query.Where(p => p.CampaignId == campaignId.Value);
                }
                if (shopId.HasValue && shopId.Value > 0)
                {
                    query = query.Where(p => p.ShopId == shopId.Value);
                }

                return Task.FromResult(query.OrderBy(p => p.ApprovalStatus == "Pending" ? 0 : 1).ThenBy(p => p.Id).ToList());
            }
        }

        public async Task<AdminSellerSaleParticipationDto?> UpdateSellerProductParticipationAsync(int participationId, bool isParticipating, decimal? salePrice, int? allocatedStock, string? notes)
        {
            AdminSellerSaleParticipationDto? item;
            lock (_lock)
            {
                item = _inMemoryParticipations.FirstOrDefault(p => p.Id == participationId);
                if (item != null)
                {
                    item.IsParticipating = isParticipating;
                    if (salePrice.HasValue && salePrice.Value > 0)
                    {
                        item.SalePrice = salePrice.Value;
                    }
                    if (allocatedStock.HasValue && allocatedStock.Value >= 0)
                    {
                        item.AllocatedSaleStock = allocatedStock.Value;
                    }

                    if (!isParticipating)
                    {
                        item.ApprovalStatus = "OptedOut";
                        item.AdminRemarks = "Seller opted out from sale participation.";
                    }
                    else if (item.ApprovalStatus == "OptedOut")
                    {
                        item.ApprovalStatus = "Pending";
                        item.AdminRemarks = "Seller re-opted into sale; pending Admin review.";
                    }

                    if (!string.IsNullOrWhiteSpace(notes))
                    {
                        item.AdminRemarks = notes.Trim();
                    }
                }
            }

            if (item != null)
            {
                await _auditService.LogAsync(
                    action: "UpdateSellerSaleParticipation",
                    details: $"Seller updated participation for '{item.ProductName}'. Participating: {isParticipating}, Price: ₹{item.SalePrice:N0}, Status: {item.ApprovalStatus}",
                    userId: 1,
                    userRole: "Seller"
                );
            }

            return item;
        }

        public async Task<AdminSellerSaleParticipationDto?> ApproveSellerParticipationAsync(int participationId, string? remarks, string adminName)
        {
            AdminSellerSaleParticipationDto? item;
            lock (_lock)
            {
                item = _inMemoryParticipations.FirstOrDefault(p => p.Id == participationId);
                if (item != null)
                {
                    item.IsParticipating = true;
                    item.ApprovalStatus = "Approved";
                    item.ApprovedDate = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
                    item.ApprovedBy = string.IsNullOrWhiteSpace(adminName) ? "Admin" : adminName;
                    item.AdminRemarks = string.IsNullOrWhiteSpace(remarks) ? "Approved for Campaign Sale spotlight." : remarks.Trim();
                }
            }

            if (item != null)
            {
                await _auditService.LogAsync(
                    action: "ApproveSellerSaleParticipation",
                    details: $"Admin '{adminName}' approved '{item.ProductName}' ({item.ShopName}) for campaign '{item.CampaignName}'. Sale Price: ₹{item.SalePrice:N0}",
                    userId: 1,
                    userRole: "Admin"
                );
            }

            return item;
        }

        public async Task<AdminSellerSaleParticipationDto?> RejectSellerParticipationAsync(int participationId, string? reason, string adminName)
        {
            AdminSellerSaleParticipationDto? item;
            lock (_lock)
            {
                item = _inMemoryParticipations.FirstOrDefault(p => p.Id == participationId);
                if (item != null)
                {
                    item.ApprovalStatus = "Rejected";
                    item.AdminRemarks = string.IsNullOrWhiteSpace(reason) ? "Participation request rejected by Admin." : reason.Trim();
                    item.ApprovedBy = adminName;
                }
            }

            if (item != null)
            {
                await _auditService.LogAsync(
                    action: "RejectSellerSaleParticipation",
                    details: $"Admin rejected '{item.ProductName}' ({item.ShopName}) from campaign '{item.CampaignName}'. Reason: {item.AdminRemarks}",
                    userId: 1,
                    userRole: "Admin"
                );
            }

            return item;
        }

        public async Task<AdminSellerSaleParticipationDto> OptInProductForSaleAsync(int productId, int shopId, int campaignId, decimal salePrice, int allocatedStock)
        {
            var product = await _context.Products.FindAsync(productId);
            var shop = await _context.Shops.FindAsync(shopId);
            var campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == campaignId);

            string prodName = product?.ProductName ?? $"Product #{productId}";
            string shopName = shop?.ShopName ?? $"Shop #{shopId}";
            string campName = campaign?.CampaignName ?? "🔥 Mega Shopping Sale";
            decimal regPrice = product?.Price ?? (salePrice > 0 ? salePrice * 1.25m : 1000);

            var newEntry = new AdminSellerSaleParticipationDto
            {
                Id = _nextParticipationId++,
                ProductId = productId,
                ProductName = prodName,
                ProductImage = product?.ImageUrl ?? "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=200",
                Category = product?.Category ?? "General",
                ShopId = shopId,
                ShopName = shopName,
                CampaignId = campaignId,
                CampaignName = campName,
                RegularPrice = regPrice,
                SalePrice = salePrice > 0 ? salePrice : regPrice * 0.8m,
                AllocatedSaleStock = allocatedStock > 0 ? allocatedStock : 20,
                IsParticipating = true, // ☑ Participate
                ApprovalStatus = "Pending", // Seller submitted -> Pending Admin Approval
                AdminRemarks = "Newly submitted by seller for Admin approval.",
                SubmittedDate = DateTime.Now.ToString("dd MMM yyyy")
            };

            lock (_lock)
            {
                _inMemoryParticipations.Add(newEntry);
            }

            await _auditService.LogAsync(
                action: "SellerOptInProductForSale",
                details: $"Seller '{shopName}' opted in product '{prodName}' for campaign '{campName}'. Proposed Sale Price: ₹{newEntry.SalePrice:N0}",
                userId: shopId,
                userRole: "Seller"
            );

            return newEntry;
        }

        #endregion

        #region Point 56: Sale Analytics & Post-Campaign Intelligence Report

        public Task<SaleAnalyticsReportDto> GetSaleAnalyticsReportAsync(int campaignId)
        {
            var camp = _inMemoryCampaigns.FirstOrDefault(c => c.Id == campaignId) ?? _inMemoryCampaigns.FirstOrDefault();
            string campName = camp?.CampaignName ?? "🔥 Mega Shopping Sale";
            string dateRange = camp != null ? $"{camp.StartDate} - {camp.EndDate}" : "10 Sep 2026 - 15 Sep 2026";

            if (campaignId == 2)
            {
                // Flash Sale Report
                return Task.FromResult(new SaleAnalyticsReportDto
                {
                    CampaignId = 2,
                    CampaignName = "🌙 Midnight Super Flash Sale",
                    DateRange = dateRange,
                    CampaignStatus = "Live Flash Analytics",
                    TotalOrders = 4850,
                    ProductsSold = 6120,
                    Revenue = 4250000,
                    RevenueFormatted = "₹42.5 Lakhs",
                    NetRevenueFormatted = "₹40.8 Lakhs",
                    TopCategory = "Mobiles",
                    TopCategoryRevenue = 2800000,
                    TopCategoryShareFormatted = "65.9% of Flash GMV",
                    TopProduct = "Samsung Mobile (Galaxy S24 5G)",
                    TopProductImage = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=200",
                    TopProductUnitsSold = 2100,
                    TopProductRevenue = 6929790,
                    TopSeller = "Samsung Official Flagship Store",
                    TopSellerOrdersFulfilled = 2200,
                    TopSellerRevenue = 2800000,
                    TopSellerRating = 4.9,
                    Returns = 145,
                    Cancellations = 95,
                    CategoryBreakdown = new List<CampaignCategoryShareDto>
                    {
                        new CampaignCategoryShareDto { CategoryName = "Mobiles", Revenue = 2800000, RevenueFormatted = "₹28.0 L", UnitsSold = 3400, PercentageShare = 65.9, ColorClass = "info", IconClass = "fa-mobile-screen" },
                        new CampaignCategoryShareDto { CategoryName = "Electronics", Revenue = 1150000, RevenueFormatted = "₹11.5 L", UnitsSold = 1920, PercentageShare = 27.1, ColorClass = "warning", IconClass = "fa-laptop" },
                        new CampaignCategoryShareDto { CategoryName = "Accessories", Revenue = 300000, RevenueFormatted = "₹3.0 L", UnitsSold = 800, PercentageShare = 7.0, ColorClass = "success", IconClass = "fa-headphones" }
                    },
                    TopProductsList = new List<CampaignProductPerformanceDto>
                    {
                        new CampaignProductPerformanceDto { Rank = 1, ProductName = "Samsung Mobile Galaxy S24", Category = "Mobiles", SellerName = "Samsung Official Flagship Store", UnitsSold = 2100, SalePrice = 32999, GrossRevenue = 6929790, RevenueFormatted = "₹69.3 L", ReturnsCount = 45 },
                        new CampaignProductPerformanceDto { Rank = 2, ProductName = "Apple AirPods Pro (2nd Gen)", Category = "Electronics", SellerName = "Apple Authorized Reseller", UnitsSold = 1400, SalePrice = 18499, GrossRevenue = 2589860, RevenueFormatted = "₹25.9 L", ReturnsCount = 28 },
                        new CampaignProductPerformanceDto { Rank = 3, ProductName = "Sony WH-1000XM5 Headphones", Category = "Electronics", SellerName = "AudioPhile India", UnitsSold = 950, SalePrice = 22999, GrossRevenue = 2184905, RevenueFormatted = "₹21.8 L", ReturnsCount = 18 }
                    },
                    TopSellersList = new List<CampaignSellerPerformanceDto>
                    {
                        new CampaignSellerPerformanceDto { Rank = 1, SellerName = "Samsung Official Flagship Store", ShopCity = "Bangalore, Karnataka", OrdersCount = 2200, UnitsSold = 2400, Revenue = 2800000, RevenueFormatted = "₹28.0 L", Rating = 4.9, FulfillmentRate = 98.4 },
                        new CampaignSellerPerformanceDto { Rank = 2, SellerName = "ABC Electronics", ShopCity = "Mumbai, Maharashtra", OrdersCount = 1450, UnitsSold = 1900, Revenue = 950000, RevenueFormatted = "₹9.5 L", Rating = 4.8, FulfillmentRate = 97.2 },
                        new CampaignSellerPerformanceDto { Rank = 3, SellerName = "Apple Authorized Reseller", ShopCity = "Delhi NCR", OrdersCount = 1200, UnitsSold = 1400, Revenue = 500000, RevenueFormatted = "₹5.0 L", Rating = 4.8, FulfillmentRate = 96.5 }
                    }
                });
            }

            // Default: Prompt Exact Example for Mega Sale
            return Task.FromResult(new SaleAnalyticsReportDto
            {
                CampaignId = 1,
                CampaignName = "🔥 Mega Shopping Sale",
                DateRange = dateRange,
                CampaignStatus = "Concluded (Post-Sale Analysis)",
                TotalOrders = 25500,
                ProductsSold = 38200,
                Revenue = 25000000, // ₹2.5 Crore
                RevenueFormatted = "₹2.50 Crore",
                NetRevenueFormatted = "₹2.38 Crore",
                TopCategory = "Electronics",
                TopCategoryRevenue = 12000000, // ₹1.2 Crore
                TopCategoryShareFormatted = "48.0% of Total GMV",
                TopProduct = "Samsung Mobile",
                TopProductImage = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=200",
                TopProductUnitsSold = 12400,
                TopProductRevenue = 40918760,
                TopSeller = "ABC Electronics",
                TopSellerOrdersFulfilled = 8200,
                TopSellerRevenue = 8200000,
                TopSellerRating = 4.9,
                Returns = 1250,
                Cancellations = 850,
                CategoryBreakdown = new List<CampaignCategoryShareDto>
                {
                    new CampaignCategoryShareDto { CategoryName = "Electronics", Revenue = 12000000, RevenueFormatted = "₹1.20 Cr", UnitsSold = 18400, PercentageShare = 48.0, ColorClass = "warning", IconClass = "fa-laptop" },
                    new CampaignCategoryShareDto { CategoryName = "Mobiles & Gadgets", Revenue = 8500000, RevenueFormatted = "₹85.0 L", UnitsSold = 12800, PercentageShare = 34.0, ColorClass = "info", IconClass = "fa-mobile-screen" },
                    new CampaignCategoryShareDto { CategoryName = "Fashion & Apparel", Revenue = 3500000, RevenueFormatted = "₹35.0 L", UnitsSold = 5500, PercentageShare = 14.0, ColorClass = "danger", IconClass = "fa-shirt" },
                    new CampaignCategoryShareDto { CategoryName = "Home & Living", Revenue = 1000000, RevenueFormatted = "₹10.0 L", UnitsSold = 1500, PercentageShare = 4.0, ColorClass = "success", IconClass = "fa-house" }
                },
                TopProductsList = new List<CampaignProductPerformanceDto>
                {
                    new CampaignProductPerformanceDto { Rank = 1, ProductName = "Samsung Mobile (Galaxy S24 Flagship)", Category = "Mobiles", SellerName = "ABC Electronics", UnitsSold = 12400, SalePrice = 32999, GrossRevenue = 40918760, RevenueFormatted = "₹4.09 Cr", ReturnsCount = 280 },
                    new CampaignProductPerformanceDto { Rank = 2, ProductName = "Apple AirPods Pro (2nd Gen)", Category = "Electronics", SellerName = "Apple Authorized Reseller", UnitsSold = 7600, SalePrice = 18499, GrossRevenue = 14059240, RevenueFormatted = "₹1.41 Cr", ReturnsCount = 180 },
                    new CampaignProductPerformanceDto { Rank = 3, ProductName = "Sony WH-1000XM5 Wireless Headphones", Category = "Electronics", SellerName = "AudioPhile India", UnitsSold = 4800, SalePrice = 22999, GrossRevenue = 11039520, RevenueFormatted = "₹1.10 Cr", ReturnsCount = 120 },
                    new CampaignProductPerformanceDto { Rank = 4, ProductName = "Smart 4K QLED TV (55-Inch)", Category = "Electronics", SellerName = "VisionWorld Appliances", UnitsSold = 3200, SalePrice = 44999, GrossRevenue = 14399680, RevenueFormatted = "₹1.44 Cr", ReturnsCount = 95 },
                    new CampaignProductPerformanceDto { Rank = 5, ProductName = "Dell XPS 15 Flagship Edition Laptop", Category = "Electronics", SellerName = "TechTrend Electronics", UnitsSold = 1800, SalePrice = 85000, GrossRevenue = 15300000, RevenueFormatted = "₹1.53 Cr", ReturnsCount = 65 }
                },
                TopSellersList = new List<CampaignSellerPerformanceDto>
                {
                    new CampaignSellerPerformanceDto { Rank = 1, SellerName = "ABC Electronics", ShopCity = "Mumbai, Maharashtra", OrdersCount = 8200, UnitsSold = 14500, Revenue = 8200000, RevenueFormatted = "₹82.0 Lakhs", Rating = 4.9, FulfillmentRate = 97.8 },
                    new CampaignSellerPerformanceDto { Rank = 2, SellerName = "Samsung Official Flagship Store", ShopCity = "Bangalore, Karnataka", OrdersCount = 7400, UnitsSold = 12400, Revenue = 24400000, RevenueFormatted = "₹2.44 Crore", Rating = 4.9, FulfillmentRate = 98.6 },
                    new CampaignSellerPerformanceDto { Rank = 3, SellerName = "Apple Authorized Reseller", ShopCity = "Delhi NCR", OrdersCount = 5100, UnitsSold = 7600, Revenue = 9430000, RevenueFormatted = "₹94.3 Lakhs", Rating = 4.8, FulfillmentRate = 96.9 },
                    new CampaignSellerPerformanceDto { Rank = 4, SellerName = "AudioPhile India", ShopCity = "Hyderabad, Telangana", OrdersCount = 3200, UnitsSold = 4800, Revenue = 7360000, RevenueFormatted = "₹73.6 Lakhs", Rating = 4.8, FulfillmentRate = 97.1 },
                    new CampaignSellerPerformanceDto { Rank = 5, SellerName = "VisionWorld Appliances", ShopCity = "Ahmedabad, Gujarat", OrdersCount = 1600, UnitsSold = 3200, Revenue = 7200000, RevenueFormatted = "₹72.0 Lakhs", Rating = 4.7, FulfillmentRate = 95.4 }
                }
            });
        }

        public async Task<List<SaleAnalyticsReportDto>> GetAllCampaignReportsSummaryAsync()
        {
            var list = new List<SaleAnalyticsReportDto>();
            list.Add(await GetSaleAnalyticsReportAsync(1));
            list.Add(await GetSaleAnalyticsReportAsync(2));
            return list;
        }

        #endregion
    }
}
