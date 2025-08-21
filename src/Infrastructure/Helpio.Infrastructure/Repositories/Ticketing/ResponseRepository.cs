using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class ResponseRepository : Repository<Response>, IResponseRepository
    {
        public ResponseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Response>> GetByTicketIdAsync(int ticketId)
        {
            return await _dbSet
                .Where(r => r.TicketId == ticketId && !r.IsDeleted)
                .Include(r => r.User)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Response>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .Include(r => r.Ticket)
                .ToListAsync();
        }

        public async Task<IEnumerable<Response>> GetCustomerResponsesAsync(int ticketId)
        {
            return await _dbSet
                .Where(r => r.TicketId == ticketId && r.IsFromCustomer && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Response>> GetAgentResponsesAsync(int ticketId)
        {
            return await _dbSet
                .Where(r => r.TicketId == ticketId && !r.IsFromCustomer && !r.IsDeleted)
                .Include(r => r.User)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Response>> GetUnreadResponsesAsync(int ticketId)
        {
            return await _dbSet
                .Where(r => r.TicketId == ticketId && r.ReadAt == null && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<Response?> GetLatestResponseAsync(int ticketId)
        {
            return await _dbSet
                .Where(r => r.TicketId == ticketId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}