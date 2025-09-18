using System.ComponentModel.DataAnnotations;

namespace Helpio.web.Models;

public class TicketSubmissionModel
{
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required")]
    [Display(Name = "Priority")]
    public TicketPriority Priority { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public TicketCategory Category { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [Display(Name = "Description")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Company")]
    public string? Company { get; set; }

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
}

public enum TicketPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TicketCategory
{
    Technical = 1,
    Billing = 2,
    General = 3,
    FeatureRequest = 4,
    BugReport = 5
}