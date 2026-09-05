using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopNext.Helpers;
using ShopNext.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShopNext.Controllers
{
    /// <summary>
    /// Point 46: Universal Security & Identity Controller
    /// Handles centralized authentication routing, access denied security screens, and session sign-outs.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ShopNextDbContext _context;

        public AccountController(ShopNextDbContext context)
        {
            _context = context;
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.CurrentRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";
            ViewBag.CurrentName = User.Identity?.Name ?? "Guest User";
            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null, string? role = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Role = NormalizeRole(role);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string role, string identifier, string password, string? returnUrl = null)
        {
            role = NormalizeRole(role);
            identifier = identifier?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your login ID and password.";
                ViewBag.Role = role;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            string? name = null;
            int accountId = 0;
            bool authenticated = false;

            if (role == "Admin" || role == "Customer")
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Role == role && u.IsActive && !u.IsDeleted &&
                    (u.PhoneNumber == identifier || u.Email == identifier ||
                     (role == "Admin" && identifier.Equals("admin", StringComparison.OrdinalIgnoreCase))));

                if (user != null && SecurityHelper.VerifyPassword(password, user.Password ?? string.Empty))
                {
                    accountId = user.Id;
                    name = user.Name;
                    authenticated = true;
                }
            }
            else if (role == "Seller")
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s =>
                    s.PhoneNumber == identifier || s.Email == identifier);

                if (shop != null && shop.IsActive && !shop.IsDeleted &&
                    SecurityHelper.VerifyPassword(password, shop.Password ?? string.Empty))
                {
                    accountId = shop.Id;
                    name = shop.ShopName;
                    authenticated = true;
                }
            }
            else if (role == "Rider")
            {
                var rider = await _context.Riders.FirstOrDefaultAsync(r =>
                    r.PhoneNumber == identifier && r.IsActive && !r.IsDeleted);

                if (rider != null && SecurityHelper.VerifyPassword(password, rider.Password ?? string.Empty))
                {
                    accountId = rider.Id;
                    name = rider.RiderName;
                    authenticated = true;
                }
            }

            if (!authenticated)
            {
                ViewBag.Error = "Login ID, password, or account type is incorrect.";
                ViewBag.Role = role;
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Name, name ?? role),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserRole", role)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });

            if (role == "Customer")
            {
                Response.Cookies.Append("CustomerId", accountId.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
                Response.Cookies.Append("CustomerName", name ?? "Customer", new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
            }
            else if (role == "Seller")
            {
                Response.Cookies.Append("ShopId", accountId.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
                Response.Cookies.Append("ShopName", name ?? "Seller", new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
            }
            else if (role == "Rider")
            {
                Response.Cookies.Append("RiderId", accountId.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
                Response.Cookies.Append("RiderName", name ?? "Rider", new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(7) });
            }
            else
            {
                Response.Cookies.Append("AdminAuth", "true", new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddHours(8) });
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Redirect(role switch
            {
                "Admin" => "/Admin/Dashboard",
                "Seller" => "/Vendor/Dashboard",
                "Rider" => "/Rider/Dashboard",
                _ => "/Customer/Account"
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string? role = null)
        {
            ViewBag.Role = NormalizeRole(role);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string role, string identifier)
        {
            role = NormalizeRole(role);
            identifier = identifier?.Trim() ?? string.Empty;
            var accountIdentifier = await ResolveAccountIdentifierAsync(role, identifier);

            if (accountIdentifier == null)
            {
                ViewBag.Error = "No active account was found for these details.";
                ViewBag.Role = role;
                return View();
            }

            var activeTokens = await _context.PasswordResetTokens
                .Where(t => t.Role == role && t.Identifier == accountIdentifier && t.UsedAt == null && t.IsActive)
                .ToListAsync();
            foreach (var activeToken in activeTokens)
                activeToken.IsActive = false;

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                Role = role,
                Identifier = accountIdentifier,
                TokenHash = SecurityHelper.HashPassword(rawToken),
                ExpiresAt = DateTime.Now.AddMinutes(20),
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            ViewBag.Role = role;
            ViewBag.ResetLink = Url.Action("ResetPassword", "Account", new { role, token = rawToken });
            ViewBag.Message = "Reset link generated. Open it below to create a new password.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string role, string token)
        {
            var resetToken = await FindResetTokenAsync(role, token);
            if (resetToken == null)
            {
                ViewBag.Error = "This reset link is invalid or expired.";
                ViewBag.Role = NormalizeRole(role);
                return View();
            }

            ViewBag.Role = NormalizeRole(role);
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string role, string token, string newPassword, string confirmPassword)
        {
            role = NormalizeRole(role);
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6 || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords must match and contain at least 6 characters.";
                ViewBag.Role = role;
                ViewBag.Token = token;
                return View();
            }

            var resetToken = await FindResetTokenAsync(role, token);
            if (resetToken == null)
            {
                ViewBag.Error = "This reset link is invalid or expired.";
                ViewBag.Role = role;
                ViewBag.Token = token;
                return View();
            }

            var passwordHash = SecurityHelper.HashPassword(newPassword);
            if (role == "Customer" || role == "Admin")
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Role == role && u.PhoneNumber == resetToken.Identifier && !u.IsDeleted);
                if (user == null) return View("ResetPassword", new { Error = "Account not found." });
                user.Password = passwordHash;
            }
            else if (role == "Seller")
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PhoneNumber == resetToken.Identifier && !s.IsDeleted);
                if (shop == null) return View("ResetPassword", new { Error = "Account not found." });
                shop.Password = passwordHash;
            }
            else
            {
                var rider = await _context.Riders.FirstOrDefaultAsync(r => r.PhoneNumber == resetToken.Identifier && !r.IsDeleted);
                if (rider == null) return View("ResetPassword", new { Error = "Account not found." });
                rider.Password = passwordHash;
            }

            resetToken.UsedAt = DateTime.Now;
            resetToken.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password reset successfully. Please login with your new password.";
            return RedirectToAction(nameof(Login), new { role });
        }

        private static string NormalizeRole(string? role)
        {
            return role?.Trim().ToLowerInvariant() switch
            {
                "admin" => "Admin",
                "seller" or "vendor" => "Seller",
                "rider" => "Rider",
                _ => "Customer"
            };
        }

        private async Task<string?> ResolveAccountIdentifierAsync(string role, string identifier)
        {
            if (role == "Seller")
                return await _context.Shops.Where(s => (s.PhoneNumber == identifier || s.Email == identifier) && s.IsActive && !s.IsDeleted).Select(s => s.PhoneNumber).FirstOrDefaultAsync();
            if (role == "Rider")
                return await _context.Riders.Where(r => r.PhoneNumber == identifier && r.IsActive && !r.IsDeleted).Select(r => r.PhoneNumber).FirstOrDefaultAsync();

            return await _context.Users.Where(u => u.Role == role && (u.PhoneNumber == identifier || u.Email == identifier || (role == "Admin" && identifier.Equals("admin", StringComparison.OrdinalIgnoreCase))) && u.IsActive && !u.IsDeleted).Select(u => u.PhoneNumber).FirstOrDefaultAsync();
        }

        private async Task<PasswordResetToken?> FindResetTokenAsync(string role, string token)
        {
            var normalizedRole = NormalizeRole(role);
            var candidates = await _context.PasswordResetTokens.Where(t => t.Role == normalizedRole && t.UsedAt == null && t.IsActive && !t.IsDeleted && t.ExpiresAt > DateTime.Now).ToListAsync();
            return candidates.FirstOrDefault(t => SecurityHelper.VerifyPassword(token ?? string.Empty, t.TokenHash));
        }

        // GET: /Account/Logout
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clean legacy cookies for complete safety
            Response.Cookies.Delete("AdminAuth");
            Response.Cookies.Delete("ShopId");
            Response.Cookies.Delete("ShopName");
            Response.Cookies.Delete("CustomerId");
            Response.Cookies.Delete("CustomerName");
            Response.Cookies.Delete("CustomerPhone");
            Response.Cookies.Delete("RiderId");
            Response.Cookies.Delete("RiderName");

            return RedirectToAction("Index", "Home");
        }
    }
}
