using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<IEnumerable<Note>> GetByTicketIdAsync(int ticketId);
        Task<IEnumerable<Note>> GetBySupportAgentIdAsync(int supportAgentId);
        Task<IEnumerable<Note>> GetSystemNotesAsync(int ticketId);
        Task<IEnumerable<Note>> GetPrivateNotesAsync(int ticketId);
        Task<IEnumerable<Note>> GetPublicNotesAsync(int ticketId);
    }
}