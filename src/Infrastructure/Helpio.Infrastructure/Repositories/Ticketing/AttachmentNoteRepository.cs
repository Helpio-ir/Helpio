using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class AttachmentNoteRepository : Repository<AttachmentNote>, IAttachmentNoteRepository
    {
        public AttachmentNoteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AttachmentNote>> GetByAttachmentIdAsync(int attachmentId)
        {
            return await _dbSet
                .Where(an => an.AttachmentId == attachmentId && !an.IsDeleted)
                .Include(an => an.Note)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttachmentNote>> GetByNoteIdAsync(int noteId)
        {
            return await _dbSet
                .Where(an => an.NoteId == noteId && !an.IsDeleted)
                .Include(an => an.Attachment)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int attachmentId, int noteId)
        {
            return await _dbSet
                .AnyAsync(an => an.AttachmentId == attachmentId && an.NoteId == noteId && !an.IsDeleted);
        }

        public async Task<AttachmentNote?> GetByAttachmentAndNoteIdAsync(int attachmentId, int noteId)
        {
            return await _dbSet
                .Include(an => an.Attachment)
                .Include(an => an.Note)
                .FirstOrDefaultAsync(an => an.AttachmentId == attachmentId && an.NoteId == noteId);
        }
    }
}