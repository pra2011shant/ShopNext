using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface IFlashSaleService
    {
        Task<List<AdminFlashSaleDto>> GetAllFlashSalesAsync();
        Task<AdminFlashSaleDto?> GetFlashSaleByIdAsync(int id);
        Task<AdminFlashSaleDto> CreateFlashSaleAsync(CreateOrEditFlashSaleRequest request);
        Task<AdminFlashSaleDto?> UpdateFlashSaleAsync(CreateOrEditFlashSaleRequest request);
        Task<AdminFlashSaleDto?> ToggleFlashSaleStatusAsync(int id);
        Task<bool> DeleteFlashSaleAsync(int id);
        Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> ClaimFlashSaleUnitAsync(int flashSaleId, int quantity = 1, int customerId = 1);
        Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> ClaimFlashSaleUnitAsync(ClaimFlashSaleRequest request);
        Task<(bool Success, string Message, AdminFlashSaleDto? Deal)> RestockFlashSaleAsync(int id, int addedUnits = 20);
        Task<int> SyncFlashSaleLifecyclesAsync();
    }
}
