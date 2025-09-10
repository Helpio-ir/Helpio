using Microsoft.EntityFrameworkCore.Storage;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Business;
using Helpio.Ir.Domain.Interfaces.Repositories.Knowledge;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Infrastructure.Repositories.Core;
using Helpio.Ir.Infrastructure.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Repositories.Business;
using Helpio.Ir.Infrastructure.Repositories.Knowledge;

namespace Helpio.Ir.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Core Repositories
        private IUserRepository? _users;
        private ICustomerRepository? _customers;
        private IOrganizationRepository? _organizations;
        private IBranchRepository? _branches;
        private ITeamRepository? _teams;
        private ISupportAgentRepository? _supportAgents;
        private IProfileRepository? _profiles;
        private IApiKeyRepository? _apiKeys;

        // Ticketing Repositories
        private ITicketRepository? _tickets;
        private ITicketStateRepository? _ticketStates;
        private ITicketCategoryRepository? _ticketCategories;
        private INoteRepository? _notes;
        private IResponseRepository? _responses;
        private IAttachmentRepository? _attachments;
        private IAttachmentNoteRepository? _attachmentNotes;
        private IAttachmentResponseRepository? _attachmentResponses;

        // Business Repositories
        private ITransactionRepository? _transactions;
        private IOrderRepository? _orders;
        private ISubscriptionRepository? _subscriptions;

        // Knowledge Repositories
        private ICannedResponseRepository? _cannedResponses;
        private IArticlesRepository? _articles;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Core Repositories Properties
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public IOrganizationRepository Organizations => _organizations ??= new OrganizationRepository(_context);
        public IBranchRepository Branches => _branches ??= new BranchRepository(_context);
        public ITeamRepository Teams => _teams ??= new TeamRepository(_context);
        public ISupportAgentRepository SupportAgents => _supportAgents ??= new SupportAgentRepository(_context);
        public IProfileRepository Profiles => _profiles ??= new ProfileRepository(_context);
        public IApiKeyRepository ApiKeys => _apiKeys ??= new ApiKeyRepository(_context);

        // Ticketing Repositories Properties
        public ITicketRepository Tickets => _tickets ??= new TicketRepository(_context);
        public ITicketStateRepository TicketStates => _ticketStates ??= new TicketStateRepository(_context);
        public ITicketCategoryRepository TicketCategories => _ticketCategories ??= new TicketCategoryRepository(_context);
        public INoteRepository Notes => _notes ??= new NoteRepository(_context);
        public IResponseRepository Responses => _responses ??= new ResponseRepository(_context);
        public IAttachmentRepository Attachments => _attachments ??= new AttachmentRepository(_context);
        public IAttachmentNoteRepository AttachmentNotes => _attachmentNotes ??= new AttachmentNoteRepository(_context);
        public IAttachmentResponseRepository AttachmentResponses => _attachmentResponses ??= new AttachmentResponseRepository(_context);

        // Business Repositories Properties
        public ITransactionRepository Transactions => _transactions ??= new TransactionRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);

        // Knowledge Repositories Properties
        public ICannedResponseRepository CannedResponses => _cannedResponses ??= new CannedResponseRepository(_context);
        public IArticlesRepository Articles => _articles ??= new ArticlesRepository(_context);

        // Unit of Work methods
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}