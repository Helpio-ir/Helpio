using System.ComponentModel.DataAnnotations;

namespace Helpio.web.Models;

public class ContactFormModel
{
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Company")]
    public string? Company { get; set; }

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Subject is required")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Inquiry type is required")]
    [Display(Name = "Inquiry Type")]
    public InquiryType InquiryType { get; set; }

    [Display(Name = "Priority")]
    public ContactPriority Priority { get; set; } = ContactPriority.Medium;

    [Required(ErrorMessage = "Message is required")]
    [Display(Name = "Message")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; set; } = string.Empty;
}

public enum InquiryType
{
    General = 1,
    Technical = 2,
    Billing = 3,
    Sales = 4,
    Partnership = 5,
    Feedback = 6
}

public enum ContactPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}