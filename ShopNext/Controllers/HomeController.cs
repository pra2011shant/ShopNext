using Microsoft.AspNetCore.Mvc;
using ShopNext.Models;
using ShopNext.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNext.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IShopNextService _service;

        public HomeController(ILogger<HomeController> logger, IShopNextService service)
        {
            _logger = logger;
            _service = service;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(decimal? lat, decimal? lng, string? search)
        {
            var shops = await _service.GetAllShopsAsync();
            var approvedShops = shops.Where(s => s.IsApproved).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var query = search.Trim().ToLower();
                approvedShops = approvedShops.Where(s => 
                    s.ShopName.ToLower().Contains(query) || 
                    s.Category.ToLower().Contains(query) ||
                    (s.City != null && s.City.ToLower().Contains(query))
                ).ToList();
                ViewBag.SearchQuery = search;
            }

            var shopDtos = new List<ShopWithDistanceDto>();
            foreach (var s in approvedShops)
            {
                var sProducts = await _service.GetProductsByShopIdAsync(s.Id);
                var sOrders = await _service.GetOrdersByShopIdAsync(s.Id);
                var sReviews = await _service.GetReviewsByShopIdAsync(s.Id);

                double dist = -1.0;
                if (lat.HasValue && lng.HasValue)
                {
                    dist = CalculateDistance((double)lat.Value, (double)lng.Value, (double)s.Latitude, (double)s.Longitude);
                }

                shopDtos.Add(new ShopWithDistanceDto
                {
                    Shop = s,
                    Distance = dist,
                    Rating = sReviews.Any() ? Math.Round(sReviews.Average(r => r.Rating), 1) : 4.5,
                    TotalProducts = sProducts.Count(),
                    TotalOrders = sOrders.Count(),
                    Location = !string.IsNullOrWhiteSpace(s.City) ? s.City : (!string.IsNullOrWhiteSpace(s.Address) ? s.Address : "Patna"),
                    ReviewCount = sReviews.Count()
                });
            }

            if (lat.HasValue && lng.HasValue)
            {
                shopDtos = shopDtos.OrderBy(x => x.Distance).ToList();
                ViewBag.Latitude = lat.Value;
                ViewBag.Longitude = lng.Value;
            }

            ViewBag.SortedShops = shopDtos;

            // Point 41: Active promotional offers for "🔥 Today's Deals"
            var activeOffers = await _service.GetActiveOffersAsync();
            ViewBag.TodaysDeals = activeOffers.ToList();

            return View();
        }

        // GET: /Home/Products (Marketplace / All Products)
        [HttpGet]
        public async Task<IActionResult> Products(string? search, string? category, decimal? minPrice, decimal? maxPrice, string? sort)
        {
            var products = await _service.SearchProductsAsync(search, category, minPrice, maxPrice, sort);
            var allProducts = await _service.GetAllProductsAsync();
            
            var categories = allProducts
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Search = search;
            ViewBag.SelectedCategory = category ?? "All";
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort ?? "default";
            ViewBag.Categories = categories;

            return View(products);
        }

        // GET: /Home/ProductDetails
        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _service.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var reviews = await _service.GetReviewsByProductIdAsync(id);
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
            ViewBag.ReviewCount = reviews.Count();

            var related = (await _service.GetProductsByShopIdAsync(product.ShopId))
                .Where(p => p.Id != product.Id)
                .Take(4)
                .ToList();
            ViewBag.RelatedProducts = related;

            // Seller Shop Metrics (Point 30)
            if (product.Shop != null)
            {
                var sProducts = await _service.GetProductsByShopIdAsync(product.ShopId);
                var sOrders = await _service.GetOrdersByShopIdAsync(product.ShopId);
                var sReviews = await _service.GetReviewsByShopIdAsync(product.ShopId);
                ViewBag.ShopTotalProducts = sProducts.Count();
                ViewBag.ShopTotalOrders = sOrders.Count();
                ViewBag.ShopRating = sReviews.Any() ? Math.Round(sReviews.Average(r => r.Rating), 1) : 4.5;
                ViewBag.ShopLocation = !string.IsNullOrWhiteSpace(product.Shop.City) ? product.Shop.City : (!string.IsNullOrWhiteSpace(product.Shop.Address) ? product.Shop.Address : "Patna");
            }

            // Wishlist status for logged-in customer
            bool isWishlisted = false;
            if (Request.Cookies.TryGetValue("CustomerId", out string? cIdStr) && int.TryParse(cIdStr, out int customerId))
            {
                var wishlist = await _service.GetWishlistAsync(customerId);
                isWishlisted = wishlist.Any(w => w.ProductId == id);
            }
            ViewBag.IsWishlisted = isWishlisted;

            return View(product);
        }

        // GET: /Home/ShopCatalog
        [HttpGet]
        public async Task<IActionResult> ShopCatalog(int shopId)
        {
            var shop = await _service.GetShopByIdAsync(shopId);
            if (shop == null || !shop.IsApproved || shop.IsDeleted)
            {
                return NotFound("Shop not found or not approved.");
            }

            var products = (await _service.GetProductsByShopIdAsync(shopId)).ToList();
            var orders = (await _service.GetOrdersByShopIdAsync(shopId)).ToList();
            var reviews = (await _service.GetReviewsByShopIdAsync(shopId)).ToList();
            
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 4.5;
            ViewBag.ReviewCount = reviews.Count();
            ViewBag.TotalProducts = products.Count;
            ViewBag.TotalOrders = orders.Count;
            ViewBag.Location = !string.IsNullOrWhiteSpace(shop.City) ? shop.City : "Patna";
            ViewBag.Shop = shop;

            return View(products);
        }

        // GET: /Home/ShopProfile or /Shop/Profile (Point 30: Seller Shop Profile)
        [HttpGet]
        public async Task<IActionResult> ShopProfile(int? id, int? shopId)
        {
            int targetId = id ?? shopId ?? 1;
            var shop = await _service.GetShopByIdAsync(targetId);
            
            if (shop == null)
            {
                var allShops = await _service.GetAllShopsAsync();
                shop = allShops.FirstOrDefault(s => s.IsApproved) ?? new Shop
                {
                    Id = targetId,
                    ShopName = "ABC Electronics",
                    Category = "Electronics & Gadgets",
                    City = "Patna",
                    Address = "Boring Road, Patna, Bihar - 800001",
                    PhoneNumber = "9876543210",
                    OwnerName = "Anil Sharma",
                    IsApproved = true
                };
            }

            var products = (await _service.GetProductsByShopIdAsync(shop.Id)).ToList();
            var orders = (await _service.GetOrdersByShopIdAsync(shop.Id)).ToList();
            var reviews = (await _service.GetReviewsByShopIdAsync(shop.Id)).ToList();

            var vm = new SellerShopProfileViewModel
            {
                Shop = shop,
                ShopName = !string.IsNullOrWhiteSpace(shop.ShopName) ? shop.ShopName : "ABC Electronics",
                Rating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 4.5,
                TotalProducts = products.Count,
                TotalOrders = orders.Count,
                Location = !string.IsNullOrWhiteSpace(shop.City) ? shop.City : (!string.IsNullOrWhiteSpace(shop.Address) ? shop.Address : "Patna"),
                City = !string.IsNullOrWhiteSpace(shop.City) ? shop.City : "Patna",
                Address = shop.Address ?? "Boring Road, Patna, Bihar - 800001",
                Category = shop.Category ?? "Electronics & Gadgets",
                PhoneNumber = shop.PhoneNumber ?? "9876543210",
                OwnerName = shop.OwnerName ?? "Store Partner",
                ReviewCount = reviews.Count,
                Products = products,
                Reviews = reviews,
                IsVerifiedPartner = shop.IsApproved
            };

            return View(vm);
        }

        // GET: /Home/Cart
        [HttpGet]
        public IActionResult Cart()
        {
            return View();
        }

        // POST: /Home/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CustomerName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.DeliveryAddress) ||
                request.ShopId <= 0 || request.Items == null || !request.Items.Any())
            {
                return Json(new { success = false, message = "Invalid checkout details. Please fill all fields." });
            }

            try
            {
                // 1. Identify Customer (Check if logged in or find/create by phone)
                int customerId = 0;
                if (Request.Cookies.TryGetValue("CustomerId", out string? idStr) && int.TryParse(idStr, out int loggedInId))
                {
                    customerId = loggedInId;
                }
                else
                {
                    var customer = await _service.GetUserByPhoneNumberAsync(request.PhoneNumber.Trim());
                    if (customer == null)
                    {
                        var newUser = new User
                        {
                            Name = request.CustomerName.Trim(),
                            PhoneNumber = request.PhoneNumber.Trim(),
                            Role = "Customer",
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false
                        };
                        customerId = await _service.CreateUserAsync(newUser);
                    }
                    else
                    {
                        customerId = customer.Id;
                    }
                }

                // 1.5. Validate Customer Restrictions (Point 46)
                var customerObj = await _service.GetUserByIdAsync(customerId);
                if (customerObj != null)
                {
                    if (customerObj.RestrictionLevel == "Account Blocked" || !customerObj.IsActive || customerObj.IsDeleted)
                    {
                        return Json(new { success = false, message = "Your account has been blocked due to policy violations. Please contact customer support." });
                    }

                    if (customerObj.RestrictionLevel == "Account Suspended" || customerObj.IsAccountSuspended)
                    {
                        if (customerObj.SuspendedUntilDate.HasValue && customerObj.SuspendedUntilDate.Value > DateTime.Now)
                        {
                            string expiryStr = customerObj.SuspendedUntilDate.Value.ToString("dd MMM yyyy");
                            return Json(new { success = false, message = $"Your account is temporarily suspended until {expiryStr}. Reason: {customerObj.RestrictionReason ?? "Policy enforcement"}. New orders cannot be placed during suspension." });
                        }
                    }

                    string checkMode = string.IsNullOrWhiteSpace(request.PaymentMode) ? "COD" : request.PaymentMode.Trim().ToUpper();
                    if (checkMode == "COD" && (customerObj.RestrictionLevel == "COD Restricted" || customerObj.IsCodDisabled))
                    {
                        return Json(new { success = false, message = $"Cash on Delivery (COD) is restricted on your account ({customerObj.RestrictionReason ?? "Policy enforcement"}). You can still order using prepaid payment options (UPI / Card / NetBanking)." });
                    }
                }

                // 2. Prepare Order Items and calculate total
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();
                foreach (var itemDto in request.Items)
                {
                    var product = await _service.GetProductByIdAsync(itemDto.ProductId);
                    if (product == null || product.IsDeleted || !product.IsActive)
                    {
                        return Json(new { success = false, message = $"Product '{itemDto.ProductId}' is no longer available." });
                    }

                    if (product.StockStatus != "InStock")
                    {
                        return Json(new { success = false, message = $"Product '{product.ProductName}' is currently out of stock. Please remove it from your cart." });
                    }

                    decimal unitPrice = product.Price;
                    totalAmount += unitPrice * itemDto.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    });
                }

                // Apply coupon discount if present
                if (!string.IsNullOrWhiteSpace(request.CouponCode) && request.DiscountAmount > 0)
                {
                    totalAmount -= request.DiscountAmount;
                    if (totalAmount < 0) totalAmount = 0;
                }

                // Apply delivery fee (e.g. ₹99 for Express, ₹0 for Standard Free)
                if (request.DeliveryFee > 0)
                {
                    totalAmount += request.DeliveryFee;
                }

                // 3. Determine Payment Mode & Status
                string paymentMode = string.IsNullOrWhiteSpace(request.PaymentMode) ? "COD" : request.PaymentMode.Trim().ToUpper();
                string paymentStatus = (paymentMode == "COD") ? "Pending" : "Paid";

                // 4. Create Order
                var order = new Order
                {
                    CustomerId = customerId,
                    ShopId = request.ShopId,
                    TotalAmount = totalAmount,
                    OrderStatus = "Pending",
                    PaymentMode = paymentMode,
                    PaymentStatus = paymentStatus,
                    Remark = string.IsNullOrWhiteSpace(request.DeliveryInstructions) 
                        ? request.DeliveryAddress 
                        : $"{request.DeliveryAddress} [Delivery Note: {request.DeliveryInstructions}]",
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                int orderId = await _service.CreateOrderWithItemsAsync(order, orderItems);

                return Json(new { success = true, orderId = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return Json(new { success = false, message = "Error placing order: " + ex.Message });
            }
        }

        // GET: /Home/OrderSuccess
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var order = await _service.GetOrderDetailsAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var shop = await _service.GetShopByIdAsync(order.ShopId);
            ViewBag.ShopName = shop?.ShopName ?? "Shop";
            return View(order);
        }

        // GET: /Home/GetOrderRiderLocation
        [HttpGet]
        public async Task<IActionResult> GetOrderRiderLocation(int orderId)
        {
            if (orderId <= 0)
            {
                return Json(new { success = false, message = "Invalid Order ID." });
            }

            try
            {
                var order = await _service.GetOrderDetailsAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                if (order.RiderId.HasValue)
                {
                    var rider = await _service.GetRiderByIdAsync(order.RiderId.Value);
                    if (rider != null)
                    {
                        return Json(new {
                            success = true,
                            orderStatus = order.OrderStatus,
                            riderName = rider.RiderName,
                            phoneNumber = rider.PhoneNumber,
                            latitude = rider.CurrentLatitude,
                            longitude = rider.CurrentLongitude
                        });
                    }
                }

                return Json(new {
                    success = true,
                    orderStatus = order.OrderStatus,
                    riderName = (string?)null,
                    phoneNumber = (string?)null,
                    latitude = 0,
                    longitude = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/RefundPolicy (Point 72: Return & Refund Policy)
        public IActionResult RefundPolicy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Helper calculations
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Earth radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }

        // POST: /Home/ValidateCoupon
        [HttpPost]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
            {
                return Json(new { success = false, message = "Please enter a valid coupon code." });
            }

            var coupons = await _service.GetActiveCouponsAsync();
            var coupon = coupons.FirstOrDefault(c => string.Equals(c.Code, request.Code.Trim(), StringComparison.OrdinalIgnoreCase));

            if (coupon == null)
            {
                return Json(new { success = false, message = $"Coupon '{request.Code}' is invalid or expired." });
            }

            if (request.Subtotal < coupon.MinOrderAmount)
            {
                return Json(new { 
                    success = false, 
                    message = $"Coupon '{coupon.Code}' requires a minimum subtotal of ₹{coupon.MinOrderAmount.ToString("0.00")}." 
                });
            }

            decimal discount = 0;
            if (coupon.DiscountType == "Flat")
            {
                discount = coupon.DiscountValue;
            }
            else if (coupon.DiscountType == "Percent")
            {
                discount = Math.Round((request.Subtotal * coupon.DiscountValue) / 100m, 2);
            }
            else if (coupon.DiscountType == "FreeDelivery")
            {
                discount = 0; // Handled in UI as free delivery
            }

            if (discount > request.Subtotal) discount = request.Subtotal;

            return Json(new {
                success = true,
                code = coupon.Code,
                discountType = coupon.DiscountType,
                discountValue = coupon.DiscountValue,
                discountAmount = discount,
                message = $"Coupon '{coupon.Code}' applied successfully! You saved ₹{discount.ToString("0.00")}."
            });
        }

        // POST: /Home/CheckStockAndPrices
        [HttpPost]
        public async Task<IActionResult> CheckStockAndPrices([FromBody] List<CartItemCheckRequest> items)
        {
            if (items == null || !items.Any())
            {
                return Json(new { success = true, items = new List<object>() });
            }

            var result = new List<object>();
            bool hasPriceChange = false;
            bool hasStockIssue = false;

            foreach (var item in items)
            {
                var product = await _service.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    bool isOutOfStock = product.StockStatus != "InStock";
                    if (isOutOfStock) hasStockIssue = true;

                    bool priceUpdated = product.Price != item.ClientPrice;
                    if (priceUpdated) hasPriceChange = true;

                    result.Add(new {
                        productId = product.Id,
                        productName = product.ProductName,
                        currentPrice = product.Price,
                        oldPrice = item.ClientPrice,
                        priceChanged = priceUpdated,
                        stockStatus = product.StockStatus,
                        isAvailable = !isOutOfStock,
                        imageUrl = product.ImageUrl,
                        category = product.Category
                    });
                }
                else
                {
                    hasStockIssue = true;
                    result.Add(new {
                        productId = item.ProductId,
                        productName = "Unavailable Item",
                        currentPrice = item.ClientPrice,
                        oldPrice = item.ClientPrice,
                        priceChanged = false,
                        stockStatus = "OutOfStock",
                        isAvailable = false,
                        imageUrl = "",
                        category = "General"
                    });
                }
            }

            return Json(new {
                success = true,
                hasPriceChange = hasPriceChange,
                hasStockIssue = hasStockIssue,
                items = result
            });
        }
    }

    // DTO Classes
    public class ShopWithDistanceDto
    {
        public Shop Shop { get; set; } = null!;
        public double Distance { get; set; }
        public double Rating { get; set; } = 4.5;
        public int TotalProducts { get; set; } = 120;
        public int TotalOrders { get; set; } = 500;
        public string Location { get; set; } = "Patna";
        public int ReviewCount { get; set; } = 1;
    }

    public class CheckoutRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryInstructions { get; set; }
        public string? DeliverySpeed { get; set; }
        public decimal DeliveryFee { get; set; } = 0;
        public int ShopId { get; set; }
        public string PaymentMode { get; set; } = "COD"; // COD, UPI, CARD, NETBANKING
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
    }

    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CouponValidationRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
    }

    public class CartItemCheckRequest
    {
        public int ProductId { get; set; }
        public decimal ClientPrice { get; set; }
        public int Quantity { get; set; }
    }
}
