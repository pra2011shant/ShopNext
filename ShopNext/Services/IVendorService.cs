using ShopNext.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles merchant shops, vendor orders, merchant catalog, and approval operations.
    /// </summary>
    public interface IVendorService
    {
        Task<int> CreateShopAsync(Shop shop);
        Task<Shop?> GetShopByIdAsync(int id);
        Task<Shop?> GetShopByPhoneNumberAsync(string phoneNumber);
        Task<IEnumerable<Shop>> GetAllShopsAsync();
        Task<bool> UpdateShopAsync(Shop shop);
        Task<bool> DeleteShopAsync(int id, int? updatedById = null);
        Task<IEnumerable<Product>> GetProductsByShopIdAsync(int shopId);
        Task<IEnumerable<Order>> GetOrdersByShopIdAsync(int shopId);
    }
}
