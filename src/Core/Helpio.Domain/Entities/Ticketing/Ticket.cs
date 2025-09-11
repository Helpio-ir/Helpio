using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public enum TicketPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public class Ticket : BaseEntity
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
        
        public int TicketStateId { get; set; }
        
        // TeamId will be set by system, so no validation required from user
        public int TeamId { get; set; }
        
        public int? SupportAgentId { get; set; }
        
        [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "لطفا دسته‌بندی را انتخاب کنید")]
        public int TicketCategoryId { get; set; }
        
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(2000, ErrorMessage = "راه‌حل نمی‌تواند بیش از 2000 کاراکتر باشد")]
        public string? Resolution { get; set; }
        
        [Range(0, 1000, ErrorMessage = "ساعات تخمینی باید بین 0 تا 1000 باشد")]
        public decimal EstimatedHours { get; set; }
        
        [Range(0, 1000, ErrorMessage = "ساعات واقعی باید بین 0 تا 1000 باشد")]
        public decimal ActualHours { get; set; }
        
        // Navigation properties
        public virtual Core.Customer Customer { get; set; } = null!;
        public virtual TicketState TicketState { get; set; } = null!;
        public virtual Core.Team Team { get; set; } = null!;
        public virtual Core.SupportAgent? SupportAgent { get; set; }
        public virtual TicketCategory TicketCategory { get; set; } = null!;
        public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}