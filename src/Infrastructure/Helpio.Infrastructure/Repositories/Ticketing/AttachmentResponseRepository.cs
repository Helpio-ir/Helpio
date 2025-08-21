using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class AttachmentResponseRepository : Repository<AttachmentResponse>, IAttachmentResponseRepository
    {
        public AttachmentResponseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AttachmentResponse>> GetByAttachmentIdAsync(int attachmentId)
        {
            return await _dbSet
                .Where(ar => ar.AttachmentId == attachmentId && !ar.IsDeleted)
                .Include(ar => ar.Response)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttachmentResponse>> GetByResponseIdAsync(int responseId)
        {
            return await _dbSet
                .Where(ar => ar.ResponseId == responseId && !ar.IsDeleted)
                .Include(ar => ar.Attachment)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int attachmentId, int responseId)
        {
            return await _dbSet
                .AnyAsync(ar => ar.AttachmentId == attachmentId && ar.ResponseId == responseId && !ar.IsDeleted);
        }

        public async Task<AttachmentResponse?> GetByAttachmentAndResponseIdAsync(int attachmentId, int responseId)
        {
            return await _dbSet
                .Include(ar => ar.Attachment)
                .Include(ar => ar.Response)
                .FirstOrDefaultAsync(ar => ar.AttachmentId == attachmentId && ar.ResponseId == responseId);
        }
    }
}