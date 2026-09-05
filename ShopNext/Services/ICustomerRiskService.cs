using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    public class CustomerRiskEvaluationResult
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low"; // Low (0-29), Medium (30-69), High (70-100)
        public bool IsCodDisabled { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
        public decimal ReturnRate { get; set; }
        public decimal CancellationRate { get; set; }
        public int TotalOrders { get; set; }
        public int ReturnedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int CodRejectedOrders { get; set; }
        public int SwappedFraudCount { get; set; }
        public int ComplaintsCount { get; set; }
        public DateTime EvaluatedAt { get; set; } = DateTime.Now;
    }

    public interface ICustomerRiskService
    {
        Task<CustomerRiskEvaluationResult> EvaluateCustomerRiskAsync(int customerId);
        Task<bool> ToggleCustomerCodAsync(int customerId, string adminUser);
        Task<bool> ToggleCustomerFlagAsync(int customerId, string adminUser);
        Task<CustomerRiskEvaluationResult> RecalculateCustomerRiskAsync(int customerId, string adminUser);
    }
}
