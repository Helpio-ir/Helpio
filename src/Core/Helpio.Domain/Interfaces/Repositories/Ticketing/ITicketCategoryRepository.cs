using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Ticketing
{
    public interface ITicketCategoryRepository : IRepository<TicketCategory>
    {
        Task<IEnumerable<TicketCategory>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<TicketCategory>> GetActiveCategoriesAsync();
    }
}