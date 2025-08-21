using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class NoteRepository : Repository<Note>, INoteRepository
    {
        public NoteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Note>> GetByTicketIdAsync(int ticketId)
        {
            return await _dbSet
                .Where(n => n.TicketId == ticketId && !n.IsDeleted)
                .Include(n => n.SupportAgent)
                .ThenInclude(sa => sa.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetBySupportAgentIdAsync(int supportAgentId)
        {
            return await _dbSet
                .Where(n => n.SupportAgentId == supportAgentId && !n.IsDeleted)
                .Include(n => n.Ticket)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetSystemNotesAsync(int ticketId)
        {
            return await _dbSet
                .Where(n => n.TicketId == ticketId && n.IsSystemNote && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetPrivateNotesAsync(int ticketId)
        {
            return await _dbSet
                .Where(n => n.TicketId == ticketId && n.IsPrivate && !n.IsSystemNote && !n.IsDeleted)
                .Include(n => n.SupportAgent)
                .ThenInclude(sa => sa.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> GetPublicNotesAsync(int ticketId)
        {
            return await _dbSet
                .Where(n => n.TicketId == ticketId && !n.IsPrivate && !n.IsSystemNote && !n.IsDeleted)
                .Include(n => n.SupportAgent)
                .ThenInclude(sa => sa.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}