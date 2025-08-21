using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface IAttachmentNoteRepository : IRepository<AttachmentNote>
    {
        Task<IEnumerable<AttachmentNote>> GetByAttachmentIdAsync(int attachmentId);
        Task<IEnumerable<AttachmentNote>> GetByNoteIdAsync(int noteId);
        Task<bool> ExistsAsync(int attachmentId, int noteId);
        Task<AttachmentNote?> GetByAttachmentAndNoteIdAsync(int attachmentId, int noteId);
    }
}