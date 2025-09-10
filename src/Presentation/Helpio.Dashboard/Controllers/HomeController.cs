using System.Diagnostics;
using Helpio.Dashboard.Models;
using Helpio.Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpio.Dashboard.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, ICurrentUserContext userContext)
        : base(userContext)
    {
        _logger = logger;
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
