using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Infrastructure.Data.Seeders
{
    public static class SampleDataSeeder
    {
        public static async Task SeedSampleDataAsync(ApplicationDbContext context)
        {
            // ?? ???? ?? ??? ????? seed ??? ?? ??
            if (await context.Organizations.AnyAsync())
            {
                return; // ????? seed ???
            }

            // 1. ?????????
            var organization1 = new Organization
            {
                Name = "TechCorp Solutions",
                Description = "A leading technology solutions provider",
                Email = "contact@techcorp.com",
                PhoneNumber = "+1-555-0100",
                Address = "123 Tech Street, Silicon Valley, CA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var organization2 = new Organization
            {
                Name = "Business Dynamics Inc",
                Description = "Professional business consulting and services",
                Email = "info@bizodynamics.com",
                PhoneNumber = "+1-555-0200",
                Address = "456 Business Ave, New York, NY",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Organizations.AddRangeAsync(organization1, organization2);
            await context.SaveChangesAsync();

            // 2. ???????
            var branch1 = new Branch
            {
                Name = "TechCorp Main Branch",
                Address = "123 Tech Street, Silicon Valley, CA",
                PhoneNumber = "+1-555-0101",
                OrganizationId = organization1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var branch2 = new Branch
            {
                Name = "TechCorp East Coast",
                Address = "789 East Street, Boston, MA",
                PhoneNumber = "+1-555-0102",
                OrganizationId = organization1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var branch3 = new Branch
            {
                Name = "Business Dynamics HQ",
                Address = "456 Business Ave, New York, NY",
                PhoneNumber = "+1-555-0201",
                OrganizationId = organization2.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Branches.AddRangeAsync(branch1, branch2, branch3);
            await context.SaveChangesAsync();

            // 3. ??????
            var team1 = new Team
            {
                Name = "Technical Support",
                Description = "Handles technical issues and software support",
                BranchId = branch1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var team2 = new Team
            {
                Name = "Customer Success",
                Description = "Ensures customer satisfaction and success",
                BranchId = branch1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var team3 = new Team
            {
                Name = "Consulting Team",
                Description = "Business consulting and advisory services",
                BranchId = branch3.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Teams.AddRangeAsync(team1, team2, team3);
            await context.SaveChangesAsync();

            // 4. ???????
            var user1 = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@techcorp.com",
                PhoneNumber = "+1-555-1001",
                PasswordHash = "hashed_password_123", // ?? production ???? hash ????? ????
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@techcorp.com",
                PhoneNumber = "+1-555-1002",
                PasswordHash = "hashed_password_456",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user3 = new User
            {
                FirstName = "Mike",
                LastName = "Wilson",
                Email = "mike.wilson@bizodynamics.com",
                PhoneNumber = "+1-555-2001",
                PasswordHash = "hashed_password_789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(user1, user2, user3);
            await context.SaveChangesAsync();

            // 5. ??????????
            var profile1 = new Profile
            {
                Bio = "Experienced technical support specialist with 5+ years in IT",
                Skills = "Technical Support, Troubleshooting, Software Installation",
                Certifications = "CompTIA A+, Microsoft Certified",
                Avatar = "/avatars/john-smith.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var profile2 = new Profile
            {
                Bio = "Customer success manager focused on client satisfaction",
                Skills = "Customer Relations, Project Management, Communication",
                Certifications = "Certified Customer Success Manager",
                Avatar = "/avatars/sarah-johnson.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var profile3 = new Profile
            {
                Bio = "Senior business consultant with MBA and 10+ years experience",
                Skills = "Business Strategy, Process Improvement, Analytics",
                Certifications = "MBA, PMP, Six Sigma Black Belt",
                Avatar = "/avatars/mike-wilson.jpg",
                CreatedAt = DateTime.UtcNow
            };

            await context.Profiles.AddRangeAsync(profile1, profile2, profile3);
            await context.SaveChangesAsync();

            // 6. ??????????
            var agent1 = new SupportAgent
            {
                AgentCode = "TC001",
                UserId = user1.Id,
                ProfileId = profile1.Id,
                TeamId = team1.Id,
                Department = "Technical Support",
                Position = "Senior Support Specialist",
                Specialization = "Software & Hardware Support",
                HireDate = DateTime.UtcNow.AddYears(-2),
                Salary = 65000,
                SupportLevel = 2, // Level 2
                MaxConcurrentTickets = 10,
                CurrentTicketCount = 0,
                IsActive = true,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            var agent2 = new SupportAgent
            {
                AgentCode = "TC002",
                UserId = user2.Id,
                ProfileId = profile2.Id,
                TeamId = team2.Id,
                Department = "Customer Success",
                Position = "Customer Success Manager",
                Specialization = "Customer Relations & Account Management",
                HireDate = DateTime.UtcNow.AddYears(-1),
                Salary = 70000,
                SupportLevel = 3, // Level 3
                MaxConcurrentTickets = 8,
                CurrentTicketCount = 0,
                IsActive = true,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            var agent3 = new SupportAgent
            {
                AgentCode = "BD001",
                UserId = user3.Id,
                ProfileId = profile3.Id,
                TeamId = team3.Id,
                Department = "Consulting",
                Position = "Senior Consultant",
                Specialization = "Business Process & Strategy",
                HireDate = DateTime.UtcNow.AddYears(-3),
                Salary = 95000,
                SupportLevel = 3, // Level 3
                MaxConcurrentTickets = 5,
                CurrentTicketCount = 0,
                IsActive = true,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.SupportAgents.AddRangeAsync(agent1, agent2, agent3);
            await context.SaveChangesAsync();

            // 7. ????????? ???????
            var category1 = new TicketCategory
            {
                Name = "Technical Issues",
                Description = "Software bugs, system errors, and technical problems",
                ColorCode = "#FF5722",
                OrganizationId = organization1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var category2 = new TicketCategory
            {
                Name = "Feature Requests",
                Description = "New feature requests and enhancements",
                ColorCode = "#2196F3",
                OrganizationId = organization1.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var category3 = new TicketCategory
            {
                Name = "Business Consulting",
                Description = "Business process and strategy consultation",
                ColorCode = "#4CAF50",
                OrganizationId = organization2.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.TicketCategories.AddRangeAsync(category1, category2, category3);
            await context.SaveChangesAsync();

            // 8. ????????? ????
            var ticketStates = new[]
            {
                new TicketState
                {
                    Name = "New",
                    Description = "Newly created ticket",
                    ColorCode = "#9E9E9E",
                    Order = 1,
                    IsDefault = true,
                    IsFinal = false,
                    CreatedAt = DateTime.UtcNow
                },
                new TicketState
                {
                    Name = "In Progress",
                    Description = "Ticket is being worked on",
                    ColorCode = "#FF9800",
                    Order = 2,
                    IsDefault = false,
                    IsFinal = false,
                    CreatedAt = DateTime.UtcNow
                },
                new TicketState
                {
                    Name = "Resolved",
                    Description = "Ticket has been resolved",
                    ColorCode = "#4CAF50",
                    Order = 3,
                    IsDefault = false,
                    IsFinal = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TicketState
                {
                    Name = "Closed",
                    Description = "Ticket is closed",
                    ColorCode = "#607D8B",
                    Order = 4,
                    IsDefault = false,
                    IsFinal = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.TicketStates.AddRangeAsync(ticketStates);
            await context.SaveChangesAsync();

            // 9. ???????
            var customer1 = new Customer
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@email.com",
                PhoneNumber = "+1-555-3001",
                Address = "123 Customer St, Customer City, CC",
                CompanyName = "Brown Industries",
                OrganizationId = organization1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var customer2 = new Customer
            {
                FirstName = "Bob",
                LastName = "Davis",
                Email = "bob.davis@email.com",
                PhoneNumber = "+1-555-3002",
                Address = "456 Client Ave, Client Town, CT",
                CompanyName = "Davis Corp",
                OrganizationId = organization1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var customer3 = new Customer
            {
                FirstName = "Carol",
                LastName = "Miller",
                Email = "carol.miller@email.com",
                PhoneNumber = "+1-555-4001",
                Address = "789 Business Blvd, Business City, BC",
                CompanyName = "Miller Enterprises",
                OrganizationId = organization2.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.Customers.AddRangeAsync(customer1, customer2, customer3);
            await context.SaveChangesAsync();

            // 10. Subscriptions
            var subscription1 = new Subscription
            {
                Name = "TechCorp Pro Plan",
                Description = "Professional technical support with priority handling",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(335), // 1 year from 30 days ago
                Price = 999.99m,
                Currency = "USD",
                BillingCycleDays = 365,
                Status = SubscriptionStatus.Active,
                OrganizationId = organization1.Id,
                IsActive = true,
                Features = "{\"priority_support\": true, \"max_tickets\": 100, \"response_time\": \"2h\"}",
                CreatedAt = DateTime.UtcNow
            };

            var subscription2 = new Subscription
            {
                Name = "Business Consulting Package",
                Description = "Comprehensive business consulting services",
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(305), // 1 year from 60 days ago
                Price = 2499.99m,
                Currency = "USD",
                BillingCycleDays = 365,
                Status = SubscriptionStatus.Active,
                OrganizationId = organization2.Id,
                IsActive = true,
                Features = "{\"consulting_hours\": 50, \"priority_scheduling\": true, \"dedicated_consultant\": true}",
                CreatedAt = DateTime.UtcNow
            };

            await context.Subscriptions.AddRangeAsync(subscription1, subscription2);
            await context.SaveChangesAsync();

            // 11. API Keys
            var apiKey1 = new ApiKey
            {
                KeyName = "TechCorp Production API",
                KeyValue = "tc_prod_" + Guid.NewGuid().ToString().Replace("-", "")[..24],
                KeyHash = "hash_" + Guid.NewGuid().ToString().Replace("-", "")[..32], // ???? ???
                OrganizationId = organization1.Id,
                Description = "Production API key for TechCorp",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                Permissions = "read,write,tickets,customers",
                CreatedAt = DateTime.UtcNow
            };

            var apiKey2 = new ApiKey
            {
                KeyName = "Business Dynamics API",
                KeyValue = "bd_prod_" + Guid.NewGuid().ToString().Replace("-", "")[..24],
                KeyHash = "hash_" + Guid.NewGuid().ToString().Replace("-", "")[..32], // ???? ???
                OrganizationId = organization2.Id,
                Description = "API key for Business Dynamics integration",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddYears(1),
                Permissions = "read,write,consulting,analytics",
                CreatedAt = DateTime.UtcNow
            };

            await context.ApiKeys.AddRangeAsync(apiKey1, apiKey2);
            await context.SaveChangesAsync();

            Console.WriteLine("? Sample data seeded successfully!");
            Console.WriteLine($"Created {await context.Organizations.CountAsync()} organizations");
            Console.WriteLine($"Created {await context.Customers.CountAsync()} customers");
            Console.WriteLine($"Created {await context.SupportAgents.CountAsync()} support agents");
            Console.WriteLine($"Created {await context.TicketCategories.CountAsync()} ticket categories");
        }
    }
}