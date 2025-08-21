using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Ticketing;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public class TicketStateService : ITicketStateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketStateService> _logger;

        public TicketStateService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TicketStateService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TicketStateDto?> GetByIdAsync(int id)
        {
            var state = await _unitOfWork.TicketStates.GetByIdAsync(id);
            return state != null ? _mapper.Map<TicketStateDto>(state) : null;
        }

        public async Task<PaginatedResult<TicketStateDto>> GetStatesAsync(PaginationRequest request)
        {
            var states = await _unitOfWork.TicketStates.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                states = states.Where(s =>
                    s.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (s.Description != null && s.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            states = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? states.OrderByDescending(s => s.Name) : states.OrderBy(s => s.Name),
                "order" => request.SortDescending ? states.OrderByDescending(s => s.Order) : states.OrderBy(s => s.Order),
                "createdat" => request.SortDescending ? states.OrderByDescending(s => s.CreatedAt) : states.OrderBy(s => s.CreatedAt),
                _ => states.OrderBy(s => s.Order)
            };

            var totalItems = states.Count();
            var items = states
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var stateDtos = _mapper.Map<List<TicketStateDto>>(items);

            return new PaginatedResult<TicketStateDto>
            {
                Items = stateDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TicketStateDto> CreateAsync(CreateTicketStateDto createDto)
        {
            // Check if default state already exists
            if (createDto.IsDefault)
            {
                var existingDefault = await _unitOfWork.TicketStates.GetDefaultStateAsync();
                if (existingDefault != null)
                {
                    existingDefault.IsDefault = false;
                    await _unitOfWork.TicketStates.UpdateAsync(existingDefault);
                }
            }

            var state = _mapper.Map<TicketState>(createDto);
            var createdState = await _unitOfWork.TicketStates.AddAsync(state);

            _logger.LogInformation("Ticket state created with ID: {StateId}, Name: {Name}", 
                createdState.Id, createdState.Name);

            return _mapper.Map<TicketStateDto>(createdState);
        }

        public async Task<TicketStateDto> UpdateAsync(int id, UpdateTicketStateDto updateDto)
        {
            var state = await _unitOfWork.TicketStates.GetByIdAsync(id);
            if (state == null)
            {
                throw new NotFoundException("TicketState", id);
            }

            // Handle default state change
            if (updateDto.IsDefault && !state.IsDefault)
            {
                var existingDefault = await _unitOfWork.TicketStates.GetDefaultStateAsync();
                if (existingDefault != null && existingDefault.Id != id)
                {
                    existingDefault.IsDefault = false;
                    await _unitOfWork.TicketStates.UpdateAsync(existingDefault);
                }
            }

            _mapper.Map(updateDto, state);
            await _unitOfWork.TicketStates.UpdateAsync(state);

            _logger.LogInformation("Ticket state updated with ID: {StateId}", id);

            return _mapper.Map<TicketStateDto>(state);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var state = await _unitOfWork.TicketStates.GetByIdAsync(id);
            if (state == null)
            {
                return false;
            }

            // Don't allow deletion of default state
            if (state.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete default ticket state");
            }

            // Check if state is being used by tickets
            var allTickets = await _unitOfWork.Tickets.GetAllAsync();
            var hasTickets = allTickets.Any(t => t.TicketStateId == id);
            if (hasTickets)
            {
                throw new InvalidOperationException("Cannot delete ticket state that is being used by tickets");
            }

            await _unitOfWork.TicketStates.DeleteAsync(state);

            _logger.LogInformation("Ticket state deleted with ID: {StateId}", id);

            return true;
        }

        public async Task<TicketStateDto?> GetDefaultStateAsync()
        {
            var state = await _unitOfWork.TicketStates.GetDefaultStateAsync();
            return state != null ? _mapper.Map<TicketStateDto>(state) : null;
        }

        public async Task<IEnumerable<TicketStateDto>> GetFinalStatesAsync()
        {
            var states = await _unitOfWork.TicketStates.GetFinalStatesAsync();
            return _mapper.Map<IEnumerable<TicketStateDto>>(states);
        }

        public async Task<IEnumerable<TicketStateDto>> GetOrderedStatesAsync()
        {
            var states = await _unitOfWork.TicketStates.GetOrderedStatesAsync();
            return _mapper.Map<IEnumerable<TicketStateDto>>(states);
        }
    }
}