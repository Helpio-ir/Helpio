using Helpio.Dashboard.Middleware;
using Helpio.Dashboard.Services;
using Helpio.Ir.Application.Mappings;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Infrastructure.Repositories;
using Helpio.Ir.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Disable Hot Reload and Browser Link completely

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Add Entity Framework
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register IApplicationDbContext
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Add Identity
        builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure application cookie
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
        });

        // Add custom services
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IOrganizationService, OrganizationService>();
        builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        builder.Services.AddScoped<IVariableReplacementService, VariableReplacementService>();
        
        // Add Infrastructure services
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<IDateTime, DateTimeService>();
        
        // Add Application services
        builder.Services.AddScoped<ISubscriptionLimitService, SubscriptionLimitService>();
        builder.Services.AddScoped<IPlanService, PlanService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IInvoiceService, InvoiceService>();

        // Add AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile)); // Add AutoMapper

        // Add Seeders
        builder.Services.AddScoped<Helpio.Ir.Infrastructure.Data.Seeders.PlanSeeder>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            // Use developer exception page without hot reload
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Add user context middleware AFTER authentication
        app.UseUserContext();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Dashboard}/{action=Index}/{id?}");

        // Initialize database with default users
        await Services.DatabaseInitializer.SeedDefaultUsersAsync(app.Services);

        // Seed default plans
        using (var scope = app.Services.CreateScope())
        {
            var planSeeder = scope.ServiceProvider.GetRequiredService<Helpio.Ir.Infrastructure.Data.Seeders.PlanSeeder>();
            await planSeeder.SeedAsync();
        }

        await app.RunAsync();
    }
}
