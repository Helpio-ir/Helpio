using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public interface IResponseService
    {
        Task<ResponseDto?> GetByIdAsync(int id);
        Task<PaginatedResult<ResponseDto>> GetResponsesAsync(PaginationRequest request);
        Task<ResponseDto> CreateAsync(CreateResponseDto createDto);
        Task<ResponseDto> UpdateAsync(int id, UpdateResponseDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ResponseDto>> GetByTicketIdAsync(int ticketId);
        Task<IEnumerable<ResponseDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ResponseDto>> GetCustomerResponsesAsync(int ticketId);
        Task<IEnumerable<ResponseDto>> GetAgentResponsesAsync(int ticketId);
        Task<IEnumerable<ResponseDto>> GetUnreadResponsesAsync(int ticketId);
        Task<ResponseDto?> GetLatestResponseAsync(int ticketId);
        Task<bool> MarkAsReadAsync(int responseId);
    }
}