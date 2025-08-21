using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attachment>> GetByTypeAsync(string type)
        {
            return await _dbSet
                .Where(a => a.Type == type && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attachment>> GetBySizeRangeAsync(long minSize, long maxSize)
        {
            return await _dbSet
                .Where(a => a.Size >= minSize && a.Size <= maxSize && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<long> GetTotalSizeAsync()
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .SumAsync(a => a.Size);
        }

        public async Task<IEnumerable<Attachment>> GetAttachmentsByTicketAsync(int ticketId)
        {
            return await _dbSet
                .Where(a => !a.IsDeleted &&
                           (a.AttachmentNotes.Any(an => an.Note.TicketId == ticketId) ||
                            a.AttachmentResponses.Any(ar => ar.Response.TicketId == ticketId)))
                .ToListAsync();
        }
    }
}