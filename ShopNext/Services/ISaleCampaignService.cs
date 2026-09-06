using System.Collections.Generic;
using System.Threading.Tasks;
using ShopNext.Models;

namespace ShopNext.Services
{
    public interface ISaleCampaignService
    {
        Task<List<AdminSaleCampaignDto>> GetAllCampaignsAsync();
        Task<AdminSaleCampaignDto?> GetCampaignByIdAsync(int id);
        Task<AdminSaleCampaignDto> CreateCampaignAsync(CreateOrEditSaleCampaignRequest request);
        Task<AdminSaleCampaignDto?> UpdateCampaignAsync(CreateOrEditSaleCampaignRequest request);
        Task<bool> ToggleCampaignStatusAsync(int id);
        Task<bool> DeleteCampaignAsync(int id);
        Task<int> AutoSyncCampaignLifecyclesAsync();

        // Point 55: Seller Sale Participation & Admin Approval Workflow
        Task<List<AdminSellerSaleParticipationDto>> GetAllSellerSaleParticipationsAsync(int? campaignId = null, int? shopId = null);
        Task<AdminSellerSaleParticipationDto?> UpdateSellerProductParticipationAsync(int participationId, bool isParticipating, decimal? salePrice, int? allocatedStock, string? notes);
        Task<AdminSellerSaleParticipationDto?> ApproveSellerParticipationAsync(int participationId, string? remarks, string adminName);
        Task<AdminSellerSaleParticipationDto?> RejectSellerParticipationAsync(int participationId, string? reason, string adminName);
        Task<AdminSellerSaleParticipationDto> OptInProductForSaleAsync(int productId, int shopId, int campaignId, decimal salePrice, int allocatedStock);
    }
}
