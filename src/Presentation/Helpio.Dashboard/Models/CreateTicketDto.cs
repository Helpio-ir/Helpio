using System.ComponentModel.DataAnnotations;
using Helpio.Ir.Domain.Entities.Ticketing;

namespace Helpio.Dashboard.Models
{
    public class CreateTicketDto
    {
        [Required(ErrorMessage = "عنوان تیکت الزامی است")]
        [StringLength(200, ErrorMessage = "عنوان تیکت نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "شرح مسئله الزامی است")]
        [StringLength(5000, ErrorMessage = "شرح مسئله نمی‌تواند بیش از 5000 کاراکتر باشد")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "انتخاب مشتری الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "لطفا مشتری را انتخاب کنید")]
        public int CustomerId { get; set; }
        
        [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "لطفا دسته‌بندی را انتخاب کنید")]
        public int TicketCategoryId { get; set; }
        
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;
        
        public DateTime? DueDate { get; set; }
        
        [Range(0, 1000, ErrorMessage = "ساعات تخمینی باید بین 0 تا 1000 باشد")]
        public decimal EstimatedHours { get; set; }
        
        // Optional for Admin users
        public int? TeamId { get; set; }
    }
}