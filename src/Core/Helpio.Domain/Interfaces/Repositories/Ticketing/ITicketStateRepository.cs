using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface ITicketStateRepository : IRepository<TicketState>
    {
        Task<TicketState?> GetDefaultStateAsync();
        Task<IEnumerable<TicketState>> GetFinalStatesAsync();
        Task<IEnumerable<TicketState>> GetOrderedStatesAsync();
    }
}