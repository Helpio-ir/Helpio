using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Infrastructure.Data.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Helpio.Dashboard.Middleware
{
    public static class DatabaseInitializer
    {
        public static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed roles first
                await SeedRolesAsync(roleManager);
                
                // Check if we have users already
                if (await userManager.Users.AnyAsync())
                {
                    Console.WriteLine("✅ کاربران قبلاً در سیستم وجود دارند.");
                    return;
                }

                // Seed sample data including organizations, users, subscriptions
                await SampleDataSeeder.SeedSampleDataAsync(serviceProvider);

                Console.WriteLine("✅ تمام داده‌های اولیه با موفقیت ایجاد شدند.");
                Console.WriteLine("");
                Console.WriteLine("🔐 اطلاعات ورود:");
                Console.WriteLine("   👨‍💼 Admin: admin@helpio.ir / Admin123!");
                Console.WriteLine("   👤 Manager: manager@helpio.ir / Manager123!");
                Console.WriteLine("   🔧 Agent: agent@helpio.ir / Agent123!");
                Console.WriteLine("");
                Console.WriteLine("💡 برای تست محدودیت فریمیوم:");
                Console.WriteLine("   - ۲۵ تیکت از ۵۰ تیکت مجاز استفاده شده");
                Console.WriteLine("   - ۲۵ تیکت باقی‌مانده برای ایجاد");
                Console.WriteLine("   - پس از رسیدن به ۵۰ تیکت، ایجاد تیکت جدید محدود می‌شود");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در ایجاد داده‌های اولیه: {ex.Message}");
                // Don't throw - let the application continue
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            var roles = new[] { "Admin", "Manager", "Agent" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }
    }
}