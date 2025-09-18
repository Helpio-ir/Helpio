using System.ComponentModel.DataAnnotations;

namespace Helpio.Dashboard.Models
{
    public class ContactSalesViewModel
    {
        public int? PlanId { get; set; }
        
        [Required(ErrorMessage = "نام سازمان الزامی است")]
        [Display(Name = "نام سازمان")]
        public string OrganizationName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        [Display(Name = "نام و نام خانوادگی")]
        public string ContactName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        [Display(Name = "ایمیل")]
        public string ContactEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
        [Display(Name = "شماره تلفن")]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "تعداد تقریبی تیکت در ماه")]
        [Range(1, int.MaxValue, ErrorMessage = "تعداد تیکت باید بیشتر از صفر باشد")]
        public int? EstimatedMonthlyTickets { get; set; }
        
        [Display(Name = "اندازه تیم")]
        [Range(1, int.MaxValue, ErrorMessage = "اندازه تیم باید بیشتر از صفر باشد")]
        public int? TeamSize { get; set; }
        
        [Display(Name = "توضیحات اضافی")]
        [MaxLength(1000, ErrorMessage = "توضیحات نباید بیشتر از ۱۰۰۰ کاراکتر باشد")]
        public string? AdditionalNotes { get; set; }
        
        [Display(Name = "زمان مناسب برای تماس")]
        public string? PreferredContactTime { get; set; }
    }
}