using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Entities.Knowledge;

namespace Helpio.Ir.Domain.Interfaces;

public interface IApplicationDbContext
{
    // Entity DbSets
    DbSet<User> Users { get; set; }
    DbSet<Customer> Customers { get; set; }
    DbSet<Organization> Organizations { get; set; }
    DbSet<Branch> Branches { get; set; }
    DbSet<Team> Teams { get; set; }
    DbSet<SupportAgent> SupportAgents { get; set; }
    DbSet<Profile> Profiles { get; set; }
    DbSet<Ticket> Tickets { get; set; }
    DbSet<TicketState> TicketStates { get; set; }
    DbSet<TicketCategory> TicketCategories { get; set; }
    DbSet<Note> Notes { get; set; }
    DbSet<Response> Responses { get; set; }
    DbSet<Attachment> Attachments { get; set; }
    DbSet<AttachmentNote> AttachmentNotes { get; set; }
    DbSet<AttachmentResponse> AttachmentResponses { get; set; }
    DbSet<Transaction> Transactions { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<CannedResponse> CannedResponses { get; set; }
    DbSet<Articles> Articles { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Update<TEntity>(TEntity entity) where TEntity : class;
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}