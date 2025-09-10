using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Business;
using Helpio.Ir.Domain.Interfaces.Repositories.Knowledge;

namespace Helpio.Ir.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Core Repositories
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IOrganizationRepository Organizations { get; }
        IBranchRepository Branches { get; }
        ITeamRepository Teams { get; }
        ISupportAgentRepository SupportAgents { get; }
        IProfileRepository Profiles { get; }
        IApiKeyRepository ApiKeys { get; }

        // Ticketing Repositories
        ITicketRepository Tickets { get; }
        ITicketStateRepository TicketStates { get; }
        ITicketCategoryRepository TicketCategories { get; }
        INoteRepository Notes { get; }
        IResponseRepository Responses { get; }
        IAttachmentRepository Attachments { get; }
        IAttachmentNoteRepository AttachmentNotes { get; }
        IAttachmentResponseRepository AttachmentResponses { get; }

        // Business Repositories
        ITransactionRepository Transactions { get; }
        IOrderRepository Orders { get; }
        ISubscriptionRepository Subscriptions { get; }

        // Knowledge Repositories
        ICannedResponseRepository CannedResponses { get; }
        IArticlesRepository Articles { get; }

        // Unit of Work methods
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}