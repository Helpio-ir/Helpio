using System.Diagnostics;
using Helpio.Dashboard.Models;
using Helpio.Dashboard.Services;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Application.Services.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPlanService _planService;

    public HomeController(ILogger<HomeController> logger, ICurrentUserContext userContext, IPlanService planService)
        : base(userContext)
    {
        _logger = logger;
        _planService = planService;
    }

    public IActionResult Index()
    {
        // نمایش اطلاعات کاربر در صفحه اصلی
        ViewBag.WelcomeMessage = $"خوش آمدید {UserContext.UserFullName}";

        if (UserContext.CurrentOrganization != null)
        {
            ViewBag.OrganizationMessage = $"شما عضو سازمان '{UserContext.CurrentOrganization.Name}' هستید";
        }

        if (UserContext.CurrentTeam != null)
        {
            ViewBag.TeamMessage = $"شما عضو تیم '{UserContext.CurrentTeam.Name}' هستید";
        }

        return View();
    }

    public async Task<IActionResult> Pricing()
    {
        try
        {
            _logger.LogInformation("Loading pricing plans...");
            
            var planDtos = await _planService.GetPublicPlansAsync();
            
            _logger.LogInformation("Loaded {PlanCount} plans from database", planDtos?.Count() ?? 0);
            
            if (planDtos != null)
            {
                foreach (var plan in planDtos)
                {
                    _logger.LogInformation("Plan: {PlanName}, Type: {PlanType}, Price: {Price}", 
                        plan.Name, plan.Type, plan.Price);
                }
            }
            
            return View(planDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing plans");
            TempData["Error"] = "خطا در بارگذاری طرح‌های قیمت‌گذاری";
            return View(new List<Helpio.Ir.Application.DTOs.Business.PlanDto>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
