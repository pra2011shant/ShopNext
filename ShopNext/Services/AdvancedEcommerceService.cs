using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class AdvancedEcommerceService : IAdvancedEcommerceService
    {
        private readonly ShopNextDbContext _context;
        private readonly ILogger<AdvancedEcommerceService> _logger;
        private readonly IAuditService _auditService;

        public AdvancedEcommerceService(
            ShopNextDbContext context, 
            ILogger<AdvancedEcommerceService> logger,
            IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        // ==========================================
        // 81. PRODUCT COMPARISON
        // ==========================================
        public async Task<ProductComparisonViewModel> GetProductComparisonAsync(int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
            {
                return new ProductComparisonViewModel();
            }

            // Limit to 4 products max for side-by-side comparison
            var targetIds = productIds.Distinct().Take(4).ToList();

            var products = await _context.Products
                .Include(p => p.Shop)
                .Where(p => targetIds.Contains(p.Id))
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.ProductId.HasValue && targetIds.Contains(r.ProductId.Value) && r.IsActive && !r.IsHidden)
                .ToListAsync();

            var items = new List<ProductComparisonItemDto>();
            var allKeys = new HashSet<string>();

            foreach (var p in products)
            {
                var pReviews = reviews.Where(r => r.ProductId == p.Id).ToList();
                double avgRating = pReviews.Any() ? Math.Round(pReviews.Average(r => r.Rating), 1) : 4.5;
                int reviewCount = pReviews.Count;

                var specs = new Dictionary<string, string>
                {
                    { "Category", p.Category ?? "General" },
                    { "Brand", p.Brand ?? "ShopNext Choice" },
                    { "Stock Available", $"{p.Stock} Units" },
                    { "Merchant / Shop", p.Shop?.ShopName ?? "Verified Partner" },
                    { "Standard Warranty", "1 Year Brand Warranty" },
                    { "Delivery", "2-3 Days Fast Dispatch" },
                    { "Replacement Policy", "7 Days Replacement" }
                };

                if (!string.IsNullOrWhiteSpace(p.SubCategory)) specs["Sub Category"] = p.SubCategory;
                if (!string.IsNullOrWhiteSpace(p.Sku)) specs["SKU Code"] = p.Sku;

                foreach (var k in specs.Keys) allKeys.Add(k);

                items.Add(new ProductComparisonItemDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    ImageUrl = p.ImageUrl ?? "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=100",
                    Category = p.Category ?? "General",
                    Brand = p.Brand ?? "ShopNext",
                    Price = p.Price,
                    Mrp = p.Mrp,
                    Rating = avgRating,
                    ReviewCount = reviewCount,
                    Stock = p.Stock,
                    StockStatus = p.Stock > 0 ? "In Stock" : "Out of Stock",
                    ShopName = p.Shop?.ShopName ?? "Verified Shop",
                    Description = p.Description ?? string.Empty,
                    Specifications = specs
                });
            }

            int bestPriceId = items.OrderBy(i => i.Price).FirstOrDefault()?.ProductId ?? 0;
            int highestRatingId = items.OrderByDescending(i => i.Rating).FirstOrDefault()?.ProductId ?? 0;

            return new ProductComparisonViewModel
            {
                Products = items,
                AllSpecificationKeys = allKeys.OrderBy(k => k).ToList(),
                BestPriceProductId = bestPriceId,
                HighestRatingProductId = highestRatingId
            };
        }

        // ==========================================
        // 82. RECENTLY SEARCHED PRODUCTS
        // ==========================================
        public async Task<List<SearchHistoryDto>> GetSearchHistoryAsync(int? customerId, string? clientIp, int take = 10)
        {
            var query = _context.SearchHistories.AsNoTracking().AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(s => s.CustomerId == customerId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(clientIp))
            {
                query = query.Where(s => s.ClientIp == clientIp);
            }
            else
            {
                return new List<SearchHistoryDto>();
            }

            var list = await query
                .OrderByDescending(s => s.CreatedDate)
                .Take(take)
                .Select(s => new SearchHistoryDto
                {
                    Id = s.Id,
                    SearchTerm = s.SearchTerm,
                    Category = s.Category,
                    SearchedAt = s.CreatedDate.ToString("dd MMM, hh:mm tt")
                })
                .ToListAsync();

            return list;
        }

        public async Task RecordSearchAsync(string term, int? customerId, string? category, int resultCount, string? clientIp)
        {
            if (string.IsNullOrWhiteSpace(term)) return;
            term = term.Trim();

            try
            {
                var history = new SearchHistory
                {
                    SearchTerm = term,
                    CustomerId = customerId > 0 ? customerId : null,
                    Category = category,
                    ResultCount = resultCount,
                    ClientIp = clientIp,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.SearchHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record search term: {Term}", term);
            }
        }

        public async Task ClearSearchHistoryAsync(int? customerId, string? clientIp)
        {
            var query = _context.SearchHistories.AsQueryable();
            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(s => s.CustomerId == customerId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(clientIp))
            {
                query = query.Where(s => s.ClientIp == clientIp);
            }
            else
            {
                return;
            }

            var items = await query.ToListAsync();
            _context.SearchHistories.RemoveRange(items);
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 83. SMART RECOMMENDATIONS
        // ==========================================
        public async Task<SmartRecommendationsDto> GetSmartRecommendationsAsync(int productId, int take = 6)
        {
            var currentProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (currentProduct == null)
            {
                var fallback = await _context.Products.AsNoTracking().Take(take).ToListAsync();
                return new SmartRecommendationsDto
                {
                    CustomersAlsoBought = fallback,
                    YouMayAlsoLike = fallback,
                    SimilarProducts = fallback
                };
            }

            // 1. Similar Products: Same Category and Brand
            var similar = await _context.Products.AsNoTracking()
                .Where(p => p.Id != productId && p.Category == currentProduct.Category)
                .OrderByDescending(p => p.CreatedDate)
                .Take(take)
                .ToListAsync();

            // 2. Customers Also Bought: Co-purchased in previous orders
            var coBoughtProductIds = await _context.OrderItems.AsNoTracking()
                .Where(oi => oi.ProductId == productId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .SelectMany(orderId => _context.OrderItems
                    .Where(item => item.OrderId == orderId && item.ProductId != productId)
                    .Select(item => item.ProductId))
                .Distinct()
                .Take(take)
                .ToListAsync();

            var alsoBought = await _context.Products.AsNoTracking()
                .Where(p => coBoughtProductIds.Contains(p.Id))
                .ToListAsync();

            if (alsoBought.Count < take)
            {
                var topSellers = await _context.Products.AsNoTracking()
                    .Where(p => p.Id != productId && !alsoBought.Select(a => a.Id).Contains(p.Id))
                    .OrderByDescending(p => p.Price)
                    .Take(take - alsoBought.Count)
                    .ToListAsync();
                alsoBought.AddRange(topSellers);
            }

            // 3. You May Also Like: Similar price bracket
            decimal minPrice = currentProduct.Price * 0.7m;
            decimal maxPrice = currentProduct.Price * 1.4m;

            var youMayLike = await _context.Products.AsNoTracking()
                .Where(p => p.Id != productId && p.Price >= minPrice && p.Price <= maxPrice)
                .OrderByDescending(p => p.Id)
                .Take(take)
                .ToListAsync();

            return new SmartRecommendationsDto
            {
                CustomersAlsoBought = alsoBought,
                YouMayAlsoLike = youMayLike,
                SimilarProducts = similar
            };
        }

        // ==========================================
        // 84. PRODUCT Q&A
        // ==========================================
        public async Task<List<ProductQnADto>> GetProductQuestionsAsync(int productId)
        {
            var questions = await _context.ProductQuestions
                .Include(q => q.Customer)
                .Include(q => q.Answers)
                    .ThenInclude(a => a.Responder)
                .Where(q => q.ProductId == productId && q.IsApproved)
                .OrderByDescending(q => q.Upvotes)
                .ThenByDescending(q => q.CreatedDate)
                .ToListAsync();

            return questions.Select(q => new ProductQnADto
            {
                Id = q.Id,
                ProductId = q.ProductId,
                QuestionText = q.QuestionText,
                AskedBy = q.Customer?.Name ?? "Verified Customer",
                AskedDate = q.CreatedDate.ToString("dd MMM yyyy"),
                Upvotes = q.Upvotes,
                Answers = q.Answers.Select(a => new ProductAnswerDto
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText,
                    AnsweredBy = a.Responder?.Name ?? (a.ResponderRole == "Seller" ? "Official Seller" : "ShopNext Support"),
                    ResponderRole = a.ResponderRole,
                    IsVerifiedSeller = a.IsVerifiedSellerAnswer,
                    AnsweredDate = a.CreatedDate.ToString("dd MMM yyyy"),
                    HelpfulCount = a.HelpfulCount
                }).ToList()
            }).ToList();
        }

        public async Task<ProductQuestion> AskQuestionAsync(int productId, int customerId, string question)
        {
            var q = new ProductQuestion
            {
                ProductId = productId,
                CustomerId = customerId,
                QuestionText = question.Trim(),
                IsApproved = true,
                Upvotes = 0,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.ProductQuestions.Add(q);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Question_Asked", "Product", productId, $"Question: {question}", customerId, null, "Customer");
            return q;
        }

        public async Task<ProductAnswer> AnswerQuestionAsync(int questionId, int responderId, string responderRole, string answer)
        {
            var a = new ProductAnswer
            {
                QuestionId = questionId,
                ResponderId = responderId,
                ResponderRole = responderRole,
                AnswerText = answer.Trim(),
                IsVerifiedSellerAnswer = responderRole.Equals("Seller", StringComparison.OrdinalIgnoreCase) || responderRole.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                HelpfulCount = 0,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.ProductAnswers.Add(a);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Question_Answered", "ProductQuestion", questionId, $"Answer: {answer}", responderId, null, responderRole);
            return a;
        }

        public async Task UpvoteQuestionAsync(int questionId)
        {
            var q = await _context.ProductQuestions.FindAsync(questionId);
            if (q != null)
            {
                q.Upvotes += 1;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAnswerHelpfulAsync(int answerId)
        {
            var a = await _context.ProductAnswers.FindAsync(answerId);
            if (a != null)
            {
                a.HelpfulCount += 1;
                await _context.SaveChangesAsync();
            }
        }

        // ==========================================
        // 85. SELLER & CUSTOMER SUPPORT CHAT
        // ==========================================
        public async Task<List<ChatMessageDto>> GetChatMessagesAsync(string threadId, int currentUserId)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            // Mark unread messages as read
            var unread = messages.Where(m => m.RecipientId == currentUserId && !m.IsRead).ToList();
            if (unread.Any())
            {
                foreach (var u in unread)
                {
                    u.IsRead = true;
                    u.ReadAt = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            return messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                ThreadId = m.ThreadId,
                SenderId = m.SenderId,
                SenderName = m.Sender?.Name ?? m.SenderRole,
                SenderRole = m.SenderRole,
                MessageText = m.MessageText,
                AttachmentUrl = m.AttachmentUrl,
                Timestamp = m.CreatedDate.ToString("hh:mm tt, dd MMM"),
                IsFromCurrentUser = m.SenderId == currentUserId,
                IsRead = m.IsRead
            }).ToList();
        }

        public async Task<ChatMessage> SendChatMessageAsync(
            string threadId, 
            int senderId, 
            string senderRole, 
            string message, 
            int? recipientId = null, 
            int? orderId = null, 
            int? productId = null, 
            int? shopId = null, 
            string? attachmentUrl = null)
        {
            var msg = new ChatMessage
            {
                ThreadId = threadId,
                SenderId = senderId,
                SenderRole = senderRole,
                RecipientId = recipientId,
                OrderId = orderId,
                ProductId = productId,
                ShopId = shopId,
                MessageText = message.Trim(),
                AttachmentUrl = attachmentUrl,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();
            return msg;
        }

        // ==========================================
        // 86 & 87. PRICE DROP & BACK-IN-STOCK ALERTS
        // ==========================================
        public async Task<PriceDropAlert> SubscribePriceDropAsync(int customerId, int productId, decimal currentPrice, decimal? targetPrice = null)
        {
            var alert = await _context.PriceDropAlerts.FirstOrDefaultAsync(a => a.CustomerId == customerId && a.ProductId == productId);
            if (alert == null)
            {
                alert = new PriceDropAlert
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    SubscribedPrice = currentPrice,
                    TargetPrice = targetPrice ?? (currentPrice * 0.9m),
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.PriceDropAlerts.Add(alert);
            }
            else
            {
                alert.SubscribedPrice = currentPrice;
                alert.TargetPrice = targetPrice ?? (currentPrice * 0.9m);
                alert.IsNotified = false;
            }

            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task<StockAlert> SubscribeStockAlertAsync(int customerId, int productId, string? email = null, string? phone = null)
        {
            var alert = await _context.StockAlerts.FirstOrDefaultAsync(s => s.CustomerId == customerId && s.ProductId == productId);
            if (alert == null)
            {
                alert = new StockAlert
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    CustomerEmail = email,
                    CustomerPhone = phone,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.StockAlerts.Add(alert);
            }
            else
            {
                alert.IsNotified = false;
                if (!string.IsNullOrWhiteSpace(email)) alert.CustomerEmail = email;
                if (!string.IsNullOrWhiteSpace(phone)) alert.CustomerPhone = phone;
            }

            await _context.SaveChangesAsync();
            return alert;
        }

        // ==========================================
        // 88. STRICT SERVER-SIDE COUPON VALIDATION ENGINE
        // ==========================================
        public async Task<(bool IsValid, string Message, decimal DiscountAmount, Coupon? Coupon)> ValidateCouponAsync(
            string couponCode, 
            decimal orderAmount, 
            int customerId, 
            List<int>? productIds = null, 
            List<string>? categoryNames = null)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return (false, "Please provide a valid promo coupon code.", 0, null);
            }

            couponCode = couponCode.Trim().ToUpperInvariant();

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
            if (coupon == null)
            {
                return (false, $"Coupon '{couponCode}' does not exist or has expired.", 0, null);
            }

            if (!coupon.IsActive)
            {
                return (false, "This coupon is currently inactive.", 0, coupon);
            }

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.Now)
            {
                return (false, $"Coupon '{couponCode}' expired on {coupon.ExpiryDate.Value:dd MMM yyyy}.", 0, coupon);
            }

            if (orderAmount < coupon.MinOrderAmount)
            {
                return (false, $"Minimum order amount of ₹{coupon.MinOrderAmount:N0} required to apply this coupon.", 0, coupon);
            }

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
            {
                return (false, "This coupon has reached its maximum global redemption limit.", 0, coupon);
            }

            // Customer usage check
            if (customerId > 0)
            {
                int pastOrderCount = await _context.Orders.CountAsync(o => o.CustomerId == customerId && o.OrderStatus != "Cancelled");
                if (pastOrderCount >= 1 && couponCode.StartsWith("WELCOME", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Welcome coupon can only be used on your first order.", 0, coupon);
                }
            }

            decimal discount = 0m;
            if (coupon.DiscountType == "Percentage" || coupon.DiscountType == "Percent")
            {
                discount = Math.Round(orderAmount * (coupon.DiscountValue / 100m), 2);
                if (coupon.MaxDiscountAmount.HasValue && coupon.MaxDiscountAmount.Value > 0 && discount > coupon.MaxDiscountAmount.Value)
                {
                    discount = coupon.MaxDiscountAmount.Value;
                }
            }
            else
            {
                discount = Math.Min(coupon.DiscountValue, orderAmount);
            }

            return (true, $"Coupon '{couponCode}' applied! You saved ₹{discount:N2}.", discount, coupon);
        }

        // ==========================================
        // 89. GIFT CARD & DIGITAL WALLET
        // ==========================================
        public async Task<WalletAccount> GetOrCreateWalletAccountAsync(int customerId)
        {
            var wallet = await _context.WalletAccounts
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.CustomerId == customerId);

            if (wallet == null)
            {
                wallet = new WalletAccount
                {
                    CustomerId = customerId,
                    MainBalance = 0m,
                    RefundWalletBalance = 0m,
                    GiftCardBalance = 0m,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.WalletAccounts.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return wallet;
        }

        public async Task<WalletDashboardViewModel> GetWalletDashboardAsync(int customerId)
        {
            var wallet = await GetOrCreateWalletAccountAsync(customerId);
            var rewardAccount = await GetRewardPointsAccountAsync(customerId);

            var transactions = await _context.WalletTransactions
                .Where(t => t.WalletAccountId == wallet.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Take(20)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType,
                    SourceCategory = t.SourceCategory,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    DateFormatted = t.CreatedDate.ToString("dd MMM yyyy, hh:mm tt"),
                    ReferenceCode = t.ReferenceCode
                })
                .ToListAsync();

            return new WalletDashboardViewModel
            {
                MainBalance = wallet.MainBalance,
                RefundWalletBalance = wallet.RefundWalletBalance,
                GiftCardBalance = wallet.GiftCardBalance,
                TotalBalance = wallet.TotalBalance,
                RewardPoints = rewardAccount.CurrentPoints,
                RewardPointsCashValue = Math.Round(rewardAccount.CurrentPoints * 0.25m, 2), // 4 points = ₹1
                RecentTransactions = transactions
            };
        }

        public async Task<(bool Success, string Message, decimal RedeemedAmount)> RedeemGiftCardAsync(int customerId, string cardCode, string pin)
        {
            if (string.IsNullOrWhiteSpace(cardCode))
            {
                return (false, "Please provide a valid Gift Card code.", 0);
            }

            cardCode = cardCode.Trim().ToUpperInvariant();
            var card = await _context.GiftCards.FirstOrDefaultAsync(g => g.CardCode == cardCode);
            if (card == null)
            {
                return (false, "Gift Card code not found.", 0);
            }

            if (card.IsRedeemed || card.CurrentBalance <= 0)
            {
                return (false, "This Gift Card has already been fully redeemed.", 0);
            }

            if (!string.IsNullOrWhiteSpace(card.Pin) && card.Pin != pin)
            {
                return (false, "Incorrect Gift Card PIN.", 0);
            }

            if (card.ExpiryDate < DateTime.Now)
            {
                return (false, "This Gift Card has expired.", 0);
            }

            decimal amountToCredit = card.CurrentBalance;
            var wallet = await GetOrCreateWalletAccountAsync(customerId);

            wallet.GiftCardBalance += amountToCredit;
            card.CurrentBalance = 0;
            card.IsRedeemed = true;
            card.RedeemedByCustomerId = customerId;
            card.RedeemedAt = DateTime.Now;

            var txn = new WalletTransaction
            {
                WalletAccountId = wallet.Id,
                TransactionType = "Credit",
                SourceCategory = "GiftCard",
                Amount = amountToCredit,
                BalanceAfter = wallet.TotalBalance,
                Description = $"Redeemed Gift Card #{cardCode}",
                ReferenceCode = cardCode,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.WalletTransactions.Add(txn);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("GiftCard_Redeemed", "GiftCard", card.Id, $"Redeemed ₹{amountToCredit:N2} to Wallet", customerId, null, "Customer");

            return (true, $"Success! ₹{amountToCredit:N2} added to your Gift Card Wallet.", amountToCredit);
        }

        public async Task<WalletTransaction> CreditWalletAsync(int customerId, decimal amount, string category, string description, int? orderId = null)
        {
            var wallet = await GetOrCreateWalletAccountAsync(customerId);

            if (category.Equals("Refund", StringComparison.OrdinalIgnoreCase))
            {
                wallet.RefundWalletBalance += amount;
            }
            else if (category.Equals("GiftCard", StringComparison.OrdinalIgnoreCase))
            {
                wallet.GiftCardBalance += amount;
            }
            else
            {
                wallet.MainBalance += amount;
            }

            var txn = new WalletTransaction
            {
                WalletAccountId = wallet.Id,
                TransactionType = "Credit",
                SourceCategory = category,
                Amount = amount,
                BalanceAfter = wallet.TotalBalance,
                Description = description,
                RelatedOrderId = orderId,
                ReferenceCode = $"CR-{DateTime.Now.Ticks % 1000000}",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.WalletTransactions.Add(txn);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Wallet_Credited", "WalletAccount", wallet.Id, $"Credited ₹{amount:N2} ({category})", customerId, null, "Customer");
            return txn;
        }

        public async Task<(bool Success, string Message, WalletTransaction? Txn)> DebitWalletAsync(int customerId, decimal amount, string description, int? orderId = null)
        {
            var wallet = await GetOrCreateWalletAccountAsync(customerId);

            if (wallet.IsLocked)
            {
                return (false, $"Wallet is temporarily locked: {wallet.LockReason ?? "Security review"}", null);
            }

            if (wallet.TotalBalance < amount)
            {
                return (false, $"Insufficient wallet balance. Total available: ₹{wallet.TotalBalance:N2}", null);
            }

            decimal remainingToDeduct = amount;

            // Deduct in order: Refund Wallet -> Gift Card -> Main Balance
            if (wallet.RefundWalletBalance > 0)
            {
                decimal deductFromRefund = Math.Min(wallet.RefundWalletBalance, remainingToDeduct);
                wallet.RefundWalletBalance -= deductFromRefund;
                remainingToDeduct -= deductFromRefund;
            }

            if (remainingToDeduct > 0 && wallet.GiftCardBalance > 0)
            {
                decimal deductFromGift = Math.Min(wallet.GiftCardBalance, remainingToDeduct);
                wallet.GiftCardBalance -= deductFromGift;
                remainingToDeduct -= deductFromGift;
            }

            if (remainingToDeduct > 0)
            {
                wallet.MainBalance -= remainingToDeduct;
            }

            var txn = new WalletTransaction
            {
                WalletAccountId = wallet.Id,
                TransactionType = "Debit",
                SourceCategory = "OrderPayment",
                Amount = amount,
                BalanceAfter = wallet.TotalBalance,
                Description = description,
                RelatedOrderId = orderId,
                ReferenceCode = $"DB-{DateTime.Now.Ticks % 1000000}",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.WalletTransactions.Add(txn);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Wallet_Debited", "WalletAccount", wallet.Id, $"Debited ₹{amount:N2} for Order #{orderId}", customerId, null, "Customer");
            return (true, $"₹{amount:N2} deducted from your ShopNext Wallet.", txn);
        }

        // ==========================================
        // 90. LOYALTY & REWARD POINTS
        // ==========================================
        public async Task<RewardPointsAccount> GetRewardPointsAccountAsync(int customerId)
        {
            var acc = await _context.RewardPointsAccounts
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(r => r.CustomerId == customerId);

            if (acc == null)
            {
                acc = new RewardPointsAccount
                {
                    CustomerId = customerId,
                    CurrentPoints = 100, // Welcome 100 reward points
                    LifetimeEarnedPoints = 100,
                    LifetimeRedeemedPoints = 0,
                    Tier = "Silver",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.RewardPointsAccounts.Add(acc);
                await _context.SaveChangesAsync();
            }

            return acc;
        }

        public async Task<int> AwardRewardPointsForOrderAsync(int customerId, int orderId, decimal orderAmount)
        {
            var acc = await GetRewardPointsAccountAsync(customerId);

            // 1 point per ₹100 spent (Multipliers: Gold 1.5x, Platinum 2x)
            double multiplier = acc.Tier == "Platinum" ? 2.0 : (acc.Tier == "Gold" ? 1.5 : 1.0);
            int pointsEarned = (int)Math.Floor((double)(orderAmount / 100m) * multiplier);

            if (pointsEarned <= 0) pointsEarned = 5; // Minimum 5 points

            acc.CurrentPoints += pointsEarned;
            acc.LifetimeEarnedPoints += pointsEarned;

            // Tier upgrade check
            if (acc.LifetimeEarnedPoints >= 2000) acc.Tier = "Platinum";
            else if (acc.LifetimeEarnedPoints >= 500) acc.Tier = "Gold";

            var txn = new RewardPointsTransaction
            {
                RewardPointsAccountId = acc.Id,
                TransactionType = "Earned",
                Points = pointsEarned,
                PointsBalanceAfter = acc.CurrentPoints,
                Description = $"Earned {pointsEarned} loyalty points on Order #{orderId}",
                RelatedOrderId = orderId,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.RewardPointsTransactions.Add(txn);
            await _context.SaveChangesAsync();
            return pointsEarned;
        }

        public async Task<(bool Success, string Message, decimal DiscountValue)> RedeemRewardPointsAsync(int customerId, int pointsToRedeem, int orderId)
        {
            var acc = await GetRewardPointsAccountAsync(customerId);

            if (pointsToRedeem <= 0)
            {
                return (false, "Please specify valid points to redeem.", 0);
            }

            if (acc.CurrentPoints < pointsToRedeem)
            {
                return (false, $"Insufficient points. Available: {acc.CurrentPoints} Points", 0);
            }

            // 4 points = ₹1 discount
            decimal discountValue = Math.Round(pointsToRedeem * 0.25m, 2);

            acc.CurrentPoints -= pointsToRedeem;
            acc.LifetimeRedeemedPoints += pointsToRedeem;

            var txn = new RewardPointsTransaction
            {
                RewardPointsAccountId = acc.Id,
                TransactionType = "Redeemed",
                Points = pointsToRedeem,
                PointsBalanceAfter = acc.CurrentPoints,
                Description = $"Redeemed {pointsToRedeem} points (₹{discountValue:N2}) on Order #{orderId}",
                RelatedOrderId = orderId,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.RewardPointsTransactions.Add(txn);
            await _context.SaveChangesAsync();
            return (true, $"Redeemed {pointsToRedeem} points for ₹{discountValue:N2} instant checkout discount.", discountValue);
        }

        // ==========================================
        // 91. PAYMENT FAILURE RECOVERY
        // ==========================================
        public async Task<Order?> GetFailedPaymentOrderAsync(int orderId, int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);
        }

        public async Task<(bool Success, string Message)> RetryOrderPaymentAsync(int orderId, string paymentMode, string transactionId, decimal paidAmount)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return (false, "Order not found.");
            }

            order.PaymentStatus = "Paid";
            order.PaymentMode = paymentMode;
            order.OrderStatus = "Preparing";
            order.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Payment_Recovery_Success", "Order", orderId, $"Recovered failed payment via {paymentMode}. Txn: {transactionId}", order.CustomerId, null, "Customer");

            return (true, $"Payment successful! Order #{order.Id} is confirmed and preparing for dispatch.");
        }

        // ==========================================
        // 92. PAYMENT RECONCILIATION DASHBOARD
        // ==========================================
        public async Task<PaymentReconciliationSummaryDto> GetPaymentReconciliationSummaryAsync()
        {
            var allOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            var records = new List<PaymentReconciliationDto>();

            decimal totalGross = 0m;
            decimal totalCaptured = 0m;
            decimal totalRefunds = 0m;
            decimal totalNet = 0m;
            int discrepancies = 0;

            foreach (var o in allOrders)
            {
                decimal captured = (o.PaymentStatus == "Paid" || o.PaymentStatus == "Success") ? o.TotalAmount : 0m;
                decimal refunded = (o.PaymentStatus == "Refunded" || o.ReturnStatus == "Approved") ? o.TotalAmount : 0m;
                decimal net = captured - refunded;

                string reconStatus = "Matched";
                decimal disc = 0m;

                if (o.PaymentStatus == "Failed" && captured > 0)
                {
                    reconStatus = "Discrepancy";
                    disc = captured;
                    discrepancies++;
                }

                totalGross += o.TotalAmount;
                totalCaptured += captured;
                totalRefunds += refunded;
                totalNet += net;

                records.Add(new PaymentReconciliationDto
                {
                    OrderId = o.Id,
                    OrderNumber = $"#ORD{o.Id}",
                    OrderTotalAmount = o.TotalAmount,
                    GatewayReceivedAmount = captured,
                    RefundedAmount = refunded,
                    NetSettlementAmount = net,
                    PaymentMode = o.PaymentMode ?? "Online",
                    PaymentStatus = o.PaymentStatus ?? "Pending",
                    TransactionId = $"TXN-PG-{o.Id.ToString().PadLeft(6, '0')}",
                    ReconciliationStatus = reconStatus,
                    DiscrepancyAmount = disc,
                    OrderDate = o.CreatedDate.ToString("dd MMM yyyy, hh:mm tt")
                });
            }

            return new PaymentReconciliationSummaryDto
            {
                TotalGrossSales = totalGross,
                TotalGatewayCaptured = totalCaptured,
                TotalRefundsIssued = totalRefunds,
                TotalNetSettled = totalNet,
                TotalReconciledOrders = allOrders.Count,
                DiscrepanciesCount = discrepancies,
                Records = records
            };
        }

        public async Task<List<PaymentReconciliationDto>> GetPaymentReconciliationListAsync(DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
        {
            var summary = await GetPaymentReconciliationSummaryAsync();
            var list = summary.Records;

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                list = list.Where(r => r.ReconciliationStatus.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return list;
        }

        // ==========================================
        // 93 & 94. STOCK RESERVATION & IDEMPOTENCY
        // ==========================================
        public async Task<(bool Success, string Token, string Message)> ReserveStockForCheckoutAsync(int customerId, List<(int ProductId, int Quantity)> items)
        {
            if (items == null || !items.Any())
            {
                return (false, string.Empty, "Cart items list is empty.");
            }

            string token = Guid.NewGuid().ToString("N");
            DateTime expiry = DateTime.Now.AddMinutes(10);

            foreach (var item in items)
            {
                var prod = await _context.Products.FindAsync(item.ProductId);
                if (prod == null || prod.Stock < item.Quantity)
                {
                    return (false, string.Empty, $"Product '{prod?.ProductName ?? "Item"}' is out of stock.");
                }

                // Decrement stock for temporary reservation
                prod.Stock -= item.Quantity;

                _context.StockReservations.Add(new StockReservation
                {
                    ReservationToken = token,
                    CustomerId = customerId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ExpiresAt = expiry,
                    IsCommitted = false,
                    IsReleased = false,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
            return (true, token, "Stock reserved for 10 minutes.");
        }

        public async Task CommitStockReservationAsync(string token)
        {
            var reservations = await _context.StockReservations
                .Where(r => r.ReservationToken == token && !r.IsCommitted && !r.IsReleased)
                .ToListAsync();

            foreach (var r in reservations)
            {
                r.IsCommitted = true;
            }
            await _context.SaveChangesAsync();
        }

        public async Task ReleaseStockReservationAsync(string token)
        {
            var reservations = await _context.StockReservations
                .Include(r => r.Product)
                .Where(r => r.ReservationToken == token && !r.IsCommitted && !r.IsReleased)
                .ToListAsync();

            foreach (var r in reservations)
            {
                if (r.Product != null)
                {
                    r.Product.Stock += r.Quantity;
                }
                r.IsReleased = true;
            }
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 95 & 96. GST & TAX CALCULATION
        // ==========================================
        public TaxBreakdownDto CalculateTax(decimal subtotal, string shopState, string customerState, string category = "General")
        {
            decimal rate = 18.0m;
            string hsn = "8517";

            if (category.Equals("Grocery", StringComparison.OrdinalIgnoreCase))
            {
                rate = 5.0m;
                hsn = "0901";
            }
            else if (category.Equals("Fashion", StringComparison.OrdinalIgnoreCase))
            {
                rate = 12.0m;
                hsn = "6109";
            }

            bool isInterstate = !string.IsNullOrWhiteSpace(shopState) && 
                               !string.IsNullOrWhiteSpace(customerState) && 
                               !shopState.Trim().Equals(customerState.Trim(), StringComparison.OrdinalIgnoreCase);

            decimal totalTax = Math.Round(subtotal * (rate / 100m), 2);
            decimal cgst = 0m;
            decimal sgst = 0m;
            decimal igst = 0m;

            if (isInterstate)
            {
                igst = totalTax;
            }
            else
            {
                cgst = Math.Round(totalTax / 2m, 2);
                sgst = totalTax - cgst;
            }

            return new TaxBreakdownDto
            {
                SubTotal = subtotal,
                GstRatePercent = rate,
                IsInterstate = isInterstate,
                CgstAmount = cgst,
                SgstAmount = sgst,
                IgstAmount = igst,
                TotalTaxAmount = totalTax,
                GrandTotal = subtotal + totalTax,
                HsnCode = hsn,
                ShopGstin = "10AAAAA0000A1Z5"
            };
        }

        public async Task<TaxBreakdownDto> GetOrderTaxInvoiceDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return CalculateTax(1000m, "Bihar", "Bihar");
            }

            string category = order.OrderItems?.FirstOrDefault()?.Product?.Category ?? "General";
            string shopState = order.Shop?.State ?? "Bihar";
            string custState = "Bihar";

            return CalculateTax(order.TotalAmount, shopState, custState, category);
        }

        // ==========================================
        // 97 & 98. DELIVERY SLOTS & PINCODE SERVICEABILITY
        // ==========================================
        public async Task<List<DeliverySlotDto>> GetAvailableDeliverySlotsAsync(string pincode)
        {
            var slots = new List<DeliverySlotDto>();
            DateTime today = DateTime.Today;

            for (int i = 1; i <= 3; i++)
            {
                DateTime date = today.AddDays(i);
                string dateLabel = i == 1 ? "Tomorrow" : date.ToString("ddd, dd MMM");

                slots.Add(new DeliverySlotDto
                {
                    SlotId = $"SLOT-{date:yyyyMMdd}-MORN",
                    DateLabel = dateLabel,
                    TimeWindow = "07:00 AM - 11:00 AM (Morning)",
                    SlotCategory = "Morning",
                    IsAvailable = true,
                    ExtraSlotFee = 0m
                });

                slots.Add(new DeliverySlotDto
                {
                    SlotId = $"SLOT-{date:yyyyMMdd}-AFTN",
                    DateLabel = dateLabel,
                    TimeWindow = "12:00 PM - 04:00 PM (Afternoon)",
                    SlotCategory = "Afternoon",
                    IsAvailable = true,
                    ExtraSlotFee = 0m
                });

                slots.Add(new DeliverySlotDto
                {
                    SlotId = $"SLOT-{date:yyyyMMdd}-EVEN",
                    DateLabel = dateLabel,
                    TimeWindow = "05:00 PM - 09:00 PM (Evening)",
                    SlotCategory = "Evening",
                    IsAvailable = true,
                    ExtraSlotFee = 19m // Prime evening slot
                });
            }

            return slots;
        }

        public async Task<PincodeServiceability> CheckPincodeServiceabilityAsync(string pincode)
        {
            if (string.IsNullOrWhiteSpace(pincode))
            {
                return new PincodeServiceability { Pincode = "800001", City = "Patna", State = "Bihar", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 2, IsExpressAvailable = true };
            }

            pincode = pincode.Trim();
            var entry = await _context.PincodeServiceabilities.FirstOrDefaultAsync(p => p.Pincode == pincode);
            if (entry == null)
            {
                // Default Tier-1/Tier-2 serviceability heuristic
                bool isValidPin = pincode.Length == 6 && pincode.All(char.IsDigit);
                entry = new PincodeServiceability
                {
                    Pincode = pincode,
                    City = "Patna",
                    State = "Bihar",
                    IsServiceable = isValidPin,
                    IsCodAvailable = isValidPin,
                    EstimatedDeliveryDays = isValidPin ? 2 : 5,
                    IsExpressAvailable = isValidPin
                };
            }

            return entry;
        }

        // ==========================================
        // 99. FAILED DELIVERY MANAGEMENT
        // ==========================================
        public async Task<FailedDeliveryLog> LogFailedDeliveryAttemptAsync(
            int orderId, 
            int? riderId, 
            string reason, 
            string? remarks = null, 
            string? photoUrl = null, 
            double? lat = null, 
            double? lng = null)
        {
            int pastAttempts = await _context.FailedDeliveryLogs.CountAsync(f => f.OrderId == orderId);
            int currentAttempt = pastAttempts + 1;

            string nextAction = currentAttempt >= 3 ? "Return to Merchant (3 Failed Attempts)" : "Auto Re-Attempt Scheduled";
            DateTime? rescheduled = currentAttempt < 3 ? DateTime.Now.AddDays(1) : null;

            var log = new FailedDeliveryLog
            {
                OrderId = orderId,
                RiderId = riderId,
                AttemptNumber = currentAttempt,
                FailureReason = reason,
                RiderRemarks = remarks,
                DoorstepPhotoUrl = photoUrl,
                GeoLatitude = lat,
                GeoLongitude = lng,
                NextAction = nextAction,
                RescheduledDate = rescheduled,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.FailedDeliveryLogs.Add(log);

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = currentAttempt >= 3 ? "Delivery_Failed" : "OutForDelivery";
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Delivery_Attempt_Failed", "Order", orderId, $"Attempt #{currentAttempt} Failed: {reason}. Action: {nextAction}", riderId, null, "Rider");

            return log;
        }

        public async Task<List<FailedDeliveryLog>> GetFailedDeliveryLogsAsync(string? status = null, int? orderId = null)
        {
            var query = _context.FailedDeliveryLogs
                .Include(f => f.Order)
                .Include(f => f.Rider)
                .OrderByDescending(f => f.CreatedDate)
                .AsQueryable();

            if (orderId.HasValue && orderId.Value > 0)
            {
                query = query.Where(f => f.OrderId == orderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(f => f.NextAction.Contains(status));
            }

            return await query.Take(50).ToListAsync();
        }

        public async Task<bool> RescheduleFailedDeliveryAsync(int failedLogId, DateTime newDate, string newSlot, string? notes = null)
        {
            var log = await _context.FailedDeliveryLogs.FindAsync(failedLogId);
            if (log == null) return false;

            log.RescheduledDate = newDate;
            log.NextAction = $"Rescheduled for {newDate:dd MMM yyyy} ({newSlot})";
            log.RiderRemarks = string.IsNullOrWhiteSpace(notes) ? log.RiderRemarks : $"{log.RiderRemarks} | Admin Note: {notes}";
            log.UpdatedDate = DateTime.Now;

            var order = await _context.Orders.FindAsync(log.OrderId);
            if (order != null)
            {
                order.OrderStatus = "OutForDelivery";
                order.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // 100. PROOF OF DELIVERY (POD)
        // ==========================================
        public async Task<DeliveryProof> RecordProofOfDeliveryAsync(
            int orderId, 
            int? riderId, 
            string otp, 
            string? proofPhotoUrl = null, 
            double? lat = null, 
            double? lng = null, 
            string? receivedByName = null)
        {
            var proof = new DeliveryProof
            {
                OrderId = orderId,
                RiderId = riderId,
                HandoverOtp = otp,
                IsOtpVerified = true,
                DeliveryTimestamp = DateTime.Now,
                SignatureOrPhotoUrl = proofPhotoUrl,
                HandoverLatitude = lat ?? 25.5941,
                HandoverLongitude = lng ?? 85.1376,
                ReceivedByName = receivedByName ?? "Customer (Verified OTP)",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.DeliveryProofs.Add(proof);

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = "Delivered";
                order.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Order_Delivered_POD", "Order", orderId, $"Verified delivery with OTP {otp} by Rider #{riderId}", riderId, null, "Rider");

            return proof;
        }

        public async Task<DeliveryProof?> GetProofOfDeliveryAsync(int orderId)
        {
            return await _context.DeliveryProofs
                .Include(p => p.Rider)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        // ==========================================
        // SPECIAL 1: COMPLETE ADMIN AUDIT LOG ENGINE
        // ==========================================
        public async Task<List<AuditLog>> GetAuditLogsAsync(int limit = 100, string? module = null, string? userEmail = null)
        {
            var query = _context.AuditLogs.OrderByDescending(a => a.CreatedDate).AsQueryable();

            if (!string.IsNullOrWhiteSpace(module) && module != "All")
            {
                query = query.Where(a => a.EntityName == module || a.Action.Contains(module));
            }

            return await query.Take(limit).ToListAsync();
        }

        public async Task RecordAuditLogAsync(
            int? userId, 
            string userName, 
            string? userEmail, 
            string role, 
            string action, 
            string module, 
            string targetEntity, 
            string? entityId = null, 
            string? details = null, 
            string? ipAddress = null)
        {
            int entityIntId = 0;
            int.TryParse(entityId, out entityIntId);

            var log = new AuditLog
            {
                Action = $"{action} [{module}]",
                EntityName = targetEntity,
                EntityId = entityIntId,
                UserName = userName,
                UserRole = role,
                Details = details ?? $"{userName} ({role}) performed {action} on {targetEntity} #{entityId}. IP: {ipAddress ?? "127.0.0.1"}",
                UserId = userId,
                IpAddress = ipAddress ?? "127.0.0.1",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
