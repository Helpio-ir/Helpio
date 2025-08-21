using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface IAttachmentResponseRepository : IRepository<AttachmentResponse>
    {
        Task<IEnumerable<AttachmentResponse>> GetByAttachmentIdAsync(int attachmentId);
        Task<IEnumerable<AttachmentResponse>> GetByResponseIdAsync(int responseId);
        Task<bool> ExistsAsync(int attachmentId, int responseId);
        Task<AttachmentResponse?> GetByAttachmentAndResponseIdAsync(int attachmentId, int responseId);
    }
}