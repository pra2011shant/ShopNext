using ShopNext.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles product catalog, variants, search, inventory, and reviews.
    /// </summary>
    public interface IProductService
    {
        Task<int> CreateProductAsync(Product product);
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> UpdateProductStockAsync(int productId, int newStock);
        Task<bool> DeleteProductAsync(int id, int? updatedById = null);
        Task<IEnumerable<Product>> SearchProductsAsync(string? query, string? category, decimal? minPrice, decimal? maxPrice, string? sortBy);

        // Product Variant Operations
        Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId);
        Task<bool> AddProductVariantsBatchAsync(int productId, List<ProductVariant> variants);

        // 37. Product Approval Operations
        Task<bool> ApproveProductAsync(int productId, int? adminId = null);
        Task<bool> RejectProductAsync(int productId, string reason, int? adminId = null);

        // Product Reviews & Ratings (Point 42 Moderation)
        Task<int> InsertReviewAsync(Review review);
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByShopIdAsync(int shopId);
        Task<IEnumerable<Review>> GetReviewsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Review>> GetAllReviewsForAdminAsync();
        Task<Review?> GetReviewByIdAsync(int id);
        Task<bool> HideReviewAsync(int reviewId, string? reason = null, int? adminId = null);
        Task<bool> UnhideReviewAsync(int reviewId, int? adminId = null);
        Task<bool> DeleteReviewAsync(int reviewId, int? adminId = null);

        // 41. Offers & Deals Operations
        Task<IEnumerable<Offer>> GetActiveOffersAsync();
        Task<IEnumerable<Offer>> GetAllOffersAsync();
        Task<Offer?> GetOfferByIdAsync(int id);
        Task<int> CreateOfferAsync(Offer offer);
        Task<bool> UpdateOfferAsync(Offer offer);
        Task<bool> DeleteOfferAsync(int id, int? adminId = null);
        Task<bool> ToggleOfferStatusAsync(int id, int? adminId = null);
    }
}
