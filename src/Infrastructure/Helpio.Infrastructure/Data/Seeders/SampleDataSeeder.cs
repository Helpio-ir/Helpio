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

            try
            {
                // چک کردن و ایجاد سازمان نمونه
                var organization = await context.Organizations.FirstOrDefaultAsync(o => o.Name == "شرکت نمونه");
                if (organization == null)
                {
                    organization = new Organization
                    {
                        Name = "شرکت نمونه",
                        Description = "شرکت نمونه برای تست سیستم",
                        Email = "info@sample.com",
                        PhoneNumber = "021-12345678",
                        Address = "تهران، خیابان ولیعصر، پلاک ۱۲۳",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Organizations.Add(organization);
                    await context.SaveChangesAsync();
                }

                // چک کردن و ایجاد شعبه نمونه
                var branch = await context.Branches.FirstOrDefaultAsync(b => b.OrganizationId == organization.Id);
                if (branch == null)
                {
                    branch = new Branch
                    {
                        Name = "شعبه اصلی",
                        OrganizationId = organization.Id,
                        Address = "تهران، خیابان ولیعصر، پلاک ۱۲۳",
                        PhoneNumber = "021-12345678",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Branches.Add(branch);
                    await context.SaveChangesAsync();
                }

                // چک کردن و ایجاد تیم نمونه
                var team = await context.Teams.FirstOrDefaultAsync(t => t.BranchId == branch.Id);
                if (team == null)
                {
                    team = new Team
                    {
                        Name = "تیم پشتیبانی",
                        Description = "تیم پشتیبانی فنی",
                        BranchId = branch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Teams.Add(team);
                    await context.SaveChangesAsync();
                }

                // چک کردن و ایجاد کاربران نمونه
                var adminUser = await userManager.FindByEmailAsync("admin@helpio.ir");
                if (adminUser == null)
                {
                    adminUser = new User
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
                }

                var agentUser = await userManager.FindByEmailAsync("agent@helpio.ir");
                if (agentUser == null)
                {
                    agentUser = new User
                    {
                        UserName = "agent@helpio.ir",
                        Email = "agent@helpio.ir",
                        FirstName = "کارشناس",
                        LastName = "پشتیبانی",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(agentUser, "Agent123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(agentUser, "Agent");
                    }
                }

                // چک کردن و ایجاد SupportAgent
                var supportAgent = await context.SupportAgents.FirstOrDefaultAsync(sa => sa.UserId == agentUser.Id);
                if (supportAgent == null)
                {
                    supportAgent = new SupportAgent
                    {
                        UserId = agentUser.Id,
                        TeamId = team.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.SupportAgents.Add(supportAgent);
                    await context.SaveChangesAsync();
                }

                // چک کردن و ایجاد مشتری نمونه
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.Email == "ali@example.com" && c.OrganizationId == organization.Id);
                if (customer == null)
                {
                    customer = new Customer
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
                }

                // چک کردن و ایجاد دسته‌بندی‌های تیکت
                var existingCategories = await context.TicketCategories
                    .Where(tc => tc.OrganizationId == organization.Id)
                    .ToListAsync();

                if (!existingCategories.Any())
                {
                    var ticketCategories = new[]
                    {
                        new TicketCategory
                        {
                            Name = "پشتیبانی فنی",
                            Description = "مسائل فنی و پشتیبانی نرم‌افزار",
                            OrganizationId = organization.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new TicketCategory
                        {
                            Name = "حسابداری و پرداخت",
                            Description = "مسائل مربوط به حسابداری و پرداخت‌ها",
                            OrganizationId = organization.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new TicketCategory
                        {
                            Name = "درخواست ویژگی جدید",
                            Description = "درخواست‌های مربوط به ویژگی‌های جدید",
                            OrganizationId = organization.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new TicketCategory
                        {
                            Name = "گزارش باگ",
                            Description = "گزارش خرابی‌ها و مشکلات سیستم",
                            OrganizationId = organization.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.TicketCategories.AddRange(ticketCategories);
                    await context.SaveChangesAsync();
                }

                // چک کردن و ایجاد وضعیت‌های تیکت
                var existingStates = await context.TicketStates.ToListAsync();
                if (!existingStates.Any())
                {
                    var ticketStates = new[]
                    {
                        new TicketState { Name = "Open", Description = "باز", CreatedAt = DateTime.UtcNow, IsDefault = true, Order = 1 },
                        new TicketState { Name = "In Progress", Description = "در حال پیگیری", CreatedAt = DateTime.UtcNow, Order = 2 },
                        new TicketState { Name = "Resolved", Description = "حل شده", CreatedAt = DateTime.UtcNow, Order = 3 },
                        new TicketState { Name = "Closed", Description = "بسته شده", CreatedAt = DateTime.UtcNow, IsFinal = true, Order = 4 },
                        new TicketState { Name = "Pending", Description = "در انتظار", CreatedAt = DateTime.UtcNow, Order = 5 }
                    };

                    context.TicketStates.AddRange(ticketStates);
                    await context.SaveChangesAsync();
                }

                // دریافت وضعیت‌های تیکت برای استفاده در تیکت‌های نمونه
                var allStates = await context.TicketStates.ToListAsync();
                var defaultState = allStates.FirstOrDefault(s => s.IsDefault) ?? allStates.First();

                // چک کردن و ایجاد چند تیکت نمونه برای تست
                var existingTickets = await context.Tickets
                    .Where(t => t.CustomerId == customer.Id)
                    .CountAsync();

                if (existingTickets == 0)
                {
                    var ticketCategory = await context.TicketCategories
                        .FirstOrDefaultAsync(tc => tc.OrganizationId == organization.Id);

                    if (ticketCategory != null)
                    {
                        var sampleTickets = new List<Ticket>();
                        for (int i = 1; i <= 5; i++) // فقط 5 تیکت نمونه
                        {
                            var ticket = new Ticket
                            {
                                Title = $"تیکت نمونه شماره {i}",
                                Description = $"توضیحات تیکت نمونه شماره {i} برای تست سیستم. این تیکت برای آزمایش کردن قابلیت‌های مختلف سیستم ایجاد شده است.",
                                CustomerId = customer.Id,
                                TicketCategoryId = ticketCategory.Id,
                                TeamId = team.Id,
                                Priority = (TicketPriority)(i % 4 + 1), // تنوع در اولویت
                                TicketStateId = allStates[i % allStates.Count].Id, // تنوع در وضعیت
                                SupportAgentId = i % 3 == 0 ? supportAgent.Id : null, // بعضی اختصاص داده شده
                                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 15)), // در ۱۵ روز گذشته
                                UpdatedAt = DateTime.UtcNow
                            };

                            sampleTickets.Add(ticket);
                        }

                        context.Tickets.AddRange(sampleTickets);
                        await context.SaveChangesAsync();
                    }
                }

                // چک کردن و ایجاد پاسخ‌های آماده
                var existingResponses = await context.CannedResponses
                    .Where(cr => cr.OrganizationId == organization.Id)
                    .ToListAsync();

                if (!existingResponses.Any())
                {
                    var cannedResponses = new[]
                    {
                        new CannedResponse
                        {
                            Name = "خوش‌آمدگویی",
                            Content = "سلام {{customer_name}}، با تشکر از تماس شما. ما در اسرع وقت به درخواست شما رسیدگی خواهیم کرد.",
                            OrganizationId = organization.Id,
                            UsageCount = 0,
                            CreatedAt = DateTime.UtcNow
                        },
                        new CannedResponse
                        {
                            Name = "درخواست اطلاعات بیشتر",
                            Content = "برای بررسی بهتر مسئله شما، لطفاً اطلاعات تکمیلی‌تری ارائه دهید:\n\n- نسخه نرم‌افزار\n- مرورگر استفاده شده\n- توضیح دقیق‌تر از مشکل",
                            OrganizationId = organization.Id,
                            UsageCount = 0,
                            CreatedAt = DateTime.UtcNow
                        },
                        new CannedResponse
                        {
                            Name = "حل مسئله",
                            Content = "مسئله شما حل شد. در صورت داشتن سوال یا مسئله جدید، لطفاً با ما تماس بگیرید.\n\nبا تشکر,\n{{agent_name}}",
                            OrganizationId = organization.Id,
                            UsageCount = 0,
                            CreatedAt = DateTime.UtcNow
                        },
                        new CannedResponse
                        {
                            Name = "ارجاع به تیم فنی",
                            Content = "تیکت شما به تیم فنی ارجاع داده شد. کارشناسان فنی در اسرع وقت با شما تماس خواهند گرفت.",
                            OrganizationId = organization.Id,
                            UsageCount = 0,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.CannedResponses.AddRange(cannedResponses);
                    await context.SaveChangesAsync();
                }

                Console.WriteLine("✅ داده‌های نمونه با موفقیت ایجاد/بروزرسانی شدند:");
                Console.WriteLine($"   📋 سازمان: {organization.Name}");
                Console.WriteLine($"   🏢 شعبه: {branch.Name}");
                Console.WriteLine($"   👥 تیم: {team.Name}");
                Console.WriteLine($"   👤 کاربران: Admin, Agent");
                Console.WriteLine($"   🗂️ دسته‌بندی‌ها: {await context.TicketCategories.CountAsync(tc => tc.OrganizationId == organization.Id)} دسته");
                Console.WriteLine($"   📊 وضعیت‌ها: {await context.TicketStates.CountAsync()} وضعیت");
                Console.WriteLine($"   📝 پاسخ‌های آماده: {await context.CannedResponses.CountAsync(cr => cr.OrganizationId == organization.Id)} پاسخ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در ایجاد داده‌های نمونه: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   جزئیات: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}