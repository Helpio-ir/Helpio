using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public class ResponseDto : BaseDto
    {
        public int TicketId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFromCustomer { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Navigation DTOs
        public TicketDto? Ticket { get; set; }
        public UserDto? User { get; set; }
        
        // Status
        public bool IsRead => ReadAt.HasValue;
        public string AuthorName => User?.FullName ?? "?????";
    }

    public class CreateResponseDto
    {
        public int TicketId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFromCustomer { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class UpdateResponseDto
    {
        public string Content { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
    }
}