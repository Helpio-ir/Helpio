using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Services.Knowledge;
using Helpio.Ir.Application.Common.Interfaces;
using System.Reflection;

namespace Helpio.Ir.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register Application Services
            RegisterCoreServices(services);
            RegisterTicketingServices(services);
            RegisterBusinessServices(services);
            RegisterKnowledgeServices(services);

            // Register Common Services
            services.AddScoped<IDateTime, DateTimeService>();

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }

        private static void RegisterCoreServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupportAgentService, SupportAgentService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
        }

        private static void RegisterTicketingServices(IServiceCollection services)
        {
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IResponseService, ResponseService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<ITicketStateService, TicketStateService>();
            services.AddScoped<ITicketCategoryService, TicketCategoryService>();
        }

        private static void RegisterBusinessServices(IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ITransactionService, TransactionService>();
        }

        private static void RegisterKnowledgeServices(IServiceCollection services)
        {
            services.AddScoped<ICannedResponseService, CannedResponseService>();
            services.AddScoped<IArticlesService, ArticlesService>();
        }
    }

    // Implementation of IDateTime
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}