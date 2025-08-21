using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class TicketStateRepository : Repository<TicketState>, ITicketStateRepository
    {
        public TicketStateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TicketState?> GetDefaultStateAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(ts => ts.IsDefault && !ts.IsDeleted);
        }

        public async Task<IEnumerable<TicketState>> GetFinalStatesAsync()
        {
            return await _dbSet
                .Where(ts => ts.IsFinal && !ts.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketState>> GetOrderedStatesAsync()
        {
            return await _dbSet
                .Where(ts => !ts.IsDeleted)
                .OrderBy(ts => ts.Order)
                .ToListAsync();
        }
    }
}