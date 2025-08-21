using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface IAttachmentRepository : IRepository<Attachment>
    {
        Task<IEnumerable<Attachment>> GetByTypeAsync(string type);
        Task<IEnumerable<Attachment>> GetBySizeRangeAsync(long minSize, long maxSize);
        Task<long> GetTotalSizeAsync();
        Task<IEnumerable<Attachment>> GetAttachmentsByTicketAsync(int ticketId);
    }
}