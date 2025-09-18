using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public interface IPlanService
    {
        Task<PlanDto?> GetByIdAsync(int id);
        Task<PlanDto?> GetByTypeAsync(PlanType type);
        Task<PaginatedResult<PlanDto>> GetPlansAsync(PaginationRequest request);
        Task<PlanDto> CreateAsync(CreatePlanDto createDto);
        Task<PlanDto> UpdateAsync(int id, UpdatePlanDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PlanDto>> GetActivePlansAsync();
        Task<IEnumerable<PlanDto>> GetPublicPlansAsync(); // Plans visible to customers
        Task<PlanDto?> GetDefaultFreemiumPlanAsync();
        Task<PlanDto?> GetRecommendedPlanAsync();
        Task<bool> SetRecommendedPlanAsync(int planId);
        Task<bool> ReorderPlansAsync(Dictionary<int, int> planOrders);
    }
}