using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public interface ITicketService
    {
        Task<TicketDto?> GetByIdAsync(int id);
        Task<TicketDto?> GetWithDetailsAsync(int id);
        Task<PaginatedResult<TicketDto>> GetTicketsAsync(PaginationRequest request);
        Task<TicketDto> CreateAsync(CreateTicketDto createDto);
        Task<TicketDto> UpdateAsync(int id, UpdateTicketDto updateDto);
        Task<bool> DeleteAsync(int id);
        
        // Filtering methods
        Task<IEnumerable<TicketDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<TicketDto>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<TicketDto>> GetBySupportAgentIdAsync(int supportAgentId);
        Task<IEnumerable<TicketDto>> GetByStateIdAsync(int stateId);
        Task<IEnumerable<TicketDto>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<TicketDto>> GetByPriorityAsync(TicketPriorityDto priority);
        Task<IEnumerable<TicketDto>> GetOverdueTicketsAsync();
        Task<IEnumerable<TicketDto>> GetUnassignedTicketsAsync();
        Task<IEnumerable<TicketDto>> GetTicketsDueSoonAsync(DateTime dueDate);
        
        // Actions
        Task<bool> AssignTicketAsync(AssignTicketDto assignDto);
        Task<bool> ResolveTicketAsync(ResolveTicketDto resolveDto);
        Task<bool> ChangeStateAsync(int ticketId, int newStateId);
        Task<bool> ChangePriorityAsync(int ticketId, TicketPriorityDto priority);
        Task<bool> SetDueDateAsync(int ticketId, DateTime? dueDate);
        
        // Statistics
        Task<int> GetTicketCountByAgentAsync(int agentId);
        Task<Dictionary<string, int>> GetTicketStatisticsAsync();
        Task<IEnumerable<TicketDto>> GetRecentTicketsAsync(int count = 10);
    }
}