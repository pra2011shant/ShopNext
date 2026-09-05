using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNext.Helpers;
using ShopNext.Models;
using ShopNext.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShopNext.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class CustomerController : Controller
    {
        private readonly IShopNextService _service;
        private readonly ILogger<CustomerController> _logger;
        private readonly IAuditService _auditService;

        public CustomerController(IShopNextService service, ILogger<CustomerController> logger, IAuditService auditService)
        {
            _service = service;
            _logger = logger;
            _auditService = auditService;
        }

        private bool IsCustomerLoggedIn(out int customerId, out string customerName)
        {
            customerId = 0;
            customerName = string.Empty;

            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Customer") || User.IsInRole("Admin")))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out int cId))
                {
                    customerId = cId;
                    customerName = User.Identity?.Name ?? "Customer";
                    return true;
                }
            }

            if (Request.Cookies.TryGetValue("CustomerId", out string? idStr) && int.TryParse(idStr, out int id))
            {
                customerId = id;
                if (Request.Cookies.TryGetValue("CustomerName", out string? name))
                {
                    customerName = name ?? "Customer";
                }
                return true;
            }
            return false;
        }

        // GET: /Customer/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (IsCustomerLoggedIn(out _, out _))
            {
                return RedirectToAction("Account");
            }
            return View();
        }

        // POST: /Customer/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string phoneNumber, string? email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all mandatory fields (Name, Phone Number, Password).";
                return View();
            }

            if (!Regex.IsMatch(phoneNumber.Trim(), @"^[6-9]\d{9}$"))
            {
                ViewBag.Error = "Please enter a valid 10-digit Indian mobile number.";
                return View();
            }

            if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.Error = "Please provide a valid email address.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            try
            {
                var newUser = new User
                {
                    Name = name.Trim(),
                    PhoneNumber = phoneNumber.Trim(),
                    Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    Password = SecurityHelper.HashPassword(password),
                    Role = "Customer",
                    CreatedAt = DateTime.Now
                };

                int customerId = await _service.RegisterCustomerAsync(newUser);
                if (customerId == -1)
                {
                    ViewBag.Error = "An account with this phone number or email already exists. Please login instead.";
                    return View();
                }

                // Point 46: Claims-based Identity SignIn for Customer Role
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
                    new Claim(ClaimTypes.Name, newUser.Name),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("UserRole", "Customer"),
                    new Claim(ClaimTypes.MobilePhone, newUser.PhoneNumber)
                };
                if (!string.IsNullOrEmpty(newUser.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, newUser.Email));
                }
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

                Response.Cookies.Append("CustomerId", customerId.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });
                Response.Cookies.Append("CustomerName", newUser.Name, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });
                Response.Cookies.Append("CustomerPhone", newUser.PhoneNumber, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });

                return RedirectToAction("Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering customer");
                ViewBag.Error = "An unexpected error occurred during registration. Please try again.";
                return View();
            }
        }

        // GET: /Customer/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl, role = "Customer" });
        }

        // POST: /Customer/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string loginIdentifier, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(loginIdentifier) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your Phone Number/Email and Password.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var customer = await _service.CustomerLoginAsync(loginIdentifier, password);
            if (customer == null)
            {
                ViewBag.Error = "Invalid credentials. Please check your phone/email and password.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Point 46: Claims-based Identity SignIn for Customer Role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Name, customer.Name),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim("UserRole", "Customer"),
                new Claim(ClaimTypes.MobilePhone, customer.PhoneNumber)
            };
            if (!string.IsNullOrEmpty(customer.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, customer.Email));
            }
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            });

            Response.Cookies.Append("CustomerId", customer.Id.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });
            Response.Cookies.Append("CustomerName", customer.Name, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });
            Response.Cookies.Append("CustomerPhone", customer.PhoneNumber, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Account");
        }

        // GET: /Customer/Logout
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("CustomerId");
            Response.Cookies.Delete("CustomerName");
            Response.Cookies.Delete("CustomerPhone");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Customer/Account (Unified 12-Module Customer Hub)
        [HttpGet]
        public async Task<IActionResult> Account(string tab = "profile")
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return RedirectToAction("Login", new { returnUrl = "/Customer/Account?tab=" + tab });
            }

            var customer = await _service.GetUserByIdAsync(customerId);
            if (customer == null)
            {
                return RedirectToAction("Logout");
            }

            // Load module data
            ViewBag.ActiveTab = tab.ToLower();
            ViewBag.Addresses = await _service.GetCustomerAddressesAsync(customerId);
            ViewBag.Orders = await _service.GetOrdersByCustomerIdAsync(customerId);
            ViewBag.Wishlist = await _service.GetWishlistAsync(customerId);
            ViewBag.Notifications = await _service.GetNotificationsAsync(customerId);
            ViewBag.Coupons = await _service.GetActiveCouponsAsync();
            ViewBag.Reviews = await _service.GetReviewsByCustomerIdAsync(customerId);

            return View(customer);
        }

        // Backward compatibility & direct links
        [HttpGet]
        public IActionResult Profile() => RedirectToAction("Account", new { tab = "profile" });

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return RedirectToAction("Login", new { returnUrl = "/Customer/MyOrders" });
            }
            var orders = await _service.GetOrdersByCustomerIdAsync(customerId);
            return View(orders);
        }

        [HttpGet]
        public IActionResult Wishlist() => RedirectToAction("Account", new { tab = "wishlist" });

        // POST: /Customer/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string name, string? email, string? profilePhoto, string? gender, DateTime? dob)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Name cannot be empty.";
                return RedirectToAction("Account", new { tab = "profile" });
            }

            bool result = await _service.UpdateCustomerProfileAsync(customerId, name, email, profilePhoto, gender, dob);
            if (result)
            {
                Response.Cookies.Append("CustomerName", name.Trim(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(14), HttpOnly = true });
                TempData["Success"] = "Profile details updated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update profile details.";
            }

            return RedirectToAction("Account", new { tab = "profile" });
        }

        // POST: /Customer/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Please fill in all password fields.";
                return RedirectToAction("Account", new { tab = "password" });
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long.";
                return RedirectToAction("Account", new { tab = "password" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Account", new { tab = "password" });
            }

            bool result = await _service.ChangePasswordAsync(customerId, currentPassword, newPassword);
            if (result)
            {
                TempData["Success"] = "Password updated successfully!";
            }
            else
            {
                TempData["Error"] = "Current password is incorrect.";
            }

            return RedirectToAction("Account", new { tab = "password" });
        }

        // POST: /Customer/ToggleWishlist
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login to manage your wishlist." });
            }

            bool isAdded = await _service.ToggleWishlistAsync(customerId, productId);
            return Json(new { 
                success = true, 
                isAdded = isAdded, 
                message = isAdded ? "Added to Wishlist! ❤️" : "Removed from Wishlist." 
            });
        }

        // POST: /Customer/RemoveFromWishlist
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login." });
            }

            bool result = await _service.RemoveFromWishlistAsync(customerId, productId);
            return Json(new { success = result });
        }

        // POST: /Customer/MarkNotificationRead
        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int notificationId)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false });
            }

            bool result = await _service.MarkNotificationReadAsync(notificationId, customerId);
            return Json(new { success = result });
        }

        // POST: /Customer/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId, string cancelReason)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            if (string.IsNullOrWhiteSpace(cancelReason))
            {
                return Json(new { success = false, message = "Please provide a cancellation reason." });
            }

            bool result = await _service.CancelOrderAsync(orderId, customerId, cancelReason.Trim());
            if (result)
            {
                return Json(new { success = true, message = "Order cancelled successfully." });
            }
            return Json(new { success = false, message = "Only Placed, Confirmed, or Packed orders can be cancelled. Shipped orders cannot be cancelled." });
        }

        // POST: /Customer/ReturnOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnOrder(int orderId, string returnReason)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            if (string.IsNullOrWhiteSpace(returnReason))
            {
                return Json(new { success = false, message = "Please select or enter a return reason." });
            }

            bool result = await _service.ReturnOrderAsync(orderId, customerId, returnReason.Trim());
            if (result)
            {
                return Json(new { success = true, message = "Return request initiated successfully." });
            }
            return Json(new { success = false, message = "Only Delivered orders can be returned." });
        }

        // POST: /Customer/ConfirmReceived
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReceived(int orderId)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            bool result = await _service.ConfirmOrderReceivedAsync(orderId, customerId);
            if (result)
            {
                return Json(new { success = true, message = "Order delivery confirmed! Thank you for shopping with us." });
            }
            return Json(new { success = false, message = "Unable to confirm delivery for this order status." });
        }

        // POST: /Customer/UpdateRefundStatus (for testing/interactive refund tracking)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRefundStatus(int orderId, string nextStatus)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login first." });
            }

            var order = await _service.GetOrderByIdAsync(orderId);
            if (order == null || order.CustomerId != customerId)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            var allowed = new[] { "ReturnRequested", "ReturnApproved", "ProductPickup", "ProductReceived", "RefundInitiated", "RefundCompleted" };
            if (!allowed.Contains(nextStatus))
            {
                return Json(new { success = false, message = "Invalid refund stage." });
            }

            bool result = await _service.UpdateOrderStatusAsync(orderId, nextStatus);
            return Json(new { success = result, nextStatus, message = $"Refund status moved to '{nextStatus}'." });
        }

        // POST: /Customer/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int shopId, int? productId, int? orderId, int rating, string comment)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login to submit a review." });
            }

            // Strict Rule: Sirf delivered product ko review karne do (Fake reviews rokne ke liye)
            if (orderId.HasValue && orderId.Value > 0)
            {
                var order = await _service.GetOrderDetailsAsync(orderId.Value);
                if (order == null || order.CustomerId != customerId)
                {
                    return Json(new { success = false, message = "Order not found or invalid customer." });
                }

                if (order.OrderStatus != "Completed" && order.OrderStatus != "Delivered")
                {
                    return Json(new { success = false, message = "Sirf delivered product ko review karne ki permission hai taaki fake reviews na ho sakein." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Verified delivered order required to post a review." });
            }

            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Please provide a rating between 1 and 5 stars." });
            }

            var review = new Review
            {
                CustomerId = customerId,
                ShopId = shopId,
                ProductId = productId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment?.Trim()
            };

            int reviewId = await _service.InsertReviewAsync(review);
            return Json(new { success = true, message = "Thank you! Your verified purchase review has been submitted.", reviewId });
        }

        // AJAX: /Customer/GetAddresses
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, addresses = Array.Empty<object>() });
            }

            var addresses = await _service.GetCustomerAddressesAsync(customerId);
            return Json(new { success = true, addresses });
        }

        // AJAX: /Customer/SaveAddress
        [HttpPost]
        public async Task<IActionResult> SaveAddress([FromBody] CustomerAddress address)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login to save addresses." });
            }

            if (string.IsNullOrWhiteSpace(address.RecipientName) ||
                string.IsNullOrWhiteSpace(address.PhoneNumber) ||
                string.IsNullOrWhiteSpace(address.AddressLine) ||
                string.IsNullOrWhiteSpace(address.Pincode))
            {
                return Json(new { success = false, message = "Please fill in recipient name, phone, address line, and pincode." });
            }

            address.CustomerId = customerId;
            int newId = await _service.InsertCustomerAddressAsync(address);

            return Json(new { success = true, id = newId, message = "Address saved successfully!" });
        }

        // AJAX: /Customer/DeleteAddress
        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login." });
            }

            bool result = await _service.DeleteCustomerAddressAsync(id, customerId);
            return Json(new { success = result });
        }

        // ==========================================
        // POINT 43: CUSTOMER COMPLAINTS / SUPPORT
        // ==========================================

        // POST: /Customer/RaiseComplaint
        [HttpPost]
        public async Task<IActionResult> RaiseComplaint(int orderId, string issue, string description, string? attachmentUrl)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Please login to submit a support ticket." });
            }

            if (orderId <= 0)
            {
                return Json(new { success = false, message = "Invalid order specified." });
            }

            if (string.IsNullOrWhiteSpace(issue))
            {
                return Json(new { success = false, message = "Please select an issue category." });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return Json(new { success = false, message = "Please enter a detailed description of the issue." });
            }

            var complaint = new Complaint
            {
                CustomerId = customerId,
                OrderId = orderId,
                Issue = issue.Trim(),
                Description = description.Trim(),
                AttachmentUrl = string.IsNullOrWhiteSpace(attachmentUrl) ? null : attachmentUrl.Trim(),
                Priority = "High",
                Status = "Open"
            };

            var created = await _service.CreateComplaintAsync(complaint);

            return Json(new
            {
                success = true,
                message = "Your complaint has been submitted successfully! Our support team will review it shortly.",
                ticketId = created.Id,
                ticketNumber = created.TicketNumber,
                status = created.Status,
                date = created.CreatedDate.ToString("dd MMM yyyy, hh:mm tt")
            });
        }

        // GET: /Customer/GetOrderComplaints?orderId={orderId}
        [HttpGet]
        public async Task<IActionResult> GetOrderComplaints(int orderId)
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            var complaints = await _service.GetComplaintsByOrderIdAsync(orderId);
            var result = complaints.Where(c => c.CustomerId == customerId).Select(c => new
            {
                id = c.Id,
                ticketNumber = c.TicketNumber,
                issue = c.Issue,
                description = c.Description,
                attachmentUrl = c.AttachmentUrl,
                status = c.Status,
                date = c.CreatedDate.ToString("dd MMM yyyy, hh:mm tt"),
                resolutionNotes = c.ResolutionNotes,
                resolvedDate = c.ResolvedDate?.ToString("dd MMM yyyy, hh:mm tt")
            });

            return Json(new { success = true, complaints = result });
        }

        // ==========================================
        // POINT 44: CUSTOMER NOTIFICATIONS
        // ==========================================

        // GET: /Customer/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!IsCustomerLoggedIn(out int customerId, out _))
            {
                return Json(new { success = false, notifications = Array.Empty<object>(), unreadCount = 0 });
            }

            var notifications = await _service.GetNotificationsForRoleAsync("Customer", customerId: customerId);
            var dtoList = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type,
                time = n.CreatedDate.ToString("dd MMM yyyy, hh:mm tt"),
                linkUrl = n.LinkUrl ?? "/Customer/MyOrders",
                isRead = n.IsRead
            });

            int unread = notifications.Count(n => !n.IsRead);
            return Json(new { success = true, notifications = dtoList, unreadCount = unread });
        }
    }
}
