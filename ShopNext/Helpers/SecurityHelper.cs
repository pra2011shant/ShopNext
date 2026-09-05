using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace ShopNext.Helpers
{
    public static class SecurityHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Validates that an uploaded file is a safe, allowed image.
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateImageFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (false, "No file was uploaded.");

            if (file.Length > MaxFileSize)
                return (false, "File size exceeds the 5MB limit.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return (false, "Invalid file format. Allowed formats: .jpg, .jpeg, .png, .webp, .gif");

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "Invalid image MIME type.");

            return (true, null);
        }

        /// <summary>
        /// Generates a sanitized unique filename to avoid path traversal and overwrites.
        /// </summary>
        public static string GenerateSafeFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var sanitizedBase = Regex.Replace(Path.GetFileNameWithoutExtension(originalFileName), @"[^a-zA-Z0-9_\-]", "");
            if (sanitizedBase.Length > 30) sanitizedBase = sanitizedBase.Substring(0, 30);
            return $"{sanitizedBase}_{Guid.NewGuid():N}{extension}";
        }

        /// <summary>
        /// Computes SHA256 cryptographic hash with salt for secure password comparison.
        /// </summary>
        public static string HashPassword(string password, string salt = "ShopNext_Secure_Salt_2026")
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha256 = SHA256.Create();
            var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(combinedBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a plain text password against a hashed password or plain password fallback.
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedPassword, string salt = "ShopNext_Secure_Salt_2026")
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword)) return false;

            // Check if storedPassword is plain text match (legacy/demo records)
            if (inputPassword == storedPassword) return true;

            // Check hashed match
            var hashedInput = HashPassword(inputPassword, salt);
            return hashedInput == storedPassword;
        }
    }
}
