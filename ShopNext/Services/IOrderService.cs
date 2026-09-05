using ShopNext.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles order lifecycle, line items, status progression, returns, and tracking.
    /// </summary>
    public interface IOrderService
    {
        Task<int> CreateOrderWithItemsAsync(Order order, List<OrderItem> items);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId, int customerId, string cancelReason);
        Task<bool> ReturnOrderAsync(int orderId, int customerId, string returnReason);
        Task<bool> ConfirmOrderReceivedAsync(int orderId, int customerId);
        Task<OrderEvidence?> GetDeliveryEvidenceAsync(int orderId);
    }
}
