using ShopNext.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopNext.Services
{
    /// <summary>
    /// SOLID - Interface Segregation: Handles rider onboarding, GPS telemetry, dispatch, and fulfillment delivery.
    /// </summary>
    public interface IRiderService
    {
        Task<int> CreateRiderAsync(Rider rider);
        Task<Rider?> GetRiderByIdAsync(int id);
        Task<Rider?> GetRiderByPhoneNumberAsync(string phoneNumber);
        Task<IEnumerable<Rider>> GetAvailableRidersAsync();
        Task<bool> UpdateRiderLocationAsync(int riderId, decimal lat, decimal lng);
        Task<bool> AssignRiderToOrderAsync(int orderId, int riderId);
        Task<IEnumerable<Order>> GetRiderAssignedOrdersAsync(int riderId);
        Task<(bool Success, string Message, string? Otp)> VerifyDeliveryOtpAndCompleteAsync(int orderId, int riderId, string enteredOtp, string? notes = null);
        Task<string> GenerateOrGetDeliveryOtpAsync(int orderId);
    }
}
