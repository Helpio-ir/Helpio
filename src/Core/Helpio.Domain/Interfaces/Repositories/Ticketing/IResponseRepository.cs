using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface IResponseRepository : IRepository<Response>
    {
        Task<IEnumerable<Response>> GetByTicketIdAsync(int ticketId);
        Task<IEnumerable<Response>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Response>> GetCustomerResponsesAsync(int ticketId);
        Task<IEnumerable<Response>> GetAgentResponsesAsync(int ticketId);
        Task<IEnumerable<Response>> GetUnreadResponsesAsync(int ticketId);
        Task<Response?> GetLatestResponseAsync(int ticketId);
    }
}