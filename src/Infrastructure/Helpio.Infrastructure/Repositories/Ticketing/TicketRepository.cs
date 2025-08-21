using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(t => t.CustomerId == customerId && !t.IsDeleted)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByTeamIdAsync(int teamId)
        {
            return await _dbSet
                .Where(t => t.TeamId == teamId && !t.IsDeleted)
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.SupportAgent)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetBySupportAgentIdAsync(int supportAgentId)
        {
            return await _dbSet
                .Where(t => t.SupportAgentId == supportAgentId && !t.IsDeleted)
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByStateIdAsync(int stateId)
        {
            return await _dbSet
                .Where(t => t.TicketStateId == stateId && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Where(t => t.TicketCategoryId == categoryId && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByPriorityAsync(TicketPriority priority)
        {
            return await _dbSet
                .Where(t => t.Priority == priority && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetOverdueTicketsAsync()
        {
            return await _dbSet
                .Where(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow &&
                           t.ResolvedDate == null && !t.IsDeleted)
                .Include(t => t.Customer)
                .Include(t => t.SupportAgent)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync()
        {
            return await _dbSet
                .Where(t => t.SupportAgentId == null && !t.IsDeleted)
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .ToListAsync();
        }

        public async Task<Ticket?> GetWithDetailsAsync(int ticketId)
        {
            return await _dbSet
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.Team)
                .Include(t => t.SupportAgent)
                .Include(t => t.TicketCategory)
                .Include(t => t.Responses)
                .Include(t => t.Notes)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsDueSoonAsync(DateTime dueDate)
        {
            return await _dbSet
                .Where(t => t.DueDate.HasValue && t.DueDate <= dueDate &&
                           t.ResolvedDate == null && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<int> GetTicketCountByAgentAsync(int agentId)
        {
            return await _dbSet
                .CountAsync(t => t.SupportAgentId == agentId && t.ResolvedDate == null && !t.IsDeleted);
        }
    }
}