using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IInventoryProtectionService
    {
        /// <summary>
        /// Atomically checks and decrements product stock within a safe database transaction.
        /// Guaranteed 0 overselling under high concurrency.
        /// </summary>
        Task<StockReservationResult> TryReserveStockAsync(int productId, int quantity, string orderReference, int customerId);

        /// <summary>
        /// Releases safely reserved stock on cart timeout or order cancellation.
        /// </summary>
        Task<StockReleaseResult> ReleaseStockAsync(int productId, int quantity, string orderReference, string reason);

        /// <summary>
        /// Runs a live multi-threaded concurrency stress test simulating high-volume surge traffic (e.g., 100 users ordering 10 units simultaneously).
        /// </summary>
        Task<ConcurrencySimulationResult> RunConcurrencySimulationAsync(ConcurrencySimulationRequest request);

        /// <summary>
        /// Gets dashboard summary metrics for inventory protection and recent concurrency logs.
        /// </summary>
        Task<InventoryProtectionDashboardDto> GetDashboardSummaryAsync();
    }
}
