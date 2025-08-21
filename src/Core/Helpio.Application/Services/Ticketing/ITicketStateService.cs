using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public interface ITicketStateService
    {
        Task<TicketStateDto?> GetByIdAsync(int id);
        Task<PaginatedResult<TicketStateDto>> GetStatesAsync(PaginationRequest request);
        Task<TicketStateDto> CreateAsync(CreateTicketStateDto createDto);
        Task<TicketStateDto> UpdateAsync(int id, UpdateTicketStateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<TicketStateDto?> GetDefaultStateAsync();
        Task<IEnumerable<TicketStateDto>> GetFinalStatesAsync();
        Task<IEnumerable<TicketStateDto>> GetOrderedStatesAsync();
    }
}