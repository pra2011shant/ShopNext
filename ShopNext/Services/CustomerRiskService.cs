using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNext.Models;

namespace ShopNext.Services
{
    public class CustomerRiskService : ICustomerRiskService
    {
        private readonly ShopNextDbContext _context;
        private readonly IAuditService _auditService;

        public CustomerRiskService(ShopNextDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<CustomerRiskEvaluationResult> EvaluateCustomerRiskAsync(int customerId)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null)
            {
                return new CustomerRiskEvaluationResult
                {
                    CustomerId = customerId,
                    RiskScore = 15,
                    RiskLevel = "Low"
                };
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();

            var totalOrders = orders.Count;
            var returnedOrders = orders.Count(o => o.OrderStatus == "Returned" || o.ReturnStatus == "Returned" || o.ReturnStatus == "Approved" || o.ReturnStatus == "Product_Swapped_Fraud");
            var cancelledOrders = orders.Count(o => o.OrderStatus == "Cancelled");
            
            // Check wrong product / swap fraud incidents
            var swappedFraudCount = orders.Count(o => o.ReturnStatus == "Product_Swapped_Fraud");

            // Check COD rejected orders
            var codRejectedOrders = orders.Count(o => o.OrderStatus == "Cancelled" && 
                                                      (o.PaymentMode == "COD" || o.PaymentMode == "CashOnDelivery") &&
                                                      ((o.CancelReason != null && (o.CancelReason.Contains("Doorstep", StringComparison.OrdinalIgnoreCase) || o.CancelReason.Contains("Refused", StringComparison.OrdinalIgnoreCase))) ||
                                                       (o.Remark != null && (o.Remark.Contains("Doorstep", StringComparison.OrdinalIgnoreCase) || o.Remark.Contains("Refused", StringComparison.OrdinalIgnoreCase)))));

            // Check complaints count
            var complaintsCount = await _context.Complaints
                .AsNoTracking()
                .CountAsync(c => c.CustomerId == customerId);

            // Compute rates
            decimal returnRate = totalOrders > 0 ? Math.Round((decimal)returnedOrders / totalOrders * 100m, 1) : 0m;
            decimal cancellationRate = totalOrders > 0 ? Math.Round((decimal)cancelledOrders / totalOrders * 100m, 1) : 0m;

            // Evaluate Risk Score and Factors
            int score = 10; // Base score
            var factors = new List<string>();

            // 1. Return Rate Risk Factor
            if (totalOrders >= 3 && returnRate > 30m)
            {
                score += 30;
                factors.Add($"High Return Rate ({returnRate}%) exceeding safe threshold");
            }
            else if (totalOrders >= 2 && returnRate > 20m)
            {
                score += 15;
                factors.Add($"Elevated Return Rate ({returnRate}%)");
            }

            // 2. Cancellation Rate Risk Factor
            if (totalOrders >= 3 && cancellationRate > 25m)
            {
                score += 25;
                factors.Add($"Excessive Cancellation Rate ({cancellationRate}%)");
            }
            else if (totalOrders >= 2 && cancellationRate > 15m)
            {
                score += 10;
                factors.Add($"Elevated Cancellation Rate ({cancellationRate}%)");
            }

            // 3. Product Swapped / Wrong Product Return Fraud
            if (swappedFraudCount > 0)
            {
                score += swappedFraudCount * 40;
                factors.Add($"Flagged {swappedFraudCount}x for Wrong Product / Counterfeit Return Swap Fraud");
            }

            // 4. COD Rejection Risk Factor
            if (codRejectedOrders > 0)
            {
                score += codRejectedOrders * 20;
                factors.Add($"Repeatedly rejected COD delivery at doorstep ({codRejectedOrders}x)");
            }

            // 5. Complaints / Disputes Risk Factor
            if (complaintsCount >= 2)
            {
                score += 20;
                factors.Add($"Multiple active customer complaints and disputes ({complaintsCount})");
            }
            else if (complaintsCount == 1)
            {
                score += 10;
                factors.Add("Customer dispute ticket on record");
            }

            // Clamp score between 5 and 100
            score = Math.Max(5, Math.Min(100, score));

            string level;
            if (score >= 70)
            {
                level = "High";
            }
            else if (score >= 30)
            {
                level = "Medium";
            }
            else
            {
                level = "Low";
            }

            if (factors.Count == 0)
            {
                factors.Add("Normal shopping and return behavior observed");
            }

            return new CustomerRiskEvaluationResult
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                RiskScore = score,
                RiskLevel = level,
                IsCodDisabled = customer.IsCodDisabled,
                IsFlaggedForReview = customer.IsFlaggedForReview,
                RiskFactors = factors,
                ReturnRate = returnRate,
                CancellationRate = cancellationRate,
                TotalOrders = totalOrders,
                ReturnedOrders = returnedOrders,
                CancelledOrders = cancelledOrders,
                CodRejectedOrders = codRejectedOrders,
                SwappedFraudCount = swappedFraudCount,
                ComplaintsCount = complaintsCount,
                EvaluatedAt = DateTime.Now
            };
        }

        public async Task<CustomerRiskEvaluationResult> RecalculateCustomerRiskAsync(int customerId, string adminUser)
        {
            var result = await EvaluateCustomerRiskAsync(customerId);
            var customer = await _context.Users.FindAsync(customerId);
            if (customer != null)
            {
                customer.RiskScore = result.RiskScore;
                customer.RiskLevel = result.RiskLevel;
                customer.RiskFactorsJson = JsonSerializer.Serialize(result.RiskFactors);
                customer.RiskLastEvaluatedDate = DateTime.Now;

                // Auto-flag high risk customers if not already flagged
                if (result.RiskScore >= 70 && !customer.IsFlaggedForReview)
                {
                    customer.IsFlaggedForReview = true;
                    result.IsFlaggedForReview = true;
                }

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    action: "Risk_Score_Recalculated",
                    entityName: "User",
                    entityId: customerId,
                    details: $"Admin '{adminUser}' recalculated risk for '{customer.Name}'. New Score: {result.RiskScore} ({result.RiskLevel} Risk).",
                    userName: adminUser,
                    userRole: "Admin"
                );
            }

            return result;
        }

        public async Task<bool> ToggleCustomerCodAsync(int customerId, string adminUser)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null) return false;

            customer.IsCodDisabled = !customer.IsCodDisabled;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: customer.IsCodDisabled ? "Customer_COD_Disabled" : "Customer_COD_Enabled",
                entityName: "User",
                entityId: customerId,
                details: $"Admin '{adminUser}' {(customer.IsCodDisabled ? "disabled" : "enabled")} COD for customer '{customer.Name}'.",
                userName: adminUser,
                userRole: "Admin"
            );

            return customer.IsCodDisabled;
        }

        public async Task<bool> ToggleCustomerFlagAsync(int customerId, string adminUser)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null) return false;

            customer.IsFlaggedForReview = !customer.IsFlaggedForReview;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                action: customer.IsFlaggedForReview ? "Customer_Flagged_For_Review" : "Customer_Unflagged_Review",
                entityName: "User",
                entityId: customerId,
                details: $"Admin '{adminUser}' {(customer.IsFlaggedForReview ? "flagged" : "unflagged")} customer '{customer.Name}' for manual review.",
                userName: adminUser,
                userRole: "Admin"
            );

            return customer.IsFlaggedForReview;
        }
    }
}
