using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Ticket>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<Ticket>> GetBySupportAgentIdAsync(int supportAgentId);
        Task<IEnumerable<Ticket>> GetByStateIdAsync(int stateId);
        Task<IEnumerable<Ticket>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority);
        Task<IEnumerable<Ticket>> GetOverdueTicketsAsync();
        Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync();
        Task<Ticket?> GetWithDetailsAsync(int ticketId);
        Task<IEnumerable<Ticket>> GetTicketsDueSoonAsync(DateTime dueDate);
        Task<int> GetTicketCountByAgentAsync(int agentId);
    }
}