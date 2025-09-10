using Helpio.Ir.Domain.Entities;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entity DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<SupportAgent> SupportAgents { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketState> TicketStates { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<AttachmentNote> AttachmentNotes { get; set; }
        public DbSet<AttachmentResponse> AttachmentResponses { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<CannedResponse> CannedResponses { get; set; }
        public DbSet<Articles> Articles { get; set; }

        void IApplicationDbContext.Update<TEntity>(TEntity entity)
        {
            base.Update(entity);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Entity configurations will be added here
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User relationships
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Customer relationships
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // SupportAgent relationships
            modelBuilder.Entity<SupportAgent>()
                .HasOne(sa => sa.User)
                .WithOne()
                .HasForeignKey<SupportAgent>(sa => sa.UserId);

            modelBuilder.Entity<SupportAgent>()
                .HasOne(sa => sa.Profile)
                .WithOne(p => p.SupportAgent)
                .HasForeignKey<SupportAgent>(sa => sa.ProfileId);

            modelBuilder.Entity<SupportAgent>()
                .HasOne(sa => sa.Team)
                .WithMany(t => t.SupportAgents)
                .HasForeignKey(sa => sa.TeamId);

            // Team relationships
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Branch)
                .WithMany(b => b.Teams)
                .HasForeignKey(t => t.BranchId);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.TeamLead)
                .WithMany(sa => sa.ManagedTeams)
                .HasForeignKey(t => t.TeamLeadId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Supervisor)
                .WithMany(sa => sa.SupervisedTeams)
                .HasForeignKey(t => t.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Branch relationships
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Organization)
                .WithMany(o => o.Branches)
                .HasForeignKey(b => b.OrganizationId);

            // Ticket relationships - Fix cascade conflicts
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Customer)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.CustomerId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.TicketState)
                .WithMany(ts => ts.Tickets)
                .HasForeignKey(t => t.TicketStateId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade conflicts

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Team)
                .WithMany(team => team.Tickets)
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade conflicts

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.SupportAgent)
                .WithMany(sa => sa.AssignedTickets)
                .HasForeignKey(t => t.SupportAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.TicketCategory)
                .WithMany(tc => tc.Tickets)
                .HasForeignKey(t => t.TicketCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade conflicts

            // TicketCategory relationships
            modelBuilder.Entity<TicketCategory>()
                .HasOne(tc => tc.Organization)
                .WithMany(o => o.TicketCategories)
                .HasForeignKey(tc => tc.OrganizationId);

            // Note relationships
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Ticket)
                .WithMany(t => t.Notes)
                .HasForeignKey(n => n.TicketId);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.SupportAgent)
                .WithMany(sa => sa.Notes)
                .HasForeignKey(n => n.SupportAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Response relationships
            modelBuilder.Entity<Response>()
                .HasOne(r => r.Ticket)
                .WithMany(t => t.Responses)
                .HasForeignKey(r => r.TicketId);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Attachment junction tables
            modelBuilder.Entity<AttachmentNote>()
                .HasKey(an => new { an.AttachmentId, an.NoteId });

            modelBuilder.Entity<AttachmentNote>()
                .HasOne(an => an.Attachment)
                .WithMany(a => a.AttachmentNotes)
                .HasForeignKey(an => an.AttachmentId);

            modelBuilder.Entity<AttachmentNote>()
                .HasOne(an => an.Note)
                .WithMany(n => n.AttachmentNotes)
                .HasForeignKey(an => an.NoteId);

            modelBuilder.Entity<AttachmentResponse>()
                .HasKey(ar => new { ar.AttachmentId, ar.ResponseId });

            modelBuilder.Entity<AttachmentResponse>()
                .HasOne(ar => ar.Attachment)
                .WithMany(a => a.AttachmentResponses)
                .HasForeignKey(ar => ar.AttachmentId);

            modelBuilder.Entity<AttachmentResponse>()
                .HasOne(ar => ar.Response)
                .WithMany(r => r.AttachmentResponses)
                .HasForeignKey(ar => ar.ResponseId);

            // Business relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Subscription)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.SubscriptionId);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Organization)
                .WithMany()
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Knowledge relationships
            modelBuilder.Entity<CannedResponse>()
                .HasOne(cr => cr.Organization)
                .WithMany(o => o.CannedResponses)
                .HasForeignKey(cr => cr.OrganizationId);

            modelBuilder.Entity<Articles>()
                .HasOne(a => a.Organization)
                .WithMany(o => o.Articles)
                .HasForeignKey(a => a.OrganizationId);

            modelBuilder.Entity<Articles>()
                .HasOne(a => a.Author)
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            // ApiKey relationships
            modelBuilder.Entity<ApiKey>()
                .HasOne(ak => ak.Organization)
                .WithMany(o => o.ApiKeys)
                .HasForeignKey(ak => ak.OrganizationId);

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.KeyValue)
                .IsUnique();

            modelBuilder.Entity<ApiKey>()
                .HasIndex(ak => ak.KeyHash)
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}