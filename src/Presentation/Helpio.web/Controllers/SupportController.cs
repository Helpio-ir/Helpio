using Microsoft.AspNetCore.Mvc;
using Helpio.web.Models;

namespace Helpio.web.Controllers;

public class SupportController : Controller
{
    private readonly ILogger<SupportController> _logger;

    public SupportController(ILogger<SupportController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult SubmitTicket()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SubmitTicket(TicketSubmissionModel model)
    {
        if (ModelState.IsValid)
        {
            // TODO: Process ticket submission
            _logger.LogInformation("New ticket submitted: {Subject} by {Email}", model.Subject, model.Email);
            ViewBag.Message = "Your ticket has been submitted successfully!";
            return View("TicketSubmitted");
        }
        return View(model);
    }

    public IActionResult KnowledgeBase()
    {
        return View();
    }

    public IActionResult FAQ()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View(new ContactFormModel());
    }

    [HttpPost]
    public IActionResult Contact(ContactFormModel model)
    {
        if (ModelState.IsValid)
        {
            // TODO: Process contact form submission
            _logger.LogInformation("New contact form submitted: {Subject} by {Email}", model.Subject, model.Email);
            
            TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you within 24 hours.";
            return RedirectToAction("Contact");
        }
        return View(model);
    }
}