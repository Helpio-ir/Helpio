using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Ticketing;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public class ResponseService : IResponseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ResponseService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public ResponseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ResponseService> logger,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<ResponseDto?> GetByIdAsync(int id)
        {
            var response = await _unitOfWork.Responses.GetByIdAsync(id);
            return response != null ? _mapper.Map<ResponseDto>(response) : null;
        }

        public async Task<PaginatedResult<ResponseDto>> GetResponsesAsync(PaginationRequest request)
        {
            var responses = await _unitOfWork.Responses.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                responses = responses.Where(r =>
                    r.Content.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            responses = request.SortBy?.ToLower() switch
            {
                "createdat" => request.SortDescending ? responses.OrderByDescending(r => r.CreatedAt) : responses.OrderBy(r => r.CreatedAt),
                "readat" => request.SortDescending ? responses.OrderByDescending(r => r.ReadAt) : responses.OrderBy(r => r.ReadAt),
                _ => responses.OrderByDescending(r => r.CreatedAt)
            };

            var totalItems = responses.Count();
            var items = responses
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var responseDtos = _mapper.Map<List<ResponseDto>>(items);

            return new PaginatedResult<ResponseDto>
            {
                Items = responseDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ResponseDto> CreateAsync(CreateResponseDto createDto)
        {
            // Validate ticket exists
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(createDto.TicketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", createDto.TicketId);
            }

            var response = _mapper.Map<Response>(createDto);
            response.UserId = _currentUserService.UserId;

            var createdResponse = await _unitOfWork.Responses.AddAsync(response);

            _logger.LogInformation("Response created with ID: {ResponseId} for Ticket: {TicketId}", 
                createdResponse.Id, createDto.TicketId);

            return _mapper.Map<ResponseDto>(createdResponse);
        }

        public async Task<ResponseDto> UpdateAsync(int id, UpdateResponseDto updateDto)
        {
            var response = await _unitOfWork.Responses.GetByIdAsync(id);
            if (response == null)
            {
                throw new NotFoundException("Response", id);
            }

            _mapper.Map(updateDto, response);
            await _unitOfWork.Responses.UpdateAsync(response);

            _logger.LogInformation("Response updated with ID: {ResponseId}", id);

            return _mapper.Map<ResponseDto>(response);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _unitOfWork.Responses.GetByIdAsync(id);
            if (response == null)
            {
                return false;
            }

            await _unitOfWork.Responses.DeleteAsync(response);

            _logger.LogInformation("Response deleted with ID: {ResponseId}", id);

            return true;
        }

        public async Task<IEnumerable<ResponseDto>> GetByTicketIdAsync(int ticketId)
        {
            var responses = await _unitOfWork.Responses.GetByTicketIdAsync(ticketId);
            return _mapper.Map<IEnumerable<ResponseDto>>(responses);
        }

        public async Task<IEnumerable<ResponseDto>> GetByUserIdAsync(int userId)
        {
            var responses = await _unitOfWork.Responses.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ResponseDto>>(responses);
        }

        public async Task<IEnumerable<ResponseDto>> GetCustomerResponsesAsync(int ticketId)
        {
            var responses = await _unitOfWork.Responses.GetCustomerResponsesAsync(ticketId);
            return _mapper.Map<IEnumerable<ResponseDto>>(responses);
        }

        public async Task<IEnumerable<ResponseDto>> GetAgentResponsesAsync(int ticketId)
        {
            var responses = await _unitOfWork.Responses.GetAgentResponsesAsync(ticketId);
            return _mapper.Map<IEnumerable<ResponseDto>>(responses);
        }

        public async Task<IEnumerable<ResponseDto>> GetUnreadResponsesAsync(int ticketId)
        {
            var responses = await _unitOfWork.Responses.GetUnreadResponsesAsync(ticketId);
            return _mapper.Map<IEnumerable<ResponseDto>>(responses);
        }

        public async Task<ResponseDto?> GetLatestResponseAsync(int ticketId)
        {
            var response = await _unitOfWork.Responses.GetLatestResponseAsync(ticketId);
            return response != null ? _mapper.Map<ResponseDto>(response) : null;
        }

        public async Task<bool> MarkAsReadAsync(int responseId)
        {
            var response = await _unitOfWork.Responses.GetByIdAsync(responseId);
            if (response == null)
            {
                throw new NotFoundException("Response", responseId);
            }

            response.ReadAt = _dateTime.UtcNow;
            await _unitOfWork.Responses.UpdateAsync(response);

            _logger.LogInformation("Response {ResponseId} marked as read", responseId);

            return true;
        }
    }
}