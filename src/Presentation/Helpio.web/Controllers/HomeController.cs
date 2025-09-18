using System.Diagnostics;
using Helpio.web.Models;
using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Business;

namespace Helpio.web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPlanService _planService;

    public HomeController(ILogger<HomeController> logger, IPlanService planService)
    {
        _logger = logger;
        _planService = planService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Helpio - Professional Customer Support Platform";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Features()
    {
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
