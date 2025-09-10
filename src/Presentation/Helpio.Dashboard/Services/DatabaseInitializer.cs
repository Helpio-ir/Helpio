using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Create default roles
            await CreateRoleIfNotExistsAsync(roleManager, "Admin");
            await CreateRoleIfNotExistsAsync(roleManager, "Manager");
            await CreateRoleIfNotExistsAsync(roleManager, "Agent");

            // Create default organization structure
            var organization = await CreateDefaultOrganizationAsync(context);
            var branch = await CreateDefaultBranchAsync(context, organization);
            var team = await CreateDefaultTeamAsync(context, branch);

            // Create default admin user
            var adminUser = await CreateDefaultAdminUserAsync(userManager);
            
            // Create SupportAgent for admin user
            if (adminUser != null)
            {
                await CreateDefaultSupportAgentAsync(context, adminUser, team);
            }

            // Create sample manager user
            var managerUser = await CreateSampleManagerUserAsync(userManager);
            if (managerUser != null)
            {
                await CreateManagerSupportAgentAsync(context, managerUser, team);
            }

            // Create sample agent user
            var agentUser = await CreateSampleAgentUserAsync(userManager);
            if (agentUser != null)
            {
                await CreateAgentSupportAgentAsync(context, agentUser, team);
            }
        }

        private static async Task CreateRoleIfNotExistsAsync(RoleManager<IdentityRole<int>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        private static async Task<Organization> CreateDefaultOrganizationAsync(ApplicationDbContext context)
        {
            var existingOrg = await context.Organizations.FirstOrDefaultAsync(o => o.Name == "شرکت نمونه هلپیو");
            if (existingOrg != null)
                return existingOrg;

            var organization = new Organization
            {
                Name = "شرکت نمونه هلپیو",
                Description = "سازمان نمونه برای تست سیستم",
                Email = "info@helpio.ir",
                PhoneNumber = "021-12345678",
                Address = "تهران، میدان ولیعصر",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
            return organization;
        }

        private static async Task<Branch> CreateDefaultBranchAsync(ApplicationDbContext context, Organization organization)
        {
            var existingBranch = await context.Branches.FirstOrDefaultAsync(b => b.OrganizationId == organization.Id);
            if (existingBranch != null)
                return existingBranch;

            var branch = new Branch
            {
                OrganizationId = organization.Id,
                Name = "شاخه مرکزی",
                Address = "تهران، میدان ولیعصر",
                PhoneNumber = "021-12345678",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Branches.Add(branch);
            await context.SaveChangesAsync();
            return branch;
        }

        private static async Task<Team> CreateDefaultTeamAsync(ApplicationDbContext context, Branch branch)
        {
            var existingTeam = await context.Teams.FirstOrDefaultAsync(t => t.BranchId == branch.Id);
            if (existingTeam != null)
                return existingTeam;

            var team = new Team
            {
                BranchId = branch.Id,
                Name = "تیم پشتیبانی فنی",
                Description = "تیم پاسخگویی به سوالات فنی مشتریان",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();
            return team;
        }

        private static async Task<User?> CreateDefaultAdminUserAsync(UserManager<User> userManager)
        {
            const string adminEmail = "admin@helpio.ir";
            const string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "مدیر",
                    LastName = "سیستم",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    return adminUser;
                }
            }
            return adminUser;
        }

        private static async Task<User?> CreateSampleManagerUserAsync(UserManager<User> userManager)
        {
            const string managerEmail = "manager@helpio.ir";
            const string managerPassword = "Manager123!";

            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new User
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FirstName = "احمد",
                    LastName = "مدیری",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(managerUser, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                    return managerUser;
                }
            }
            return managerUser;
        }

        private static async Task<User?> CreateSampleAgentUserAsync(UserManager<User> userManager)
        {
            const string agentEmail = "agent@helpio.ir";
            const string agentPassword = "Agent123!";

            var agentUser = await userManager.FindByEmailAsync(agentEmail);
            if (agentUser == null)
            {
                agentUser = new User
                {
                    UserName = agentEmail,
                    Email = agentEmail,
                    FirstName = "مریم",
                    LastName = "پشتیبان",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(agentUser, agentPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agentUser, "Agent");
                    return agentUser;
                }
            }
            return agentUser;
        }

        private static async Task CreateDefaultSupportAgentAsync(ApplicationDbContext context, User user, Team team)
        {
            var existingAgent = await context.SupportAgents.FirstOrDefaultAsync(sa => sa.UserId == user.Id);
            if (existingAgent != null)
                return;

            // Create default profile first
            var profile = new Profile
            {
                Avatar = "default-admin.png",
                Bio = "مدیر کل سیستم",
                Skills = "مدیریت,پشتیبانی,فنی",
                Certifications = "ITIL,PMP",
                CreatedAt = DateTime.UtcNow
            };

            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var supportAgent = new SupportAgent
            {
                UserId = user.Id,
                TeamId = team.Id,
                ProfileId = profile.Id,
                AgentCode = "ADM001",
                HireDate = DateTime.UtcNow,
                Department = "مدیریت",
                Position = "مدیر کل",
                Specialization = "مدیریت و نظارت",
                SupportLevel = 3,
                Salary = 0, // Admin salary is confidential
                IsActive = true,
                IsAvailable = true,
                MaxConcurrentTickets = 100,
                CurrentTicketCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            context.SupportAgents.Add(supportAgent);
            await context.SaveChangesAsync();

            // Update team to set this admin as team lead
            team.TeamLeadId = supportAgent.Id;
            team.SupervisorId = supportAgent.Id;
            await context.SaveChangesAsync();
        }

        private static async Task CreateManagerSupportAgentAsync(ApplicationDbContext context, User user, Team team)
        {
            var existingAgent = await context.SupportAgents.FirstOrDefaultAsync(sa => sa.UserId == user.Id);
            if (existingAgent != null)
                return;

            var profile = new Profile
            {
                Avatar = "default-manager.png",
                Bio = "مدیر تیم پشتیبانی",
                Skills = "مدیریت تیم,پشتیبانی,ارتباطات",
                Certifications = "Management,Customer Service",
                CreatedAt = DateTime.UtcNow
            };

            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var supportAgent = new SupportAgent
            {
                UserId = user.Id,
                TeamId = team.Id,
                ProfileId = profile.Id,
                AgentCode = "MGR001",
                HireDate = DateTime.UtcNow,
                Department = "پشتیبانی",
                Position = "مدیر تیم",
                Specialization = "مدیریت و نظارت",
                SupportLevel = 2,
                Salary = 15000000,
                IsActive = true,
                IsAvailable = true,
                MaxConcurrentTickets = 50,
                CurrentTicketCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            context.SupportAgents.Add(supportAgent);
            await context.SaveChangesAsync();
        }

        private static async Task CreateAgentSupportAgentAsync(ApplicationDbContext context, User user, Team team)
        {
            var existingAgent = await context.SupportAgents.FirstOrDefaultAsync(sa => sa.UserId == user.Id);
            if (existingAgent != null)
                return;

            var profile = new Profile
            {
                Avatar = "default-agent.png",
                Bio = "کارشناس پشتیبانی",
                Skills = "پشتیبانی فنی,حل مشکل,ارتباطات",
                Certifications = "Customer Support",
                CreatedAt = DateTime.UtcNow
            };

            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var supportAgent = new SupportAgent
            {
                UserId = user.Id,
                TeamId = team.Id,
                ProfileId = profile.Id,
                AgentCode = "AGT001",
                HireDate = DateTime.UtcNow,
                Department = "پشتیبانی",
                Position = "کارشناس پشتیبانی",
                Specialization = "پشتیبانی عمومی",
                SupportLevel = 1,
                Salary = 8000000,
                IsActive = true,
                IsAvailable = true,
                MaxConcurrentTickets = 20,
                CurrentTicketCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            context.SupportAgents.Add(supportAgent);
            await context.SaveChangesAsync();
        }
    }
}