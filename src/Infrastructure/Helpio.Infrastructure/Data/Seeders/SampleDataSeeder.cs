using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Entities.Knowledge;
using Microsoft.AspNetCore.Identity;

namespace Helpio.Ir.Infrastructure.Data.Seeders
{
    public static class SampleDataSeeder
    {
        public static async Task SeedSampleDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // اطمینان از وجود پایگاه داده
            await context.Database.EnsureCreatedAsync();

            // بررسی اینکه آیا داده‌های نمونه قبلاً ایجاد شده‌اند یا نه
            if (await context.Organizations.AnyAsync())
            {
                return; // داده‌ها قبلاً ایجاد شده‌اند
            }

            // ایجاد سازمان نمونه
            var organization = new Organization
            {
                Name = "شرکت نمونه",
                Description = "شرکت نمونه برای تست سیستم",
                Email = "info@sample.com", // اصلاح شده
                PhoneNumber = "021-12345678", // اصلاح شده
                Address = "تهران، خیابان ولیعصر، پلاک ۱۲۳",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(organization);
            await context.SaveChangesAsync();

            // ایجاد شعبه نمونه
            var branch = new Branch
            {
                Name = "شعبه اصلی",
                OrganizationId = organization.Id,
                Address = "تهران، خیابان ولیعصر، پلاک ۱۲۳",
                PhoneNumber = "021-12345678", // اصلاح شده
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Branches.Add(branch);
            await context.SaveChangesAsync();

            // ایجاد تیم نمونه
            var team = new Team
            {
                Name = "تیم پشتیبانی",
                Description = "تیم پشتیبانی فنی",
                BranchId = branch.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            // ایجاد کاربران نمونه
            var adminUser = new User
            {
                UserName = "admin@helpio.ir",
                Email = "admin@helpio.ir",
                FirstName = "مدیر",
                LastName = "سیستم",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var managerUser = new User
            {
                UserName = "manager@helpio.ir",
                Email = "manager@helpio.ir",
                FirstName = "مدیر",
                LastName = "سازمان",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            result = await userManager.CreateAsync(managerUser, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }

            var agentUser = new User
            {
                UserName = "agent@helpio.ir",
                Email = "agent@helpio.ir",
                FirstName = "کارشناس",
                LastName = "پشتیبانی",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            result = await userManager.CreateAsync(agentUser, "Agent123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agentUser, "Agent");
            }

            // ایجاد SupportAgent
            var supportAgent = new SupportAgent
            {
                UserId = agentUser.Id,
                TeamId = team.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.SupportAgents.Add(supportAgent);
            await context.SaveChangesAsync();

            // ایجاد مشتری نمونه
            var customer = new Customer
            {
                FirstName = "علی",
                LastName = "احمدی",
                Email = "ali@example.com",
                PhoneNumber = "09123456789",
                CompanyName = "شرکت مشتری",
                OrganizationId = organization.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            // ایجاد دسته‌بندی تیکت
            var ticketCategory = new TicketCategory
            {
                Name = "پشتیبانی فنی",
                Description = "مسائل فنی و پشتیبانی نرم‌افزار",
                OrganizationId = organization.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.TicketCategories.Add(ticketCategory);
            await context.SaveChangesAsync();

            // ایجاد وضعیت‌های تیکت
            var ticketStates = new[]
            {
                new TicketState { Name = "Open", Description = "باز", CreatedAt = DateTime.UtcNow },
                new TicketState { Name = "In Progress", Description = "در حال پیگیری", CreatedAt = DateTime.UtcNow },
                new TicketState { Name = "Closed", Description = "بسته شده", CreatedAt = DateTime.UtcNow },
                new TicketState { Name = "Pending", Description = "در انتظار", CreatedAt = DateTime.UtcNow }
            };

            context.TicketStates.AddRange(ticketStates);
            await context.SaveChangesAsync();

            // ایجاد اشتراک فریمیوم
            var freemiumSubscription = new Subscription
            {
                Name = "Freemium Plan",
                Description = "طرح رایگان با محدودیت ۵۰ تیکت در ماه",
                StartDate = DateTime.UtcNow.AddDays(-15), // شروع ۱۵ روز پیش
                EndDate = null, // فریمیوم منقضی نمی‌شود
                Price = 0,
                Currency = "IRR",
                BillingCycleDays = 30,
                Status = SubscriptionStatus.Active,
                PlanType = SubscriptionPlanType.Freemium,
                OrganizationId = organization.Id,
                IsActive = true,
                MonthlyTicketLimit = 50,
                CurrentMonthTicketCount = 25, // ۲۵ تیکت استفاده شده برای تست
                CurrentMonthStartDate = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day),
                CreatedAt = DateTime.UtcNow
            };

            context.Subscriptions.Add(freemiumSubscription);
            await context.SaveChangesAsync();

            // ایجاد چند تیکت نمونه برای تست محدودیت‌ها
            var sampleTickets = new List<Ticket>();
            for (int i = 1; i <= 25; i++)
            {
                var ticket = new Ticket
                {
                    Title = $"تیکت نمونه شماره {i}",
                    Description = $"توضیحات تیکت نمونه شماره {i} برای تست سیستم",
                    CustomerId = customer.Id,
                    TicketCategoryId = ticketCategory.Id,
                    TeamId = team.Id,
                    Priority = (TicketPriority)(i % 4 + 1), // تنوع در اولویت
                    TicketStateId = ticketStates[i % 3].Id, // تنوع در وضعیت
                    SupportAgentId = i % 3 == 0 ? supportAgent.Id : null, // بعضی اختصاص داده شده
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 15)), // در ۱۵ روز گذشته
                    UpdatedAt = DateTime.UtcNow
                };

                sampleTickets.Add(ticket);
            }

            context.Tickets.AddRange(sampleTickets);
            await context.SaveChangesAsync();

            // ایجاد پاسخ‌های آماده
            var cannedResponses = new[]
            {
                new CannedResponse
                {
                    Name = "خوش‌آمدگویی",
                    Content = "سلام و درود، با تشکر از تماس شما. ما در اسرع وقت به درخواست شما رسیدگی خواهیم کرد.",
                    OrganizationId = organization.Id,
                    UsageCount = 10,
                    CreatedAt = DateTime.UtcNow
                },
                new CannedResponse
                {
                    Name = "درخواست اطلاعات بیشتر",
                    Content = "برای بررسی بهتر مسئله شما، لطفاً اطلاعات تکمیلی‌تری ارائه دهید.",
                    OrganizationId = organization.Id,
                    UsageCount = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new CannedResponse
                {
                    Name = "حل مسئله",
                    Content = "مسئله شما حل شد. در صورت داشتن سوال یا مسئله جدید، لطفاً با ما تماس بگیرید.",
                    OrganizationId = organization.Id,
                    UsageCount = 20,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.CannedResponses.AddRange(cannedResponses);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ داده‌های نمونه با موفقیت ایجاد شدند:");
            Console.WriteLine($"   📋 سازمان: {organization.Name}");
            Console.WriteLine($"   🏢 شعبه: {branch.Name}");
            Console.WriteLine($"   👥 تیم: {team.Name}");
            Console.WriteLine($"   👤 کاربران: Admin, Manager, Agent");
            Console.WriteLine($"   💳 اشتراک: Freemium ({freemiumSubscription.CurrentMonthTicketCount}/{freemiumSubscription.MonthlyTicketLimit} تیکت استفاده شده)");
            Console.WriteLine($"   🎫 تیکت‌ها: {sampleTickets.Count} تیکت نمونه");
            Console.WriteLine($"   📝 پاسخ‌های آماده: {cannedResponses.Length} پاسخ");
        }
    }
}