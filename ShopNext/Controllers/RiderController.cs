using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNext.Helpers;
using ShopNext.Models;
using ShopNext.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShopNext.Controllers
{
    [Authorize(Roles = "Rider,Admin")]
    public class RiderController : Controller
    {
        private readonly IShopNextService _service;

        public RiderController(IShopNextService service)
        {
            _service = service;
        }

        // Helper to check if rider is logged in
        private bool IsLoggedIn(out int riderId, out string riderName)
        {
            riderId = 0;
            riderName = string.Empty;

            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Rider") || User.IsInRole("Admin")))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out int rId))
                {
                    riderId = rId;
                    riderName = User.Identity?.Name ?? "Rider";
                    return true;
                }
            }

            if (Request.Cookies.TryGetValue("RiderId", out string? riderIdStr) && int.TryParse(riderIdStr, out int id))
            {
                riderId = id;
                if (Request.Cookies.TryGetValue("RiderName", out string? name))
                {
                    riderName = name ?? "";
                }
                return true;
            }
            return false;
        }

        // GET: /Rider/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (IsLoggedIn(out _, out _))
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        // POST: /Rider/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string riderName, string phoneNumber, string password, decimal latitude, decimal longitude)
        {
            if (string.IsNullOrWhiteSpace(riderName) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            // Strict Mobile validation (10 digits starting with 6-9)
            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber.Trim(), @"^[6-9]\d{9}$"))
            {
                ViewBag.Error = "Invalid Indian mobile number. Must be 10 digits starting with 6, 7, 8, or 9.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                return View();
            }

            try
            {
                var existing = await _service.GetRiderByPhoneNumberAsync(phoneNumber);
                if (existing != null)
                {
                    ViewBag.Error = "A rider is already registered with this phone number. Please login.";
                    return View();
                }

                var rider = new Rider
                {
                    RiderName = riderName.Trim(),
                    PhoneNumber = phoneNumber.Trim(),
                    Password = SecurityHelper.HashPassword(password),
                    CurrentLatitude = latitude,
                    CurrentLongitude = longitude,
                    IsAvailable = true,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                int riderId = await _service.CreateRiderAsync(rider);

                // Point 46: Claims-based Identity SignIn for Rider Role
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, riderId.ToString()),
                    new Claim(ClaimTypes.Name, riderName.Trim()),
                    new Claim(ClaimTypes.Role, "Rider"),
                    new Claim("UserRole", "Rider"),
                    new Claim(ClaimTypes.MobilePhone, phoneNumber.Trim())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

                // Log them in
                Response.Cookies.Append("RiderId", riderId.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true });
                Response.Cookies.Append("RiderName", riderName, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true });

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error registering rider: " + ex.Message;
                return View();
            }
        }

        // GET: /Rider/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account", new { role = "Rider" });
        }

        // POST: /Rider/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string phoneNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Phone number is required.";
                return View();
            }

            var rider = await _service.GetRiderByPhoneNumberAsync(phoneNumber);
            if (rider == null)
            {
                ViewBag.Error = "No rider registered with this phone number.";
                return View();
            }

            if (!SecurityHelper.VerifyPassword(password, rider.Password ?? string.Empty))
            {
                ViewBag.Error = "Incorrect password.";
                return View();
            }

            // Point 46: Claims-based Identity SignIn for Rider Role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, rider.Id.ToString()),
                new Claim(ClaimTypes.Name, rider.RiderName),
                new Claim(ClaimTypes.Role, "Rider"),
                new Claim("UserRole", "Rider"),
                new Claim(ClaimTypes.MobilePhone, rider.PhoneNumber)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Log them in
            Response.Cookies.Append("RiderId", rider.Id.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true });
            Response.Cookies.Append("RiderName", rider.RiderName, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), HttpOnly = true });

            return RedirectToAction("Dashboard");
        }

        // GET: /Rider/Logout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("RiderId");
            Response.Cookies.Delete("RiderName");
            return RedirectToAction("Login");
        }

        // GET: /Rider/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn(out int riderId, out string riderName))
            {
                return RedirectToAction("Login");
            }

            ViewBag.RiderId = riderId;
            ViewBag.RiderName = riderName;

            var rider = await _service.GetRiderByIdAsync(riderId);
            ViewBag.RiderLatitude = rider?.CurrentLatitude ?? 0;
            ViewBag.RiderLongitude = rider?.CurrentLongitude ?? 0;

            var assignedOrders = await _service.GetRiderAssignedOrdersAsync(riderId);
            return View(assignedOrders);
        }

        // POST: /Rider/UpdateLocation
        [HttpPost]
        public async Task<IActionResult> UpdateLocation(decimal latitude, decimal longitude)
        {
            if (!IsLoggedIn(out int riderId, out _))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                bool result = await _service.UpdateRiderLocationAsync(riderId, latitude, longitude);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Rider/UpdateOrderStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsLoggedIn(out _, out _))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                bool result = await _service.UpdateOrderStatusAsync(orderId, status);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Rider/VerifyDeliveryOtp
        [HttpPost]
        public async Task<IActionResult> VerifyDeliveryOtp(int orderId, string otp, string? notes)
        {
            if (!IsLoggedIn(out int riderId, out _))
            {
                return Json(new { success = false, message = "Unauthorized. Please login again." });
            }

            if (string.IsNullOrWhiteSpace(otp))
            {
                return Json(new { success = false, message = "Please enter the 4-digit Delivery OTP provided by the customer." });
            }

            try
            {
                var result = await _service.VerifyDeliveryOtpAndCompleteAsync(orderId, riderId, otp.Trim(), notes);
                return Json(new 
                { 
                    success = result.Success, 
                    message = result.Message,
                    orderId = orderId,
                    otp = result.Otp
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error verifying delivery OTP: " + ex.Message });
            }
        }
    }
}
