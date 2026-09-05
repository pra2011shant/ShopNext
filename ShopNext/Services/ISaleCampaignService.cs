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
    }
}
