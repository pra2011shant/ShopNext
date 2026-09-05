using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 1,
                    CampaignName = "🔥 Mega Shopping Sale",
                    Tagline = "Biggest Super Saver Sale of the Season!",
                    BannerTheme = "flame-red",
                    StartDate = "10 Sep 2026",
                    StartTime = "00:00 AM",
                    EndDate = "15 Sep 2026",
                    EndTime = "11:59 PM",
                    Status = "Live Now",
                    IsActive = true,
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
                    CreatedDate = "01 Sep 2026",
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 20, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 40, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 30, IconClass = "fa-laptop", BadgeColor = "warning" },
                        new CampaignCategoryDiscountDto { CategoryName = "Home & Living", DiscountPercentage = 35, IconClass = "fa-house", BadgeColor = "success" }
                    }
                });

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 2,
                    CampaignName = "⚡ Great Indian Festival Days",
                    Tagline = "India's Grand Autumn Festive Extravaganza",
                    BannerTheme = "festive-gold",
                    StartDate = "20 Sep 2026",
                    StartTime = "00:00 AM",
                    EndDate = "28 Sep 2026",
                    EndTime = "11:59 PM",
                    Status = "Upcoming",
                    IsActive = true,
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
                    CreatedDate = "04 Sep 2026",
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 25, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 50, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 45, IconClass = "fa-laptop", BadgeColor = "warning" },
                        new CampaignCategoryDiscountDto { CategoryName = "Groceries", DiscountPercentage = 20, IconClass = "fa-basket-shopping", BadgeColor = "success" }
                    }
                });

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 3,
                    CampaignName = "🌙 Midnight Super Flash Sale",
                    Tagline = "6-Hour Exclusive Night Deals",
                    BannerTheme = "midnight-violet",
                    StartDate = "09 Sep 2026",
                    StartTime = "10:00 PM",
                    EndDate = "10 Sep 2026",
                    EndTime = "04:00 AM",
                    Status = "Live Now",
                    IsActive = true,
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
                    CreatedDate = "05 Sep 2026",
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 15, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 60, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 25, IconClass = "fa-laptop", BadgeColor = "warning" }
                    }
                });

                _inMemoryCampaigns.Add(new AdminSaleCampaignDto
                {
                    Id = 4,
                    CampaignName = "🎉 Diwali Dhamaka Season Sale",
                    Tagline = "Grand Lights & Deep Festive Discounts",
                    BannerTheme = "electric-blue",
                    StartDate = "15 Oct 2026",
                    StartTime = "00:00 AM",
                    EndDate = "22 Oct 2026",
                    EndTime = "11:59 PM",
                    Status = "Upcoming",
                    IsActive = false,
                    DefaultDiscountPct = 40,
                    CouponCode = "DIWALI2026",
                    MinOrderAmount = 1999,
                    MaxDiscountAmount = 10000,
                    ProductsScope = "All Catalog Products",
                    SellersScope = "All 250 Verified Sellers",
                    ParticipatingSellersCount = 250,
                    TotalOrdersGenerated = 0,
                    TotalGrossRevenue = 0,
                    Description = "Flagship annual shopping event featuring mega cashback, instant bank discounts, and category blowout pricing.",
                    CreatedDate = "05 Sep 2026",
                    CategoryDiscounts = new List<CampaignCategoryDiscountDto>
                    {
                        new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 30, IconClass = "fa-mobile-screen", BadgeColor = "info" },
                        new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 55, IconClass = "fa-shirt", BadgeColor = "danger" },
                        new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 50, IconClass = "fa-laptop", BadgeColor = "warning" },
                        new CampaignCategoryDiscountDto { CategoryName = "Beauty & Care", DiscountPercentage = 40, IconClass = "fa-sparkles", BadgeColor = "primary" }
                    }
                });
            }
        }

        public Task<List<AdminSaleCampaignDto>> GetAllCampaignsAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_inMemoryCampaigns.OrderByDescending(c => c.IsActive).ThenBy(c => c.Id).ToList());
            }
        }

        public Task<AdminSaleCampaignDto?> GetCampaignByIdAsync(int id)
        {
            lock (_lock)
            {
                var campaign = _inMemoryCampaigns.FirstOrDefault(c => c.Id == id);
                return Task.FromResult(campaign);
            }
        }

        public async Task<AdminSaleCampaignDto> CreateCampaignAsync(CreateOrEditSaleCampaignRequest request)
        {
            var categoryDiscounts = ParseCategoryDiscounts(request.CategoryDiscountsJson);
            if (!categoryDiscounts.Any())
            {
                // Default fallback category discounts
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Mobiles", DiscountPercentage = 20, IconClass = "fa-mobile-screen", BadgeColor = "info" });
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Fashion", DiscountPercentage = 40, IconClass = "fa-shirt", BadgeColor = "danger" });
                categoryDiscounts.Add(new CampaignCategoryDiscountDto { CategoryName = "Electronics", DiscountPercentage = 30, IconClass = "fa-laptop", BadgeColor = "warning" });
            }

            var newCampaign = new AdminSaleCampaignDto
            {
                CampaignName = string.IsNullOrWhiteSpace(request.CampaignName) ? "🔥 Mega Shopping Sale" : request.CampaignName.Trim(),
                Tagline = string.IsNullOrWhiteSpace(request.Tagline) ? "Biggest Sale of the Season" : request.Tagline.Trim(),
                BannerTheme = string.IsNullOrWhiteSpace(request.BannerTheme) ? "flame-red" : request.BannerTheme,
                StartDate = string.IsNullOrWhiteSpace(request.StartDate) ? DateTime.Now.ToString("dd MMM yyyy") : request.StartDate,
                StartTime = string.IsNullOrWhiteSpace(request.StartTime) ? "00:00" : request.StartTime,
                EndDate = string.IsNullOrWhiteSpace(request.EndDate) ? DateTime.Now.AddDays(5).ToString("dd MMM yyyy") : request.EndDate,
                EndTime = string.IsNullOrWhiteSpace(request.EndTime) ? "23:59" : request.EndTime,
                Status = request.IsActive ? "Live Now" : "Draft",
                IsActive = request.IsActive,
                DefaultDiscountPct = request.DefaultDiscountPct > 0 ? request.DefaultDiscountPct : 20,
                CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant(),
                MinOrderAmount = request.MinOrderAmount >= 0 ? request.MinOrderAmount : 500,
                MaxDiscountAmount = request.MaxDiscountAmount,
                ProductsScope = string.IsNullOrWhiteSpace(request.ProductsScope) ? "All Catalog Products" : request.ProductsScope,
                SellersScope = string.IsNullOrWhiteSpace(request.SellersScope) ? "All 250 Verified Sellers" : request.SellersScope,
                ParticipatingSellersCount = 250,
                TotalOrdersGenerated = 0,
                TotalGrossRevenue = 0,
                Description = request.Description ?? string.Empty,
                CreatedDate = DateTime.Now.ToString("dd MMM yyyy"),
                CategoryDiscounts = categoryDiscounts
            };

            lock (_lock)
            {
                newCampaign.Id = _nextId++;
                _inMemoryCampaigns.Insert(0, newCampaign);
            }

            await _auditService.LogAsync(
                userId: 1,
                userRole: "Admin",
                action: "CreateSaleCampaign",
                details: $"Created sale campaign '{newCampaign.CampaignName}' with Coupon: {newCampaign.CouponCode ?? "None"}, Validity: {newCampaign.ValidityFormatted}, Category Discounts: {newCampaign.CategorySummary}"
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
                campaign.DefaultDiscountPct = request.DefaultDiscountPct;
                campaign.CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant();
                campaign.MinOrderAmount = request.MinOrderAmount;
                campaign.MaxDiscountAmount = request.MaxDiscountAmount;
                campaign.ProductsScope = string.IsNullOrWhiteSpace(request.ProductsScope) ? campaign.ProductsScope : request.ProductsScope;
                campaign.SellersScope = string.IsNullOrWhiteSpace(request.SellersScope) ? campaign.SellersScope : request.SellersScope;
                campaign.Description = request.Description ?? campaign.Description;
                campaign.IsActive = request.IsActive;
                campaign.Status = request.IsActive ? "Live Now" : "Paused";

                var parsedCategories = ParseCategoryDiscounts(request.CategoryDiscountsJson);
                if (parsedCategories.Any())
                {
                    campaign.CategoryDiscounts = parsedCategories;
                }
            }

            await _auditService.LogAsync(
                userId: 1,
                userRole: "Admin",
                action: "UpdateSaleCampaign",
                details: $"Updated sale campaign #{campaign.Id} '{campaign.CampaignName}' - Status: {campaign.Status}, Coupon: {campaign.CouponCode ?? "None"}"
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
                campaign.Status = campaign.IsActive ? "Live Now" : "Paused";
            }

            await _auditService.LogAsync(
                userId: 1,
                userRole: "Admin",
                action: "ToggleSaleCampaignStatus",
                details: $"Toggled sale campaign #{campaign.Id} status to {campaign.Status} (IsActive: {campaign.IsActive})"
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
                    userId: 1,
                    userRole: "Admin",
                    action: "DeleteSaleCampaign",
                    details: $"Deleted sale campaign #{id} '{name}'"
                );
            }

            return removed;
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
