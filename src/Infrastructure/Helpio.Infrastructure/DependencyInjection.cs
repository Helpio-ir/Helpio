using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Interfaces.Repositories.Business;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Knowledge;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Infrastructure.Repositories;
using Helpio.Ir.Infrastructure.Repositories.Business;
using Helpio.Ir.Infrastructure.Repositories.Core;
using Helpio.Ir.Infrastructure.Repositories.Knowledge;
using Helpio.Ir.Infrastructure.Repositories.Ticketing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpio.Ir.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Core Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ISupportAgentRepository, SupportAgentRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

            // Ticketing Repositories
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketStateRepository, TicketStateRepository>();
            services.AddScoped<ITicketCategoryRepository, TicketCategoryRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IResponseRepository, ResponseRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<IAttachmentNoteRepository, AttachmentNoteRepository>();
            services.AddScoped<IAttachmentResponseRepository, AttachmentResponseRepository>();

            // Business Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            // Knowledge Repositories
            services.AddScoped<ICannedResponseRepository, CannedResponseRepository>();
            services.AddScoped<IArticlesRepository, ArticlesRepository>();

            return services;
        }
    }
}