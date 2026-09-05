using Microsoft.AspNetCore.Mvc;
using ShopNext.Models;
using ShopNext.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNext.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class ApiController : ControllerBase
    {
        private readonly IShopNextService _service;

        public ApiController(IShopNextService service)
        {
            _service = service;
        }

        // ==========================================
        // 1. ORDER & RIDER LIVE TRACKING API
        // ==========================================
        [HttpGet("orders/{orderId:int}/tracking")]
        public async Task<IActionResult> GetOrderTracking(int orderId)
        {
            var order = await _service.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = $"Order #{orderId} not found." });
            }

            var shop = order.Shop ?? await _service.GetShopByIdAsync(order.ShopId);
            Rider? rider = null;
            if (order.RiderId.HasValue)
            {
                rider = await _service.GetRiderByIdAsync(order.RiderId.Value);
            }

            return Ok(new
            {
                success = true,
                orderId = order.Id,
                orderStatus = order.OrderStatus,
                totalAmount = order.TotalAmount,
                paymentMode = order.PaymentMode,
                createdDate = order.CreatedDate,
                shop = new
                {
                    shopName = shop?.ShopName ?? "Store",
                    latitude = shop?.Latitude ?? 28.6139m,
                    longitude = shop?.Longitude ?? 77.2090m
                },
                rider = rider != null ? new
                {
                    riderId = rider.Id,
                    riderName = rider.RiderName,
                    phoneNumber = rider.PhoneNumber,
                    latitude = rider.CurrentLatitude,
                    longitude = rider.CurrentLongitude,
                    isAvailable = rider.IsAvailable
                } : null,
                etaMinutes = (order.OrderStatus == "Dispatched" || order.OrderStatus == "OutForDelivery") ? 15 : 0
            });
        }

        // ==========================================
        // 1.1 POINT 42: DELIVERY EVIDENCE RETRIEVAL API
        // ==========================================
        [HttpGet("orders/{orderId:int}/delivery-evidence")]
        public async Task<IActionResult> GetDeliveryEvidence(int orderId)
        {
            var order = await _service.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new { success = false, message = $"Order #{orderId} not found." });
            }

            var evidence = await _service.GetDeliveryEvidenceAsync(orderId);

            return Ok(new
            {
                success = true,
                orderId = order.Id,
                orderStatus = order.OrderStatus,
                isOtpVerified = order.IsOtpVerified,
                deliveryTimestamp = order.DeliveredDate ?? order.OtpVerifiedDate ?? evidence?.UploadedDate,
                riderId = order.RiderId,
                riderName = order.DeliveredByRiderName ?? order.Rider?.RiderName ?? "Assigned Delivery Partner",
                evidenceRecord = evidence != null ? new
                {
                    evidenceId = evidence.Id,
                    title = evidence.Title,
                    description = evidence.Description,
                    uploadedDate = evidence.UploadedDate,
                    isVerified = evidence.IsVerified,
                    verifiedBy = evidence.VerifiedBy,
                    telemetry = evidence.MetadataJson
                } : null,
                privacyCompliance = new
                {
                    status = "Compliant",
                    details = "Maintains only OTP verification, delivery timestamp, rider ID, order ID, and delivery status. No facial or unnecessary customer personal data collected."
                }
            });
        }

        // ==========================================
        // 2. RIDER GPS COORDINATES UPDATE API
        // ==========================================
        [HttpPost("riders/{riderId:int}/location")]
        public async Task<IActionResult> UpdateRiderLocation(int riderId, [FromBody] RiderLocationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Coordinates required." });
            }

            var rider = await _service.GetRiderByIdAsync(riderId);
            if (rider == null)
            {
                return NotFound(new { success = false, message = $"Rider #{riderId} not found." });
            }

            await _service.UpdateRiderLocationAsync(riderId, dto.Latitude, dto.Longitude);
            return Ok(new
            {
                success = true,
                message = "Location updated successfully.",
                latitude = dto.Latitude,
                longitude = dto.Longitude
            });
        }

        // ==========================================
        // 3. PRODUCT & SHOP SEARCH AUTOCOMPLETE API
        // ==========================================
        [HttpGet("products/search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new { success = true, results = new List<object>() });
            }

            var query = q.Trim().ToLower();
            var allProducts = await _service.GetAllProductsAsync();
            var matched = allProducts
                .Where(p => p.ProductName.ToLower().Contains(query) ||
                            p.Category.ToLower().Contains(query) ||
                            (p.Brand != null && p.Brand.ToLower().Contains(query)))
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.ProductName,
                    category = p.Category,
                    brand = p.Brand,
                    price = p.Price,
                    mrp = p.Mrp,
                    discount = p.Discount,
                    stockStatus = p.StockStatus,
                    imageUrl = p.ImageUrl
                })
                .ToList();

            return Ok(new { success = true, count = matched.Count, results = matched });
        }

        // ==========================================
        // 4. HYPERLOCAL NEARBY SHOPS API
        // ==========================================
        [HttpGet("shops/nearby")]
        public async Task<IActionResult> GetNearbyShops([FromQuery] decimal lat, [FromQuery] decimal lng, [FromQuery] double radiusKm = 15.0)
        {
            var shops = await _service.GetAllShopsAsync();
            var approved = shops.Where(s => s.IsApproved).ToList();

            double userLat = (double)lat;
            double userLng = (double)lng;

            var nearby = approved
                .Select(s => new
                {
                    shop = s,
                    distanceKm = Math.Round(CalculateDistance(userLat, userLng, (double)s.Latitude, (double)s.Longitude), 2)
                })
                .Where(x => x.distanceKm <= radiusKm)
                .OrderBy(x => x.distanceKm)
                .Select(x => new
                {
                    id = x.shop.Id,
                    shopName = x.shop.ShopName,
                    category = x.shop.Category,
                    address = x.shop.Address,
                    latitude = x.shop.Latitude,
                    longitude = x.shop.Longitude,
                    distanceKm = x.distanceKm
                })
                .ToList();

            return Ok(new { success = true, count = nearby.Count, shops = nearby });
        }

        // ==========================================
        // 5. REAL-TIME SELLER METRICS API
        // ==========================================
        [HttpGet("seller/{shopId:int}/summary")]
        public async Task<IActionResult> GetSellerSummary(int shopId)
        {
            var shop = await _service.GetShopByIdAsync(shopId);
            if (shop == null)
            {
                return NotFound(new { success = false, message = "Shop not found." });
            }

            var products = (await _service.GetProductsByShopIdAsync(shopId)).ToList();
            var orders = (await _service.GetOrdersByShopIdAsync(shopId)).ToList();

            return Ok(new
            {
                success = true,
                shopId = shop.Id,
                shopName = shop.ShopName,
                isApproved = shop.IsApproved,
                metrics = new
                {
                    totalProducts = products.Count,
                    totalOrders = orders.Count,
                    pendingOrders = orders.Count(o => o.OrderStatus == "Placed" || o.OrderStatus == "Confirmed" || o.OrderStatus == "Packed" || o.OrderStatus == "Pending"),
                    totalSales = orders.Where(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Completed" || o.PaymentStatus == "Paid").Sum(o => o.TotalAmount),
                    pendingReturns = orders.Count(o => o.OrderStatus == "Return Requested" || o.OrderStatus == "Return Approved" || !string.IsNullOrEmpty(o.ReturnReason)),
                    lowStock = products.Count(p => p.Stock <= 5 || p.StockStatus == "OutOfStock" || p.StockStatus == "LowStock")
                }
            });
        }

        // Haversine distance calculator
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Earth radius in km
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }

    public class RiderLocationDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
