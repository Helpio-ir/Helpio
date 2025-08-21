using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public interface ITicketCategoryService
    {
        Task<TicketCategoryDto?> GetByIdAsync(int id);
        Task<PaginatedResult<TicketCategoryDto>> GetCategoriesAsync(PaginationRequest request);
        Task<TicketCategoryDto> CreateAsync(CreateTicketCategoryDto createDto);
        Task<TicketCategoryDto> UpdateAsync(int id, UpdateTicketCategoryDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TicketCategoryDto>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<TicketCategoryDto>> GetActiveCategoriesAsync();
    }
}