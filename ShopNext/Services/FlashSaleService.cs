using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class FlashSaleService : IFlashSaleService
    {
        private readonly ShopNextDbContext _context;
        private readonly IAuditService _auditService;

        private static readonly object _lock = new object();
        private static readonly List<AdminFlashSaleDto> _inMemoryFlashSales = new List<AdminFlashSaleDto>();
        private static int _nextId = 201;

        static FlashSaleService()
        {
            InitializeSeedFlashDeals();
        }

        public FlashSaleService(ShopNextDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private static void InitializeSeedFlashDeals()
        {
            lock (_lock)
            {
                if (_inMemoryFlashSales.Any()) return;

                var now = DateTime.Now;

                // 1. Prompt Exact Example: Samsung Mobile ₹40,000 -> ₹32,999 | Only 50 Units | Ends in 00:25:10
                var deal1End = now.AddMinutes(25).AddSeconds(10);
                _inMemoryFlashSales.Add(new AdminFlashSaleDto
                {
                    Id = 1,
                    Title = "Samsung Mobile",
                    Subtitle = "Galaxy S24 5G Flagship Edition (128GB)",
                    ProductImage = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400",
                    Category = "Mobiles",
                    Brand = "Samsung",
                    OriginalPrice = 40000,
                    FlashPrice = 32999,
                    TotalUnits = 50,
                    ClaimedUnits = 48, // 2 units remaining (96% claimed)
                    StartDateTime = now.AddMinutes(-35),
                    EndDateTime = deal1End,
                    RemainingSeconds = 1510, // 00:25:10
                    TimerFormatted = "00:25:10",
                    Status = "Live Flash Deal",
                    MaxPerUser = 1,
                    ShopName = "Samsung Official Flagship Store",
                    CreatedDate = now.ToString("dd MMM yyyy"),
                    IsActive = true
                });

                // 2. SOLD OUT Example: Apple AirPods Pro
                var deal2End = now.AddMinutes(12).AddSeconds(45);
                _inMemoryFlashSales.Add(new AdminFlashSaleDto
                {
                    Id = 2,
                    Title = "Apple AirPods Pro (2nd Gen)",
                    Subtitle = "Active Noise Cancellation with USB-C MagSafe Case",
                    ProductImage = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=400",
                    Category = "Electronics",
                    Brand = "Apple",
                    OriginalPrice = 24900,
                    FlashPrice = 18499,
                    TotalUnits = 30,
                    ClaimedUnits = 30, // 100% claimed -> SOLD OUT
                    StartDateTime = now.AddMinutes(-45),
                    EndDateTime = deal2End,
                    RemainingSeconds = 765,
                    TimerFormatted = "00:12:45",
                    Status = "Sold Out",
                    MaxPerUser = 1,
                    ShopName = "Apple Authorized Reseller",
                    CreatedDate = now.ToString("dd MMM yyyy"),
                    IsActive = true
                });

                // 3. Sony WH-1000XM5 Wireless Headphones
                var deal3End = now.AddHours(1).AddMinutes(15).AddSeconds(30);
                _inMemoryFlashSales.Add(new AdminFlashSaleDto
                {
                    Id = 3,
                    Title = "Sony WH-1000XM5",
                    Subtitle = "Premium Wireless Noise Canceling Headphones",
                    ProductImage = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=400",
                    Category = "Electronics",
                    Brand = "Sony",
                    OriginalPrice = 29990,
                    FlashPrice = 22999,
                    TotalUnits = 40,
                    ClaimedUnits = 24, // 16 remaining (60% claimed)
                    StartDateTime = now.AddMinutes(-15),
                    EndDateTime = deal3End,
                    RemainingSeconds = 4530,
                    TimerFormatted = "01:15:30",
                    Status = "Live Flash Deal",
                    MaxPerUser = 1,
                    ShopName = "Sony Centre India",
                    CreatedDate = now.ToString("dd MMM yyyy"),
                    IsActive = true
                });

                // 4. OnePlus Watch 2 Smartwatch
                var deal4End = now.AddMinutes(45).AddSeconds(20);
                _inMemoryFlashSales.Add(new AdminFlashSaleDto
                {
                    Id = 4,
                    Title = "OnePlus Watch 2",
                    Subtitle = "Dual-Engine Architecture with Wear OS",
                    ProductImage = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400",
                    Category = "Electronics",
                    Brand = "OnePlus",
                    OriginalPrice = 24999,
                    FlashPrice = 17999,
                    TotalUnits = 25,
                    ClaimedUnits = 21, // 4 remaining (84% claimed)
                    StartDateTime = now.AddMinutes(-15),
                    EndDateTime = deal4End,
                    RemainingSeconds = 2720,
                    TimerFormatted = "00:45:20",
                    Status = "Live Flash Deal",
                    MaxPerUser = 1,
                    ShopName = "OnePlus Exclusive Store",
                    CreatedDate = now.ToString("dd MMM yyyy"),
                    IsActive = true
                });
            }
        }

        public Task<List<AdminFlashSaleDto>> GetAllFlashSalesAsync()
        {
            lock (_lock)
            {
                SyncTimersAndStatuses();
                return Task.FromResult(_inMemoryFlashSales.OrderBy(f => f.IsSoldOut).ThenByDescending(f => f.IsActive).ThenBy(f => f.Id).ToList());
            }
        }

        public Task<AdminFlashSaleDto?> GetFlashSaleByIdAsync(int id)
        {
            lock (_lock)
            {
                SyncTimersAndStatuses();
                var deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == id);
                return Task.FromResult(deal);
            }
        }

        public async Task<AdminFlashSaleDto> CreateFlashSaleAsync(CreateOrEditFlashSaleRequest request)
        {
            var now = DateTime.Now;
            var durationMins = request.DurationMinutes > 0 ? request.DurationMinutes : 30;
            var endDt = now.AddMinutes(durationMins);

            var newDeal = new AdminFlashSaleDto
            {
                Title = string.IsNullOrWhiteSpace(request.Title) ? "Flash Deal Item" : request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim() ?? string.Empty,
                ProductImage = string.IsNullOrWhiteSpace(request.ProductImage)
                    ? "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=400"
                    : request.ProductImage.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "Mobiles" : request.Category,
                Brand = string.IsNullOrWhiteSpace(request.Brand) ? "Generic" : request.Brand,
                OriginalPrice = request.OriginalPrice > 0 ? request.OriginalPrice : 40000,
                FlashPrice = request.FlashPrice > 0 ? request.FlashPrice : 32999,
                TotalUnits = request.TotalUnits > 0 ? request.TotalUnits : 50,
                ClaimedUnits = Math.Max(0, request.ClaimedUnits),
                StartDateTime = now,
                EndDateTime = endDt,
                RemainingSeconds = (long)(endDt - now).TotalSeconds,
                TimerFormatted = FormatSeconds((long)(endDt - now).TotalSeconds),
                Status = request.ClaimedUnits >= request.TotalUnits ? "Sold Out" : (request.IsActive ? "Live Flash Deal" : "Paused"),
                MaxPerUser = request.MaxPerUser > 0 ? request.MaxPerUser : 1,
                ShopName = string.IsNullOrWhiteSpace(request.ShopName) ? "Official Store" : request.ShopName,
                CreatedDate = now.ToString("dd MMM yyyy"),
                IsActive = request.IsActive
            };

            lock (_lock)
            {
                newDeal.Id = _nextId++;
                _inMemoryFlashSales.Insert(0, newDeal);
            }

            await _auditService.LogAsync(
                action: "CreateFlashSale",
                details: $"Created Flash Sale deal '{newDeal.Title}' - ₹{newDeal.FlashPrice:N0} (MRP: ₹{newDeal.OriginalPrice:N0}), Allocated Units: {newDeal.TotalUnits}, Duration: {durationMins} mins",
                userId: 1,
                userRole: "Admin"
            );

            return newDeal;
        }

        public async Task<AdminFlashSaleDto?> UpdateFlashSaleAsync(CreateOrEditFlashSaleRequest request)
        {
            if (!request.Id.HasValue) return null;

            AdminFlashSaleDto? deal;
            lock (_lock)
            {
                deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == request.Id.Value);
                if (deal == null) return null;

                deal.Title = string.IsNullOrWhiteSpace(request.Title) ? deal.Title : request.Title.Trim();
                deal.Subtitle = request.Subtitle?.Trim() ?? deal.Subtitle;
                if (!string.IsNullOrWhiteSpace(request.ProductImage)) deal.ProductImage = request.ProductImage.Trim();
                deal.Category = string.IsNullOrWhiteSpace(request.Category) ? deal.Category : request.Category;
                deal.Brand = string.IsNullOrWhiteSpace(request.Brand) ? deal.Brand : request.Brand;
                deal.OriginalPrice = request.OriginalPrice > 0 ? request.OriginalPrice : deal.OriginalPrice;
                deal.FlashPrice = request.FlashPrice > 0 ? request.FlashPrice : deal.FlashPrice;
                deal.TotalUnits = request.TotalUnits > 0 ? request.TotalUnits : deal.TotalUnits;
                deal.ClaimedUnits = Math.Min(deal.TotalUnits, Math.Max(0, request.ClaimedUnits));
                deal.ShopName = string.IsNullOrWhiteSpace(request.ShopName) ? deal.ShopName : request.ShopName;
                deal.IsActive = request.IsActive;

                if (request.DurationMinutes > 0)
                {
                    deal.EndDateTime = DateTime.Now.AddMinutes(request.DurationMinutes);
                    deal.RemainingSeconds = (long)(deal.EndDateTime - DateTime.Now).TotalSeconds;
                    deal.TimerFormatted = FormatSeconds(deal.RemainingSeconds);
                }

                deal.Status = deal.IsSoldOut ? "Sold Out" : (deal.IsActive ? "Live Flash Deal" : "Paused");
            }

            await _auditService.LogAsync(
                action: "UpdateFlashSale",
                details: $"Updated Flash Sale deal #{deal.Id} '{deal.Title}' - Stock: {deal.RemainingUnits}/{deal.TotalUnits} Units, Flash Price: ₹{deal.FlashPrice:N0}",
                userId: 1,
                userRole: "Admin"
            );

            return deal;
        }

        public async Task<AdminFlashSaleDto?> ToggleFlashSaleStatusAsync(int id)
        {
            AdminFlashSaleDto? deal;
            lock (_lock)
            {
                deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == id);
                if (deal == null) return null;

                deal.IsActive = !deal.IsActive;
                deal.Status = deal.IsSoldOut ? "Sold Out" : (deal.IsActive ? "Live Flash Deal" : "Paused");
            }

            await _auditService.LogAsync(
                action: "ToggleFlashSaleStatus",
                details: $"Toggled Flash Sale deal #{deal.Id} status to {deal.Status} (IsActive: {deal.IsActive})",
                userId: 1,
                userRole: "Admin"
            );

            return deal;
        }

        public async Task<bool> DeleteFlashSaleAsync(int id)
        {
            bool removed = false;
            string title = string.Empty;

            lock (_lock)
            {
                var deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == id);
                if (deal != null)
                {
                    title = deal.Title;
                    _inMemoryFlashSales.Remove(deal);
                    removed = true;
                }
            }

            if (removed)
            {
                await _auditService.LogAsync(
                    action: "DeleteFlashSale",
                    details: $"Deleted Flash Sale deal #{id} '{title}'",
                    userId: 1,
                    userRole: "Admin"
                );
            }

            return removed;
        }

        public Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> ClaimFlashSaleUnitAsync(ClaimFlashSaleRequest request)
        {
            if (request == null) return Task.FromResult<(bool, string, AdminFlashSaleDto?)>((false, "Invalid request.", null));
            return ClaimFlashSaleUnitAsync(request.FlashSaleId, request.Quantity, request.CustomerId);
        }

        public async Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> ClaimFlashSaleUnitAsync(int flashSaleId, int quantity = 1, int customerId = 1)
        {
            AdminFlashSaleDto? deal;
            bool wasSoldOut = false;

            lock (_lock)
            {
                deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == flashSaleId);
                if (deal == null)
                {
                    return (false, "Flash deal not found.", null);
                }

                if (deal.IsSoldOut || deal.RemainingUnits <= 0)
                {
                    deal.Status = "Sold Out";
                    return (false, "SOLD OUT! All allocated flash units have been claimed.", deal);
                }

                if (!deal.IsActive)
                {
                    return (false, "Flash deal is currently inactive.", deal);
                }

                if (deal.RemainingSeconds <= 0)
                {
                    deal.Status = "Expired";
                    return (false, "Flash deal time has expired!", deal);
                }

                int toClaim = Math.Min(quantity, deal.RemainingUnits);
                deal.ClaimedUnits += toClaim;

                if (deal.RemainingUnits <= 0)
                {
                    deal.Status = "Sold Out";
                    wasSoldOut = true;
                }
            }

            await _auditService.LogAsync(
                action: "ClaimFlashSaleUnit",
                details: wasSoldOut
                    ? $"Flash deal #{deal.Id} '{deal.Title}' is now 100% SOLD OUT! All {deal.TotalUnits} units claimed."
                    : $"Customer #{customerId} claimed {quantity} unit(s) of Flash Deal #{deal.Id} '{deal.Title}'. Remaining: {deal.RemainingUnits}/{deal.TotalUnits} units.",
                userId: customerId,
                userRole: "Customer"
            );

            string msg = wasSoldOut
                ? $"Claimed successfully! Deal #{deal.Id} is now 100% SOLD OUT."
                : $"Claimed successfully! Only {deal.RemainingUnits} unit(s) remaining.";

            return (true, msg, deal);
        }

        public async Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> RestockFlashSaleAsync(int id, int addedUnits = 20)
        {
            AdminFlashSaleDto? deal;
            lock (_lock)
            {
                deal = _inMemoryFlashSales.FirstOrDefault(f => f.Id == id);
                if (deal == null) return (false, "Flash sale deal not found.", null);

                deal.TotalUnits += addedUnits;
                deal.Status = "Live Flash Deal";
                deal.IsActive = true;
            }

            await _auditService.LogAsync(
                action: "RestockFlashSale",
                details: $"Restocked {addedUnits} flash units for deal #{deal.Id} '{deal.Title}'. New Total Units: {deal.TotalUnits}, Available: {deal.RemainingUnits}",
                userId: 1,
                userRole: "Admin"
            );

            return (true, $"Restocked {addedUnits} units successfully! Total: {deal.TotalUnits} units, Remaining: {deal.RemainingUnits} units.", deal);
        }

        public async Task<int> SyncFlashSaleLifecyclesAsync()
        {
            int updated = 0;
            var expiredDeals = new List<string>();

            lock (_lock)
            {
                var now = DateTime.Now;
                foreach (var deal in _inMemoryFlashSales)
                {
                    if (deal.Status == "Paused") continue;

                    if (deal.RemainingUnits <= 0)
                    {
                        if (deal.Status != "Sold Out")
                        {
                            deal.Status = "Sold Out";
                            updated++;
                        }
                    }
                    else if (now > deal.EndDateTime)
                    {
                        if (deal.Status != "Expired")
                        {
                            deal.Status = "Expired";
                            deal.IsActive = false;
                            deal.RemainingSeconds = 0;
                            deal.TimerFormatted = "00:00:00";
                            updated++;
                            expiredDeals.Add(deal.Title);
                        }
                    }
                    else
                    {
                        deal.RemainingSeconds = (long)(deal.EndDateTime - now).TotalSeconds;
                        deal.TimerFormatted = FormatSeconds(deal.RemainingSeconds);
                        if (deal.Status != "Live Flash Deal" && deal.IsActive)
                        {
                            deal.Status = "Live Flash Deal";
                            updated++;
                        }
                    }
                }
            }

            foreach (var name in expiredDeals)
            {
                await _auditService.LogAsync(
                    action: "FlashDealExpired",
                    details: $"Flash deal '{name}' countdown timer reached 00:00:00 and automatically expired.",
                    userId: 1,
                    userRole: "SystemAutoScheduler"
                );
            }

            return updated;
        }

        private static void SyncTimersAndStatuses()
        {
            var now = DateTime.Now;
            foreach (var deal in _inMemoryFlashSales)
            {
                if (deal.Status == "Paused") continue;

                if (deal.RemainingUnits <= 0)
                {
                    deal.Status = "Sold Out";
                }
                else if (now > deal.EndDateTime)
                {
                    deal.Status = "Expired";
                    deal.IsActive = false;
                    deal.RemainingSeconds = 0;
                    deal.TimerFormatted = "00:00:00";
                }
                else
                {
                    deal.RemainingSeconds = (long)(deal.EndDateTime - now).TotalSeconds;
                    deal.TimerFormatted = FormatSeconds(deal.RemainingSeconds);
                    if (deal.IsActive) deal.Status = "Live Flash Deal";
                }
            }
        }

        private static string FormatSeconds(long totalSecs)
        {
            if (totalSecs <= 0) return "00:00:00";
            var hours = totalSecs / 3600;
            var mins = (totalSecs % 3600) / 60;
            var secs = totalSecs % 60;
            return $"{hours:D2}:{mins:D2}:{secs:D2}";
        }
    }
}
