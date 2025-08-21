using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public class NoteDto : BaseDto
    {
        public int TicketId { get; set; }
        public int? SupportAgentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsSystemNote { get; set; }
        public bool IsPrivate { get; set; }
        
        // Navigation DTOs
        public TicketDto? Ticket { get; set; }
        public SupportAgentDto? SupportAgent { get; set; }
        
        // Computed Properties
        public string AuthorName => SupportAgent?.User?.FullName ?? "?????";
        public string NoteType => IsSystemNote ? "?????" : IsPrivate ? "?????" : "?????";
    }

    public class CreateNoteDto
    {
        public int TicketId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
    }

    public class UpdateNoteDto
    {
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
    }
}