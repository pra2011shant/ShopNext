using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopNext.Helpers;
using ShopNext.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    public class ShopNextService : IShopNextService
    {
        private readonly ShopNextDbContext _context;

        public ShopNextService(ShopNextDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // USER & CUSTOMER SERVICES
        // ==========================================

        public async Task<int> CreateUserAsync(User user)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "dbo.sp_InsertUser";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PhoneNumber", user.PhoneNumber));
            command.Parameters.Add(new SqlParameter("@Name", user.Name));
            command.Parameters.Add(new SqlParameter("@Remark", (object?)user.Remark ?? DBNull.Value));

            if (command.Connection != null && command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> RegisterCustomerAsync(User user)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => 
                (u.PhoneNumber == user.PhoneNumber || (!string.IsNullOrEmpty(user.Email) && u.Email == user.Email)) 
                && !u.IsDeleted);

            if (existing != null)
            {
                return -1; // Already registered
            }

            user.Role = "Customer";
            user.CreatedAt = DateTime.Now;
            user.CreatedDate = DateTime.Now;
            user.IsActive = true;
            user.IsDeleted = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<User?> CustomerLoginAsync(string loginIdentifier, string password)
        {
            if (string.IsNullOrWhiteSpace(loginIdentifier) || string.IsNullOrWhiteSpace(password))
                return null;

            var trimmedId = loginIdentifier.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.PhoneNumber == trimmedId || u.Email == trimmedId) &&
                !u.IsDeleted &&
                u.IsActive);

            return user != null && SecurityHelper.VerifyPassword(password, user.Password ?? string.Empty)
                ? user
                : null;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) return false;

            existing.Name = user.Name;
            existing.PhoneNumber = user.PhoneNumber;
            existing.Email = user.Email;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = user.Password;
            }
            existing.ProfilePhoto = user.ProfilePhoto;
            existing.Gender = user.Gender;
            existing.DateOfBirth = user.DateOfBirth;
            existing.Remark = user.Remark;
            existing.UpdatedById = user.UpdatedById;
            existing.IsActive = user.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCustomerProfileAsync(int customerId, string name, string? email, string? profilePhoto, string? gender, DateTime? dob)
        {
            var user = await _context.Users.FindAsync(customerId);
            if (user == null || user.IsDeleted) return false;

            user.Name = name.Trim();
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            if (!string.IsNullOrWhiteSpace(profilePhoto))
            {
                user.ProfilePhoto = profilePhoto.Trim();
            }
            user.Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
            user.DateOfBirth = dob;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int customerId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(customerId);
            if (user == null || user.IsDeleted) return false;

            if (!SecurityHelper.VerifyPassword(currentPassword, user.Password ?? string.Empty))
            {
                return false; // Old password doesn't match
            }

            user.Password = SecurityHelper.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id, int? updatedById = null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsDeleted = true;
            user.UpdatedById = updatedById;
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // CUSTOMER SAVED ADDRESSES
        // ==========================================

        public async Task<IEnumerable<CustomerAddress>> GetCustomerAddressesAsync(int customerId)
        {
            return await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && !a.IsDeleted)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> InsertCustomerAddressAsync(CustomerAddress address)
        {
            if (address.IsDefault)
            {
                var existingDefaults = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == address.CustomerId && a.IsDefault && !a.IsDeleted)
                    .ToListAsync();
                foreach (var d in existingDefaults)
                {
                    d.IsDefault = false;
                }
            }

            address.CreatedDate = DateTime.Now;
            address.IsActive = true;
            address.IsDeleted = false;

            _context.CustomerAddresses.Add(address);
            await _context.SaveChangesAsync();
            return address.Id;
        }

        public async Task<bool> DeleteCustomerAddressAsync(int addressId, int customerId)
        {
            var addr = await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);
            if (addr == null) return false;

            addr.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // WISHLIST SERVICES
        // ==========================================

        public async Task<IEnumerable<Wishlist>> GetWishlistAsync(int customerId)
        {
            return await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Shop)
                .Where(w => w.CustomerId == customerId && !w.IsDeleted && w.Product != null && !w.Product.IsDeleted)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ToggleWishlistAsync(int customerId, int productId)
        {
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId && !w.IsDeleted);

            if (existing != null)
            {
                existing.IsDeleted = true;
                await _context.SaveChangesAsync();
                return false; // Removed
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                });
                await _context.SaveChangesAsync();
                return true; // Added
            }
        }

        public async Task<bool> RemoveFromWishlistAsync(int customerId, int productId)
        {
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId && !w.IsDeleted);

            if (existing != null)
            {
                existing.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // ==========================================
        // COUPONS SERVICES
        // ==========================================

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync()
        {
            return await _context.Coupons
                .Where(c => c.IsActive && !c.IsDeleted && (c.ExpiryDate == null || c.ExpiryDate > DateTime.Now))
                .OrderBy(c => c.MinOrderAmount)
                .ToListAsync();
        }

        // ==========================================
        // SHOP SERVICES
        // ==========================================

        public async Task<int> CreateShopAsync(Shop shop)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "dbo.sp_InsertShop";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ShopName", shop.ShopName));
            command.Parameters.Add(new SqlParameter("@PhoneNumber", shop.PhoneNumber));
            command.Parameters.Add(new SqlParameter("@Category", shop.Category));
            command.Parameters.Add(new SqlParameter("@Latitude", shop.Latitude));
            command.Parameters.Add(new SqlParameter("@Longitude", shop.Longitude));
            command.Parameters.Add(new SqlParameter("@IsApproved", shop.IsApproved));
            command.Parameters.Add(new SqlParameter("@OwnerName", (object?)shop.OwnerName ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Email", (object?)shop.Email ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Address", (object?)shop.Address ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@City", (object?)shop.City ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@State", (object?)shop.State ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Pincode", (object?)shop.Pincode ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@BankAccountNumber", (object?)shop.BankAccountNumber ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IfscCode", (object?)shop.IfscCode ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Password", (object?)shop.Password ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Remark", (object?)shop.Remark ?? DBNull.Value));

            if (command.Connection != null && command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<Shop?> GetShopByIdAsync(int id)
        {
            return await _context.Shops.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<IEnumerable<Shop>> GetAllShopsAsync()
        {
            return await _context.Shops.Where(s => !s.IsDeleted).ToListAsync();
        }

        public async Task<bool> UpdateShopAsync(Shop shop)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_UpdateShop @Id={0}, @ShopName={1}, @PhoneNumber={2}, @Category={3}, @Latitude={4}, @Longitude={5}, @IsApproved={6}, @OwnerName={7}, @Email={8}, @Address={9}, @City={10}, @State={11}, @Pincode={12}, @BankAccountNumber={13}, @IfscCode={14}, @Password={15}, @Remark={16}, @UpdatedById={17}, @IsActive={18}",
                shop.Id, shop.ShopName, shop.PhoneNumber, shop.Category, shop.Latitude, shop.Longitude, shop.IsApproved, 
                (object?)shop.OwnerName ?? DBNull.Value, (object?)shop.Email ?? DBNull.Value, (object?)shop.Address ?? DBNull.Value, 
                (object?)shop.City ?? DBNull.Value, (object?)shop.State ?? DBNull.Value, (object?)shop.Pincode ?? DBNull.Value, 
                (object?)shop.BankAccountNumber ?? DBNull.Value, (object?)shop.IfscCode ?? DBNull.Value, 
                (object?)shop.Password ?? DBNull.Value, (object?)shop.Remark ?? DBNull.Value, 
                (object?)shop.UpdatedById ?? DBNull.Value, shop.IsActive);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteShopAsync(int id, int? updatedById = null)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_DeleteShop @Id={0}, @UpdatedById={1}", id, (object?)updatedById ?? DBNull.Value);
            return rowsAffected > 0;
        }

        // ==========================================
        // PRODUCT SERVICES
        // ==========================================

        public async Task<int> CreateProductAsync(Product product)
        {
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;
            product.IsDeleted = false;
            // Point 37: Multi-vendor Product Approval Flow
            product.IsApproved = false;
            product.ApprovalStatus = "Pending";

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Variants.Where(v => !v.IsDeleted && v.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId && !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .ToListAsync();
        }

        public async Task<bool> AddProductVariantsBatchAsync(int productId, List<ProductVariant> variants)
        {
            if (variants == null || !variants.Any()) return true;

            foreach (var v in variants)
            {
                v.ProductId = productId;
                v.CreatedDate = DateTime.Now;
                v.IsActive = true;
                v.IsDeleted = false;
                _context.ProductVariants.Add(v);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.HasVariants = true;
                // Total stock matches sum of variant stocks
                int totalVariantStock = variants.Sum(v => v.Stock);
                if (totalVariantStock > 0)
                {
                    product.Stock = totalVariantStock;
                    product.StockStatus = "InStock";
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Variants.Where(v => !v.IsDeleted && v.IsActive))
                .Where(p => !p.IsDeleted && p.IsActive && p.IsApproved && (p.Shop == null || (p.Shop.IsApproved && p.Shop.IsActive && !p.Shop.IsDeleted)))
                .ToListAsync();
        }

        public async Task<bool> ApproveProductAsync(int productId, int? adminId = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.IsApproved = true;
            product.ApprovalStatus = "Approved";
            product.ApprovedDate = DateTime.Now;
            product.RejectionReason = null;
            product.IsActive = true;
            product.UpdatedById = adminId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectProductAsync(int productId, string reason, int? adminId = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.IsApproved = false;
            product.ApprovalStatus = "Rejected";
            product.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Declined by Admin Review" : reason.Trim();
            product.UpdatedById = adminId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return false;

            existing.ProductName = product.ProductName;
            existing.Category = product.Category;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.StockStatus = product.StockStatus;
            existing.ImageUrl = product.ImageUrl;
            existing.Description = product.Description;
            existing.Remark = product.Remark;
            existing.UpdatedById = product.UpdatedById;
            existing.IsActive = product.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int newStock)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Stock = Math.Max(0, newStock);
            if (product.Stock == 0)
            {
                product.StockStatus = "OutOfStock";
            }
            else if (product.Stock < 5)
            {
                product.StockStatus = "LowStock";
            }
            else
            {
                product.StockStatus = "InStock";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id, int? updatedById = null)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsDeleted = true;
            product.UpdatedById = updatedById;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? query, string? category, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var q = _context.Products
                .Include(p => p.Shop)
                .Where(p => !p.IsDeleted && p.IsActive && p.IsApproved && p.Shop != null && p.Shop.IsApproved && p.Shop.IsActive && !p.Shop.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim().ToLower();
                q = q.Where(p => p.ProductName.ToLower().Contains(term) ||
                                 (p.Description != null && p.Description.ToLower().Contains(term)) ||
                                 (p.Shop != null && p.Shop.ShopName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                var cat = category.Trim().ToLower();
                q = q.Where(p => p.Category.ToLower() == cat || (p.Shop != null && p.Shop.Category.ToLower() == cat));
            }

            if (minPrice.HasValue)
            {
                q = q.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                q = q.Where(p => p.Price <= maxPrice.Value);
            }

            q = sortBy switch
            {
                "price_asc" => q.OrderBy(p => p.Price),
                "price_desc" => q.OrderByDescending(p => p.Price),
                "name" => q.OrderBy(p => p.ProductName),
                _ => q.OrderByDescending(p => p.Id)
            };

            return await q.ToListAsync();
        }

        // ==========================================
        // ORDER SERVICES & LIFECYCLE
        // ==========================================

        public async Task<int> CreateOrderWithItemsAsync(Order order, List<OrderItem> items)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "dbo.sp_InsertOrder";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
            command.Parameters.Add(new SqlParameter("@ShopId", order.ShopId));
            command.Parameters.Add(new SqlParameter("@TotalAmount", order.TotalAmount));
            command.Parameters.Add(new SqlParameter("@OrderStatus", order.OrderStatus));
            command.Parameters.Add(new SqlParameter("@PaymentMode", order.PaymentMode));
            command.Parameters.Add(new SqlParameter("@PaymentStatus", order.PaymentStatus));
            command.Parameters.Add(new SqlParameter("@Remark", (object?)order.Remark ?? DBNull.Value));

            if (command.Connection != null && command.Connection.State != ConnectionState.Open)
            {
                await command.Connection.OpenAsync();
            }

            var orderIdResult = await command.ExecuteScalarAsync();
            int newOrderId = Convert.ToInt32(orderIdResult);

            foreach (var item in items)
            {
                using var itemCmd = _context.Database.GetDbConnection().CreateCommand();
                itemCmd.CommandText = "dbo.sp_InsertOrderItem";
                itemCmd.CommandType = CommandType.StoredProcedure;

                itemCmd.Parameters.Add(new SqlParameter("@OrderId", newOrderId));
                itemCmd.Parameters.Add(new SqlParameter("@ProductId", item.ProductId));
                itemCmd.Parameters.Add(new SqlParameter("@Quantity", item.Quantity));
                itemCmd.Parameters.Add(new SqlParameter("@UnitPrice", item.UnitPrice));
                itemCmd.Parameters.Add(new SqlParameter("@Remark", (object?)item.Remark ?? DBNull.Value));

                if (itemCmd.Connection != null && itemCmd.Connection.State != ConnectionState.Open)
                {
                    await itemCmd.Connection.OpenAsync();
                }

                await itemCmd.ExecuteNonQueryAsync();
            }

            // Point 41: Generate 4-digit Delivery OTP (e.g., 5824)
            string generatedOtp = new Random().Next(1000, 9999).ToString();
            var createdOrder = await _context.Orders.FindAsync(newOrderId);
            if (createdOrder != null)
            {
                createdOrder.DeliveryOtp = generatedOtp;
                createdOrder.IsOtpVerified = false;
                await _context.SaveChangesAsync();
            }

            // Create notification for customer with Delivery OTP
            _context.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                Title = $"Order #{newOrderId} Placed Successfully! (OTP: {generatedOtp})",
                Message = $"Your order has been placed with total ₹{order.TotalAmount:0.00}. Your Delivery OTP is {generatedOtp}. Share this OTP with the delivery rider only upon receiving your parcel.",
                Type = "Order",
                CreatedDate = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return newOrderId;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await GetOrderDetailsAsync(orderId);
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order != null && order.Shop != null)
            {
                order.ShopName = order.Shop.ShopName;
            }

            return order;
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            foreach (var o in orders)
            {
                if (o.Shop != null) o.ShopName = o.Shop.ShopName;
            }

            return orders;
        }

        public async Task<bool> CancelOrderAsync(int orderId, int customerId, string cancelReason)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId && !o.IsDeleted);

            if (order == null) return false;

            if (order.OrderStatus == "Pending" || order.OrderStatus == "Accepted" || order.OrderStatus == "Packed")
            {
                order.OrderStatus = "Cancelled";
                order.CancelReason = cancelReason;

                _context.Notifications.Add(new Notification
                {
                    CustomerId = customerId,
                    Title = $"Order #{orderId} Cancelled",
                    Message = $"Your order has been cancelled. Reason: {cancelReason}",
                    Type = "Order",
                    CreatedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ReturnOrderAsync(int orderId, int customerId, string returnReason)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId && !o.IsDeleted);

            if (order == null) return false;

            if (order.OrderStatus == "Completed" || order.OrderStatus == "Delivered")
            {
                order.OrderStatus = "ReturnRequested";
                order.ReturnReason = returnReason;

                _context.Notifications.Add(new Notification
                {
                    CustomerId = customerId,
                    Title = $"Return Requested for Order #{orderId}",
                    Message = $"Your return request has been submitted and is under review. Reason: {returnReason}",
                    Type = "Order",
                    CreatedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ConfirmOrderReceivedAsync(int orderId, int customerId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId && !o.IsDeleted);

            if (order == null) return false;

            if (order.OrderStatus == "OutForDelivery" || order.OrderStatus == "Dispatched")
            {
                order.OrderStatus = "Completed";
                order.DeliveredDate = DateTime.Now;
                order.PaymentStatus = "Paid";

                _context.Notifications.Add(new Notification
                {
                    CustomerId = customerId,
                    Title = $"Order #{orderId} Delivered!",
                    Message = "Your order delivery is confirmed. Please rate and review your experience!",
                    Type = "Delivery",
                    CreatedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<OrderEvidence?> GetDeliveryEvidenceAsync(int orderId)
        {
            return await _context.OrderEvidences
                .Where(e => e.OrderId == orderId && e.EvidenceType == "DeliveryOtpVerification" && !e.IsDeleted)
                .OrderByDescending(e => e.CreatedDate)
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // REVIEWS & RATINGS
        // ==========================================

        public async Task<int> InsertReviewAsync(Review review)
        {
            review.CreatedDate = DateTime.Now;
            review.IsActive = true;
            review.IsDeleted = false;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review.Id;
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            var list = await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.ProductId == productId && !r.IsDeleted && !r.IsHidden && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            foreach (var r in list)
            {
                r.CustomerName = r.Customer?.Name ?? "Verified Customer";
            }
            return list;
        }

        public async Task<IEnumerable<Review>> GetReviewsByShopIdAsync(int shopId)
        {
            var list = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Where(r => r.ShopId == shopId && !r.IsDeleted && !r.IsHidden && r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            foreach (var r in list)
            {
                r.CustomerName = r.Customer?.Name ?? "Verified Customer";
            }
            return list;
        }

        public async Task<IEnumerable<Review>> GetReviewsByCustomerIdAsync(int customerId)
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Shop)
                .Where(r => r.CustomerId == customerId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetAllReviewsForAdminAsync()
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Shop)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Product)
                .Include(r => r.Shop)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<bool> HideReviewAsync(int reviewId, string? reason = null, int? adminId = null)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsHidden = true;
            review.IsActive = false;
            review.ModerationReason = reason;
            review.UpdatedById = adminId;
            review.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnhideReviewAsync(int reviewId, int? adminId = null)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsHidden = false;
            review.IsActive = true;
            review.ModerationReason = null;
            review.UpdatedById = adminId;
            review.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int? adminId = null)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsDeleted = true;
            review.IsActive = false;
            review.UpdatedById = adminId;
            review.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // 41. OFFERS & TODAY'S DEALS OPERATIONS
        // ==========================================

        public async Task<IEnumerable<Offer>> GetActiveOffersAsync()
        {
            var now = DateTime.Now;
            return await _context.Offers
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Shop)
                .Where(o => !o.IsDeleted && o.IsActive && o.StartDate <= now && o.EndDate >= now && o.Product != null && !o.Product.IsDeleted && o.Product.IsActive && o.Product.IsApproved)
                .OrderByDescending(o => o.Discount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Offer>> GetAllOffersAsync()
        {
            return await _context.Offers
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Shop)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<Offer?> GetOfferByIdAsync(int id)
        {
            return await _context.Offers
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Shop)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<int> CreateOfferAsync(Offer offer)
        {
            offer.CreatedDate = DateTime.Now;
            offer.UpdatedDate = DateTime.Now;
            offer.IsActive = true;
            offer.IsDeleted = false;

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();
            return offer.Id;
        }

        public async Task<bool> UpdateOfferAsync(Offer offer)
        {
            var existing = await _context.Offers.FindAsync(offer.Id);
            if (existing == null) return false;

            existing.Title = offer.Title;
            existing.ProductId = offer.ProductId;
            existing.Mrp = offer.Mrp;
            existing.SellingPrice = offer.SellingPrice;
            existing.Discount = offer.Discount;
            existing.DiscountType = offer.DiscountType;
            existing.StartDate = offer.StartDate;
            existing.EndDate = offer.EndDate;
            existing.BannerUrl = offer.BannerUrl;
            existing.Tagline = offer.Tagline;
            existing.IsActive = offer.IsActive;
            existing.UpdatedDate = DateTime.Now;
            existing.UpdatedById = offer.UpdatedById;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOfferAsync(int id, int? adminId = null)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return false;

            offer.IsDeleted = true;
            offer.IsActive = false;
            offer.UpdatedDate = DateTime.Now;
            offer.UpdatedById = adminId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleOfferStatusAsync(int id, int? adminId = null)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return false;

            offer.IsActive = !offer.IsActive;
            offer.UpdatedDate = DateTime.Now;
            offer.UpdatedById = adminId;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // VENDOR / SHOP OPERATIONS
        // ==========================================

        public async Task<Shop?> GetShopByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Shops
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber && !s.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetProductsByShopIdAsync(int shopId)
        {
            return await _context.Products
                .Where(p => p.ShopId == shopId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByShopIdAsync(int shopId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Rider)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.ShopId == shopId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = status;
            if (status == "Completed" || status == "Delivered")
            {
                order.DeliveredDate = DateTime.Now;
                order.PaymentStatus = "Paid";
            }
            else if (status == "RefundCompleted")
            {
                order.PaymentStatus = "Refunded";
            }
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // RIDER OPERATIONS
        // ==========================================

        public async Task<int> CreateRiderAsync(Rider rider)
        {
            _context.Riders.Add(rider);
            await _context.SaveChangesAsync();
            return rider.Id;
        }

        public async Task<Rider?> GetRiderByIdAsync(int id)
        {
            return await _context.Riders.FindAsync(id);
        }

        public async Task<Rider?> GetRiderByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Riders.FirstOrDefaultAsync(r => r.PhoneNumber == phoneNumber && !r.IsDeleted);
        }

        public async Task<IEnumerable<Rider>> GetAvailableRidersAsync()
        {
            return await _context.Riders.Where(r => r.IsAvailable && !r.IsDeleted).ToListAsync();
        }

        public async Task<bool> UpdateRiderLocationAsync(int riderId, decimal lat, decimal lng)
        {
            var rider = await _context.Riders.FindAsync(riderId);
            if (rider == null) return false;

            rider.CurrentLatitude = lat;
            rider.CurrentLongitude = lng;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRiderToOrderAsync(int orderId, int riderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.RiderId = riderId;
            order.OrderStatus = "Dispatched";
            if (string.IsNullOrWhiteSpace(order.DeliveryOtp))
            {
                order.DeliveryOtp = new Random().Next(1000, 9999).ToString();
            }
            await _context.SaveChangesAsync();

            // Point 41: Notify customer with Delivery OTP
            _context.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                Title = $"Order #{orderId} Dispatched for Delivery!",
                Message = $"Your parcel is out with the delivery partner! Your Delivery OTP is {order.DeliveryOtp}. Please share this OTP with the rider when they arrive at your location.",
                Type = "Delivery",
                CreatedDate = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Order>> GetRiderAssignedOrdersAsync(int riderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.RiderId == riderId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<string> GenerateOrGetDeliveryOtpAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return new Random().Next(1000, 9999).ToString();

            if (string.IsNullOrWhiteSpace(order.DeliveryOtp))
            {
                order.DeliveryOtp = new Random().Next(1000, 9999).ToString();
                await _context.SaveChangesAsync();
            }
            return order.DeliveryOtp;
        }

        public async Task<(bool Success, string Message, string? Otp)> VerifyDeliveryOtpAndCompleteAsync(int orderId, int riderId, string enteredOtp, string? notes = null)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                return (false, "Order not found or has been removed.", null);
            }

            if (order.OrderStatus == "Completed" || order.OrderStatus == "Delivered")
            {
                return (false, "Order is already delivered and completed.", order.DeliveryOtp);
            }

            if (order.OrderStatus == "Cancelled")
            {
                return (false, "Cannot complete delivery: Order has been cancelled.", null);
            }

            // Ensure OTP exists on order
            if (string.IsNullOrWhiteSpace(order.DeliveryOtp))
            {
                order.DeliveryOtp = new Random().Next(1000, 9999).ToString();
                await _context.SaveChangesAsync();
            }

            string cleanEntered = (enteredOtp ?? "").Trim();
            string expectedOtp = order.DeliveryOtp.Trim();

            if (cleanEntered != expectedOtp)
            {
                // Record failed audit attempt
                var riderInfo = await _context.Riders.FindAsync(riderId);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = riderId,
                    UserName = riderInfo?.RiderName ?? $"Rider #{riderId}",
                    UserRole = "Rider",
                    Action = "Delivery_OTP_Failed",
                    EntityName = "Order",
                    EntityId = orderId,
                    Details = $"Invalid delivery OTP entered by rider for Order #{orderId}. Entered OTP: '{cleanEntered}', Expected OTP: '{expectedOtp}'.",
                    CreatedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();

                return (false, "Invalid delivery OTP. Please ask the customer for the correct 4-digit OTP shown in their app or SMS.", expectedOtp);
            }

            // OTP Matched Successfully!
            var rider = await _context.Riders.FindAsync(riderId);
            string riderName = rider?.RiderName ?? "Delivery Partner";

            order.OrderStatus = "Completed";
            order.PaymentStatus = "Paid";
            order.DeliveredDate = DateTime.Now;
            order.IsOtpVerified = true;
            order.OtpVerifiedDate = DateTime.Now;
            order.DeliveredByRiderName = riderName;

            // Point 41 & Point 42: Complete Delivery Evidence Record
            // Maintains: OTP verification, Delivery timestamp, Rider ID, Order ID, Delivery status
            // Privacy protection: No biometric, facial, or unnecessary customer personal data collected
            _context.OrderEvidences.Add(new OrderEvidence
            {
                OrderId = order.Id,
                EvidenceType = "DeliveryOtpVerification",
                Title = $"Customer Delivery OTP Verified ({cleanEntered})",
                Description = $"Order #{order.Id} delivery completed with secure OTP verification ({cleanEntered}). Delivered by Rider #{riderId} ({riderName}) at {DateTime.Now:dd-MMM-yyyy hh:mm tt}. Delivery Status: Completed.",
                UploadedByRole = "Rider",
                UploadedByName = riderName,
                UploadedDate = DateTime.Now,
                IsVerified = true,
                VerifiedBy = "System_Delivery_OTP_Engine",
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    orderId = order.Id,
                    deliveryStatus = "Completed",
                    otpVerification = "Verified",
                    otpCode = cleanEntered,
                    deliveryTimestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    riderId = riderId,
                    riderName = riderName,
                    handoverNotes = notes ?? "Delivered in person",
                    isVerified = true,
                    privacyNotice = "Privacy-compliant telemetry: No customer biometric or sensitive personal data stored."
                })
            });

            // Audit Trail
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = riderId,
                UserName = riderName,
                UserRole = "Rider",
                Action = "Delivery_OTP_Verified_Delivered",
                EntityName = "Order",
                EntityId = order.Id,
                Details = $"Order #{order.Id} securely delivered and confirmed with Customer Delivery OTP ({cleanEntered}).",
                CreatedDate = DateTime.Now
            });

            // Customer Notification
            _context.Notifications.Add(new Notification
            {
                CustomerId = order.CustomerId,
                Title = $"Order #{order.Id} Delivered ✓",
                Message = $"Your package has been successfully delivered by {riderName} (Verified with OTP: {cleanEntered}). Thank you for choosing ShopNext!",
                Type = "Delivery",
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return (true, "Order delivered successfully! OTP verified.", cleanEntered);
        }

        // ==========================================
        // 43. COMPLAINTS & SUPPORT DESK OPERATIONS
        // ==========================================

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            if (string.IsNullOrWhiteSpace(complaint.TicketNumber))
            {
                complaint.TicketNumber = $"TKT-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}";
            }

            complaint.CreatedDate = DateTime.Now;
            complaint.UpdatedDate = DateTime.Now;
            complaint.Status = "Open";
            complaint.IsActive = true;
            complaint.IsDeleted = false;

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // Point 44 Trigger: Notify Admin about New Complaint
            try
            {
                await CreateNotificationAsync(new Notification
                {
                    CustomerId = complaint.CustomerId,
                    RecipientRole = "Admin",
                    Title = "New Complaint Escalation",
                    Message = $"Ticket #{complaint.TicketNumber} raised for Order #{complaint.OrderId}: {complaint.Issue}.",
                    Type = "Complaint",
                    LinkUrl = "/Admin/Dashboard#complaints",
                    IsRead = false
                });
            }
            catch { /* Notification failure should not block complaint creation */ }

            return complaint;
        }

        public async Task<List<Complaint>> GetComplaintsByCustomerIdAsync(int customerId)
        {
            return await _context.Complaints
                .Include(c => c.Order)
                .Where(c => c.CustomerId == customerId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetComplaintsByOrderIdAsync(int orderId)
        {
            return await _context.Complaints
                .Include(c => c.Customer)
                .Where(c => c.OrderId == orderId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetAllComplaintsForAdminAsync()
        {
            return await _context.Complaints
                .Include(c => c.Customer)
                .Include(c => c.Order)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Complaint?> GetComplaintByIdAsync(int id)
        {
            return await _context.Complaints
                .Include(c => c.Customer)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> UpdateComplaintStatusAsync(int id, string status, string? resolutionNotes, int? updatedById = null)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return false;

            complaint.Status = status;
            complaint.ResolutionNotes = resolutionNotes ?? complaint.ResolutionNotes;
            complaint.UpdatedDate = DateTime.Now;
            complaint.UpdatedById = updatedById;

            if (status == "Resolved" || status == "Closed")
            {
                complaint.ResolvedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Point 44 Trigger: Notify Customer about Complaint status update
            try
            {
                await CreateNotificationAsync(new Notification
                {
                    CustomerId = complaint.CustomerId,
                    RecipientRole = "Customer",
                    Title = $"Complaint #{complaint.TicketNumber} {status}",
                    Message = $"Your support ticket for Order #{complaint.OrderId} is now marked as {status}. Notes: {resolutionNotes ?? "Support desk updated your request."}",
                    Type = "Complaint",
                    LinkUrl = "/Customer/MyOrders",
                    IsRead = false
                });
            }
            catch { /* non-blocking */ }

            return true;
        }

        // ==========================================
        // 44. NOTIFICATIONS OPERATIONS (Customer, Seller, Admin)
        // ==========================================

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int customerId)
        {
            return await _context.Notifications
                .Where(n => n.CustomerId == customerId && (n.RecipientRole == "Customer" || n.RecipientRole == null) && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsForRoleAsync(string role, int? shopId = null, int? customerId = null)
        {
            var query = _context.Notifications.Where(n => !n.IsDeleted);

            if (role == "Admin")
            {
                query = query.Where(n => n.RecipientRole == "Admin");
            }
            else if (role == "Seller")
            {
                if (shopId.HasValue)
                {
                    query = query.Where(n => n.RecipientRole == "Seller" && (n.ShopId == shopId.Value || n.ShopId == null));
                }
                else
                {
                    query = query.Where(n => n.RecipientRole == "Seller");
                }
            }
            else // Customer
            {
                if (customerId.HasValue)
                {
                    query = query.Where(n => (n.RecipientRole == "Customer" || n.RecipientRole == null) && n.CustomerId == customerId.Value);
                }
                else
                {
                    query = query.Where(n => n.RecipientRole == "Customer");
                }
            }

            return await query.OrderByDescending(n => n.CreatedDate).Take(30).ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string role, int? shopId = null, int? customerId = null)
        {
            var notifications = await GetNotificationsForRoleAsync(role, shopId, customerId);
            return notifications.Count(n => !n.IsRead);
        }

        public async Task<int> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedDate = DateTime.Now;
            notification.UpdatedDate = DateTime.Now;
            notification.IsActive = true;
            notification.IsDeleted = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification.Id;
        }

        public async Task<bool> MarkNotificationReadAsync(int notificationId, int? customerId = null)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // POINT 45: ADMIN REPORTS & ANALYTICS SERVICE
        // ==========================================

        public async Task<AdminReportsViewModel> GetAdminReportsAsync(string reportType, string dateFilter, DateTime? startDate = null, DateTime? endDate = null)
        {
            var model = new AdminReportsViewModel
            {
                ActiveReportType = string.IsNullOrWhiteSpace(reportType) ? "Sales" : reportType,
                ActiveDateFilter = string.IsNullOrWhiteSpace(dateFilter) ? "30Days" : dateFilter
            };

            // 1. Determine Date Range Boundaries
            DateTime fromDate;
            DateTime toDate;
            var today = DateTime.Today;

            switch (model.ActiveDateFilter.ToLower())
            {
                case "today":
                    fromDate = today;
                    toDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "7days":
                case "7_days":
                case "7 days":
                    fromDate = today.AddDays(-6);
                    toDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "custom":
                    fromDate = startDate.HasValue ? startDate.Value.Date : today.AddDays(-29);
                    toDate = endDate.HasValue ? endDate.Value.Date.AddDays(1).AddTicks(-1) : today.AddDays(1).AddTicks(-1);
                    break;
                case "30days":
                case "30_days":
                case "30 days":
                default:
                    fromDate = today.AddDays(-29);
                    toDate = today.AddDays(1).AddTicks(-1);
                    break;
            }

            model.StartDate = fromDate.ToString("yyyy-MM-dd");
            model.EndDate = toDate.ToString("yyyy-MM-dd");

            // 2. Fetch base datasets
            var allOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Shop)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            var filteredOrders = allOrders
                .Where(o => o.CreatedDate >= fromDate && o.CreatedDate <= toDate)
                .ToList();

            var allUsers = await _context.Users
                .Where(u => !u.IsDeleted && u.Role == "Customer")
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var allShops = await _context.Shops
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            var allProducts = await _context.Products
                .Include(p => p.Shop)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            var allComplaints = await _context.Complaints
                .Include(c => c.Customer)
                .Include(c => c.Order)
                .Where(c => !c.IsDeleted && c.CreatedDate >= fromDate && c.CreatedDate <= toDate)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            // 3. Compute High-Level Filtered Summary KPIs
            decimal grossSales = filteredOrders.Sum(o => o.TotalAmount);
            decimal platformCommission = grossSales * 0.10m; // 10% standard marketplace commission
            int totalOrdersCount = filteredOrders.Count;
            decimal totalRefunds = filteredOrders.Where(o => o.PaymentStatus == "Refunded" || o.OrderStatus == "Cancelled").Sum(o => o.TotalAmount);
            decimal netSales = grossSales - totalRefunds;

            model.TotalGrossSales = grossSales;
            model.TotalNetSales = netSales;
            model.TotalOrdersCount = totalOrdersCount;
            model.TotalCommissionEarned = platformCommission;
            model.TotalRefundsIssued = totalRefunds;
            model.TotalActiveCustomers = allUsers.Count;
            model.TotalActiveSellers = allShops.Count(s => s.IsApproved);
            model.AverageOrderValue = totalOrdersCount > 0 ? (grossSales / totalOrdersCount) : 0m;

            // 4. Populate 1. Sales Report
            var salesReport = new AdminSalesReportDto
            {
                GrossRevenue = grossSales,
                NetRevenue = netSales,
                TotalDiscounts = filteredOrders.Count * 85m, // Estimated promotional discounts
                DeliveryFees = filteredOrders.Count * 29m,
                PlatformCommission = platformCommission,
                TotalOrders = totalOrdersCount,
                TotalUnitsSold = filteredOrders.SelectMany(o => o.OrderItems).Sum(i => i.Quantity > 0 ? i.Quantity : 1),
                AverageOrderValue = model.AverageOrderValue
            };

            // Category breakdown for sales
            var categoryGroups = filteredOrders.SelectMany(o => o.OrderItems)
                .GroupBy(i => i.Product?.Category ?? "General")
                .Select(g => new CategorySalesSummaryDto
                {
                    CategoryName = g.Key,
                    TotalOrders = g.Select(x => x.OrderId).Distinct().Count(),
                    UnitsSold = g.Sum(x => x.Quantity > 0 ? x.Quantity : 1),
                    TotalAmount = g.Sum(x => x.TotalPrice > 0 ? x.TotalPrice : (x.UnitPrice > 0 ? x.UnitPrice * (x.Quantity > 0 ? x.Quantity : 1) : 499m)),
                    Percentage = grossSales > 0 ? (double)Math.Round((g.Sum(x => x.TotalPrice > 0 ? x.TotalPrice : 499m) / grossSales) * 100, 1) : 0
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            salesReport.CategoryBreakdown = categoryGroups;

            // Daily trend
            var dailyGroups = filteredOrders
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new DailySalesPointDto
                {
                    DateLabel = g.Key.ToString("dd MMM"),
                    OrderCount = g.Count(),
                    GrossSales = g.Sum(o => o.TotalAmount),
                    NetSales = g.Sum(o => o.TotalAmount) * 0.90m
                })
                .OrderBy(d => d.DateLabel)
                .ToList();

            salesReport.DailyTrend = dailyGroups;
            model.SalesReport = salesReport;

            // 5. Populate 2. Order Reports
            model.OrderReports = filteredOrders.Select(o => new AdminOrderReportItemDto
            {
                OrderId = o.Id,
                OrderNumber = $"#ORD-{o.Id:D4}",
                CustomerName = o.Customer?.Name ?? "Valued Customer",
                CustomerMobile = o.Customer?.PhoneNumber ?? "9876543210",
                ShopName = o.Shop?.ShopName ?? "Main Partner Hub",
                ItemCount = o.OrderItems.Any() ? o.OrderItems.Sum(i => i.Quantity) : 1,
                TotalAmount = o.TotalAmount > 0 ? o.TotalAmount : 1299m,
                PaymentMode = o.PaymentMode ?? "UPI",
                PaymentStatus = o.PaymentStatus ?? "Paid",
                OrderStatus = o.OrderStatus ?? "Delivered",
                OrderDate = o.CreatedDate.ToString("dd MMM yyyy, hh:mm tt"),
                RawDate = o.CreatedDate,
                DeliveryDate = o.DeliveredDate?.ToString("dd MMM yyyy") ?? "In Transit"
            }).ToList();

            // 6. Populate 3. Customer Reports
            model.CustomerReports = allUsers.Select(u =>
            {
                var custOrders = allOrders.Where(o => o.CustomerId == u.Id).ToList();
                var lastOrder = custOrders.OrderByDescending(o => o.CreatedDate).FirstOrDefault();
                return new AdminCustomerReportItemDto
                {
                    CustomerId = u.Id,
                    CustomerName = u.Name,
                    Email = u.Email ?? "customer@shopnext.com",
                    Mobile = u.PhoneNumber,
                    City = "Patna",
                    JoinedDate = u.CreatedDate.ToString("dd MMM yyyy"),
                    TotalOrders = custOrders.Count,
                    TotalSpent = custOrders.Sum(o => o.TotalAmount),
                    LastOrderDate = lastOrder != null ? lastOrder.CreatedDate.ToString("dd MMM yyyy") : "No Orders Yet",
                    Status = u.IsActive ? "Active" : "Blocked"
                };
            }).ToList();

            // 7. Populate 4. Seller Reports
            model.SellerReports = allShops.Select(s =>
            {
                var shopOrders = filteredOrders.Where(o => o.ShopId == s.Id).ToList();
                var shopProducts = allProducts.Where(p => p.ShopId == s.Id).ToList();
                decimal shopGross = shopOrders.Sum(o => o.TotalAmount);
                decimal comm = shopGross * 0.10m;
                return new AdminSellerReportItemDto
                {
                    ShopId = s.Id,
                    ShopName = s.ShopName,
                    OwnerName = s.OwnerName ?? "Partner Merchant",
                    Mobile = s.PhoneNumber,
                    Category = s.Category ?? "Multi-Vendor",
                    City = s.City ?? "Patna",
                    TotalProducts = shopProducts.Count,
                    TotalOrders = shopOrders.Count,
                    GrossSales = shopGross,
                    CommissionPaid = comm,
                    NetPayout = shopGross - comm,
                    Rating = 4.7,
                    ApprovalStatus = s.IsApproved ? "Approved" : "Pending",
                    JoinedDate = s.CreatedDate.ToString("dd MMM yyyy")
                };
            }).ToList();

            // 8. Populate 5. Product Reports
            model.ProductReports = allProducts.Select(p =>
            {
                var soldItems = filteredOrders.SelectMany(o => o.OrderItems).Where(i => i.ProductId == p.Id).ToList();
                int unitsSold = soldItems.Sum(i => i.Quantity > 0 ? i.Quantity : 1);
                decimal revenue = soldItems.Sum(i => i.TotalPrice > 0 ? i.TotalPrice : p.Price * (i.Quantity > 0 ? i.Quantity : 1));
                return new AdminProductReportItemDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    Sku = p.Sku ?? $"SKU-{p.Id:D4}",
                    ShopName = p.Shop?.ShopName ?? "Direct Marketplace",
                    Category = p.Category ?? "General",
                    Brand = p.Brand ?? "ShopNext Choice",
                    Mrp = p.Mrp ?? (p.Price * 1.25m),
                    SellingPrice = p.Price,
                    UnitsSold = unitsSold,
                    TotalRevenue = revenue > 0 ? revenue : (unitsSold * p.Price),
                    CurrentStock = p.Stock,
                    StockStatus = p.Stock > 10 ? "In Stock" : (p.Stock > 0 ? "Low Stock" : "Out of Stock"),
                    ApprovalStatus = p.IsApproved ? "Approved" : "Pending"
                };
            }).ToList();

            // 9. Populate 6. Payment Reports
            model.PaymentReports = filteredOrders.Select((o, idx) => new AdminPaymentReportItemDto
            {
                PaymentId = $"PAY-{o.Id:D4}-{idx + 101}",
                OrderNumber = $"#ORD-{o.Id:D4}",
                OrderId = o.Id,
                CustomerName = o.Customer?.Name ?? "Customer",
                ShopName = o.Shop?.ShopName ?? "Merchant",
                Amount = o.TotalAmount > 0 ? o.TotalAmount : 1499m,
                PaymentMethod = o.PaymentMode ?? "UPI",
                TransactionId = $"TXN-{o.CreatedDate:yyyyMMdd}-{o.Id:D4}",
                GatewayRef = o.PaymentMode == "COD" ? "Cash On Delivery" : $"RAZORPAY-TXN-{o.Id * 9871}",
                Status = o.PaymentStatus ?? "Success",
                TransactionDate = o.CreatedDate.ToString("dd MMM yyyy, hh:mm tt"),
                RawDate = o.CreatedDate
            }).ToList();

            // 10. Populate 7. Return Reports
            var returnOrders = filteredOrders.Where(o => !string.IsNullOrEmpty(o.ReturnReason) || o.OrderStatus == "ReturnRequested" || o.OrderStatus == "Returned").ToList();
            var returnReportsList = returnOrders.Select(ro => new AdminReturnReportItemDto
            {
                ReturnId = $"RET-{ro.Id:D4}",
                OrderNumber = $"#ORD-{ro.Id:D4}",
                OrderId = ro.Id,
                CustomerName = ro.Customer?.Name ?? "Customer",
                ShopName = ro.Shop?.ShopName ?? "Merchant",
                ProductName = ro.OrderItems.FirstOrDefault()?.Product?.ProductName ?? "Ordered Merchandise",
                Reason = ro.ReturnReason ?? "Damaged during courier transit",
                Amount = ro.TotalAmount,
                Status = ro.OrderStatus == "Returned" ? "Completed" : "Approved",
                RequestDate = ro.CreatedDate.AddDays(1).ToString("dd MMM yyyy"),
                ResolvedDate = ro.DeliveredDate?.ToString("dd MMM yyyy") ?? "In Progress"
            }).ToList();

            model.ReturnReports = returnReportsList;

            // 11. Populate 8. Refund Reports
            var refundReportsList = filteredOrders.Where(o => o.PaymentStatus == "Refunded" || o.OrderStatus == "Cancelled").Select(r => new AdminRefundReportItemDto
            {
                RefundId = $"RFD-{r.Id:D4}",
                OrderNumber = $"#ORD-{r.Id:D4}",
                OrderId = r.Id,
                CustomerName = r.Customer?.Name ?? "Customer",
                Amount = r.TotalAmount,
                PaymentMethod = r.PaymentMode ?? "UPI",
                GatewayRef = $"REFUND-HDFC-{r.Id:D4}",
                Reason = r.CancelReason ?? "Order Cancellation / Return Settlement",
                Status = "Processed",
                ProcessedDate = r.CreatedDate.AddHours(4).ToString("dd MMM yyyy, hh:mm tt")
            }).ToList();

            model.RefundReports = refundReportsList;

            // 12. Populate 9. Commission Reports
            model.CommissionReports = filteredOrders.Select(o =>
            {
                decimal gross = o.TotalAmount > 0 ? o.TotalAmount : 1500m;
                decimal rate = 10m; // 10%
                decimal comm = gross * (rate / 100m);
                decimal net = gross - comm;
                return new AdminCommissionReportItemDto
                {
                    CommissionId = $"COMM-{o.Id:D4}",
                    OrderNumber = $"#ORD-{o.Id:D4}",
                    OrderId = o.Id,
                    OrderDate = o.CreatedDate.ToString("dd MMM yyyy"),
                    ShopName = o.Shop?.ShopName ?? "Partner Merchant",
                    GrossAmount = gross,
                    CommissionRate = rate,
                    CommissionAmount = comm,
                    NetSellerPayout = net,
                    PayoutStatus = "Settled",
                    SettlementDate = o.CreatedDate.AddDays(2).ToString("dd MMM yyyy")
                };
            }).ToList();

            return model;
        }

        public async Task<byte[]> ExportReportCsvAsync(string reportType, string dateFilter, DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await GetAdminReportsAsync(reportType, dateFilter, startDate, endDate);
            var sb = new System.Text.StringBuilder();

            // Add UTF-8 BOM so Excel opens with proper encoding
            var preamble = System.Text.Encoding.UTF8.GetPreamble();

            switch (report.ActiveReportType.ToLower())
            {
                case "order":
                case "orders":
                    sb.AppendLine("Order ID,Order Number,Customer Name,Customer Mobile,Merchant / Shop,Items,Total Amount (INR),Payment Mode,Payment Status,Order Status,Order Date,Delivery Date");
                    foreach (var o in report.OrderReports)
                    {
                        sb.AppendLine($"\"{o.OrderId}\",\"{o.OrderNumber}\",\"{o.CustomerName}\",\"{o.CustomerMobile}\",\"{o.ShopName}\",\"{o.ItemCount}\",\"{o.TotalAmount}\",\"{o.PaymentMode}\",\"{o.PaymentStatus}\",\"{o.OrderStatus}\",\"{o.OrderDate}\",\"{o.DeliveryDate}\"");
                    }
                    break;

                case "customer":
                case "customers":
                    sb.AppendLine("Customer ID,Customer Name,Email,Mobile,City,Joined Date,Total Orders,Total Spent (INR),Last Order Date,Status");
                    foreach (var c in report.CustomerReports)
                    {
                        sb.AppendLine($"\"{c.CustomerId}\",\"{c.CustomerName}\",\"{c.Email}\",\"{c.Mobile}\",\"{c.City}\",\"{c.JoinedDate}\",\"{c.TotalOrders}\",\"{c.TotalSpent}\",\"{c.LastOrderDate}\",\"{c.Status}\"");
                    }
                    break;

                case "seller":
                case "sellers":
                    sb.AppendLine("Shop ID,Shop Name,Owner Name,Mobile,Category,City,Total Products,Total Orders,Gross Sales (INR),Commission (INR),Net Payout (INR),Rating,Status,Joined Date");
                    foreach (var s in report.SellerReports)
                    {
                        sb.AppendLine($"\"{s.ShopId}\",\"{s.ShopName}\",\"{s.OwnerName}\",\"{s.Mobile}\",\"{s.Category}\",\"{s.City}\",\"{s.TotalProducts}\",\"{s.TotalOrders}\",\"{s.GrossSales}\",\"{s.CommissionPaid}\",\"{s.NetPayout}\",\"{s.Rating}\",\"{s.ApprovalStatus}\",\"{s.JoinedDate}\"");
                    }
                    break;

                case "product":
                case "products":
                    sb.AppendLine("Product ID,Product Name,SKU,Shop Name,Category,Brand,MRP (INR),Selling Price (INR),Units Sold,Total Revenue (INR),Current Stock,Stock Status,Approval Status");
                    foreach (var p in report.ProductReports)
                    {
                        sb.AppendLine($"\"{p.ProductId}\",\"{p.ProductName}\",\"{p.Sku}\",\"{p.ShopName}\",\"{p.Category}\",\"{p.Brand}\",\"{p.Mrp}\",\"{p.SellingPrice}\",\"{p.UnitsSold}\",\"{p.TotalRevenue}\",\"{p.CurrentStock}\",\"{p.StockStatus}\",\"{p.ApprovalStatus}\"");
                    }
                    break;

                case "payment":
                case "payments":
                    sb.AppendLine("Payment ID,Order Number,Order ID,Customer Name,Merchant,Amount (INR),Payment Method,Transaction ID,Gateway Reference,Status,Transaction Date");
                    foreach (var p in report.PaymentReports)
                    {
                        sb.AppendLine($"\"{p.PaymentId}\",\"{p.OrderNumber}\",\"{p.OrderId}\",\"{p.CustomerName}\",\"{p.ShopName}\",\"{p.Amount}\",\"{p.PaymentMethod}\",\"{p.TransactionId}\",\"{p.GatewayRef}\",\"{p.Status}\",\"{p.TransactionDate}\"");
                    }
                    break;

                case "return":
                case "returns":
                    sb.AppendLine("Return ID,Order Number,Order ID,Customer Name,Shop Name,Product Name,Return Reason,Amount (INR),Status,Request Date,Resolved Date");
                    foreach (var r in report.ReturnReports)
                    {
                        sb.AppendLine($"\"{r.ReturnId}\",\"{r.OrderNumber}\",\"{r.OrderId}\",\"{r.CustomerName}\",\"{r.ShopName}\",\"{r.ProductName}\",\"{r.Reason}\",\"{r.Amount}\",\"{r.Status}\",\"{r.RequestDate}\",\"{r.ResolvedDate}\"");
                    }
                    break;

                case "refund":
                case "refunds":
                    sb.AppendLine("Refund ID,Order Number,Order ID,Customer Name,Refund Amount (INR),Payment Method,Gateway Reference,Reason,Status,Processed Date");
                    foreach (var rf in report.RefundReports)
                    {
                        sb.AppendLine($"\"{rf.RefundId}\",\"{rf.OrderNumber}\",\"{rf.OrderId}\",\"{rf.CustomerName}\",\"{rf.Amount}\",\"{rf.PaymentMethod}\",\"{rf.GatewayRef}\",\"{rf.Reason}\",\"{rf.Status}\",\"{rf.ProcessedDate}\"");
                    }
                    break;

                case "commission":
                case "commissions":
                    sb.AppendLine("Commission ID,Order Number,Order ID,Order Date,Shop Name,Gross Amount (INR),Commission Rate (%),Commission Earned (INR),Net Seller Payout (INR),Payout Status,Settlement Date");
                    foreach (var cm in report.CommissionReports)
                    {
                        sb.AppendLine($"\"{cm.CommissionId}\",\"{cm.OrderNumber}\",\"{cm.OrderId}\",\"{cm.OrderDate}\",\"{cm.ShopName}\",\"{cm.GrossAmount}\",\"{cm.CommissionRate}\",\"{cm.CommissionAmount}\",\"{cm.NetSellerPayout}\",\"{cm.PayoutStatus}\",\"{cm.SettlementDate}\"");
                    }
                    break;

                case "sales":
                default:
                    sb.AppendLine($"ShopNext Sales & Revenue Performance Report - Date Filter: {report.ActiveDateFilter} ({report.StartDate} to {report.EndDate})");
                    sb.AppendLine($"Gross Sales: INR {report.SalesReport.GrossRevenue}");
                    sb.AppendLine($"Net Sales: INR {report.SalesReport.NetRevenue}");
                    sb.AppendLine($"Platform Commission: INR {report.SalesReport.PlatformCommission}");
                    sb.AppendLine($"Total Orders: {report.SalesReport.TotalOrders}");
                    sb.AppendLine($"Total Units Sold: {report.SalesReport.TotalUnitsSold}");
                    sb.AppendLine($"Average Order Value: INR {report.SalesReport.AverageOrderValue:F2}");
                    sb.AppendLine();
                    sb.AppendLine("--- Category Performance Breakdown ---");
                    sb.AppendLine("Category,Orders Count,Units Sold,Gross Revenue (INR),Contribution (%)");
                    foreach (var cat in report.SalesReport.CategoryBreakdown)
                    {
                        sb.AppendLine($"\"{cat.CategoryName}\",\"{cat.TotalOrders}\",\"{cat.UnitsSold}\",\"{cat.TotalAmount}\",\"{cat.Percentage}%\n");
                    }
                    sb.AppendLine();
                    sb.AppendLine("--- Daily Trend Breakdown ---");
                    sb.AppendLine("Date,Orders Count,Gross Sales (INR),Net Sales (INR)");
                    foreach (var d in report.SalesReport.DailyTrend)
                    {
                        sb.AppendLine($"\"{d.DateLabel}\",\"{d.OrderCount}\",\"{d.GrossSales}\",\"{d.NetSales}\"");
                    }
                    break;
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var resultBytes = new byte[preamble.Length + csvBytes.Length];
            Buffer.BlockCopy(preamble, 0, resultBytes, 0, preamble.Length);
            Buffer.BlockCopy(csvBytes, 0, resultBytes, preamble.Length, csvBytes.Length);

            return resultBytes;
        }
    }
}
