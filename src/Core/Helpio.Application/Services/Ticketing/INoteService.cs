using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public interface INoteService
    {
        Task<NoteDto?> GetByIdAsync(int id);
        Task<PaginatedResult<NoteDto>> GetNotesAsync(PaginationRequest request);
        Task<NoteDto> CreateAsync(CreateNoteDto createDto);
        Task<NoteDto> UpdateAsync(int id, UpdateNoteDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<NoteDto>> GetByTicketIdAsync(int ticketId);
        Task<IEnumerable<NoteDto>> GetBySupportAgentIdAsync(int supportAgentId);
        Task<IEnumerable<NoteDto>> GetSystemNotesAsync(int ticketId);
        Task<IEnumerable<NoteDto>> GetPrivateNotesAsync(int ticketId);
        Task<IEnumerable<NoteDto>> GetPublicNotesAsync(int ticketId);
        Task<NoteDto> CreateSystemNoteAsync(int ticketId, string description);
    }
}