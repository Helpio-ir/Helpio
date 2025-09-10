using Helpio.Ir.Domain.Entities;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entity DbSets - Override Users to match interface
        public override DbSet<User> Users { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<SupportAgent> SupportAgents { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<ApiKey> ApiKeys { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketState> TicketStates { get; set; } = null!;
        public DbSet<TicketCategory> TicketCategories { get; set; } = null!;
        public DbSet<Note> Notes { get; set; } = null!;
        public DbSet<Response> Responses { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
        public DbSet<AttachmentNote> AttachmentNotes { get; set; } = null!;
        public DbSet<AttachmentResponse> AttachmentResponses { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<CannedResponse> CannedResponses { get; set; } = null!;
        public DbSet<Articles> Articles { get; set; } = null!;

        void IApplicationDbContext.Update<TEntity>(TEntity entity)
        {
            base.Update(entity);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity tables with Persian names
            modelBuilder.Entity<User>().ToTable("Users", "Identity");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles", "Identity");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles", "Identity");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims", "Identity");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins", "Identity");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens", "Identity");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims", "Identity");

            // Entity configurations
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

            // Handle User entities separately
            var userEntries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in userEntries)
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