namespace ShopNext.Services
{
    /// <summary>
    /// SOLID Principle: Composed Master Service Contract implementing segregated interfaces.
    /// Provides 100% backward compatibility while following Interface Segregation Principle (ISP)
    /// and Dependency Inversion Principle (DIP).
    /// </summary>
    public interface IShopNextService : ICustomerService, IProductService, IOrderService, IVendorService, IRiderService, IAdminReportService
    {
    }
}
