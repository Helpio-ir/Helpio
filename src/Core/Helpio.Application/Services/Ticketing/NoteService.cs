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
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<NoteService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public NoteService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<NoteService> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<NoteDto?> GetByIdAsync(int id)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            return note != null ? _mapper.Map<NoteDto>(note) : null;
        }

        public async Task<PaginatedResult<NoteDto>> GetNotesAsync(PaginationRequest request)
        {
            var notes = await _unitOfWork.Notes.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                notes = notes.Where(n =>
                    n.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            notes = request.SortBy?.ToLower() switch
            {
                "createdat" => request.SortDescending ? notes.OrderByDescending(n => n.CreatedAt) : notes.OrderBy(n => n.CreatedAt),
                _ => notes.OrderByDescending(n => n.CreatedAt)
            };

            var totalItems = notes.Count();
            var items = notes
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var noteDtos = _mapper.Map<List<NoteDto>>(items);

            return new PaginatedResult<NoteDto>
            {
                Items = noteDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<NoteDto> CreateAsync(CreateNoteDto createDto)
        {
            // Validate ticket exists
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(createDto.TicketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", createDto.TicketId);
            }

            var note = _mapper.Map<Note>(createDto);
            
            // Set support agent from current user
            if (_currentUserService.UserId.HasValue)
            {
                var agents = await _unitOfWork.SupportAgents.GetAllAsync();
                var currentAgent = agents.FirstOrDefault(a => a.UserId == _currentUserService.UserId.Value);
                if (currentAgent != null)
                {
                    note.SupportAgentId = currentAgent.Id;
                }
            }

            var createdNote = await _unitOfWork.Notes.AddAsync(note);

            _logger.LogInformation("Note created with ID: {NoteId} for Ticket: {TicketId}", 
                createdNote.Id, createDto.TicketId);

            return _mapper.Map<NoteDto>(createdNote);
        }

        public async Task<NoteDto> UpdateAsync(int id, UpdateNoteDto updateDto)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null)
            {
                throw new NotFoundException("Note", id);
            }

            _mapper.Map(updateDto, note);
            await _unitOfWork.Notes.UpdateAsync(note);

            _logger.LogInformation("Note updated with ID: {NoteId}", id);

            return _mapper.Map<NoteDto>(note);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _unitOfWork.Notes.GetByIdAsync(id);
            if (note == null)
            {
                return false;
            }

            await _unitOfWork.Notes.DeleteAsync(note);

            _logger.LogInformation("Note deleted with ID: {NoteId}", id);

            return true;
        }

        public async Task<IEnumerable<NoteDto>> GetByTicketIdAsync(int ticketId)
        {
            var notes = await _unitOfWork.Notes.GetByTicketIdAsync(ticketId);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<IEnumerable<NoteDto>> GetBySupportAgentIdAsync(int supportAgentId)
        {
            var notes = await _unitOfWork.Notes.GetBySupportAgentIdAsync(supportAgentId);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<IEnumerable<NoteDto>> GetSystemNotesAsync(int ticketId)
        {
            var notes = await _unitOfWork.Notes.GetSystemNotesAsync(ticketId);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<IEnumerable<NoteDto>> GetPrivateNotesAsync(int ticketId)
        {
            var notes = await _unitOfWork.Notes.GetPrivateNotesAsync(ticketId);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<IEnumerable<NoteDto>> GetPublicNotesAsync(int ticketId)
        {
            var notes = await _unitOfWork.Notes.GetPublicNotesAsync(ticketId);
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<NoteDto> CreateSystemNoteAsync(int ticketId, string description)
        {
            // Validate ticket exists
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", ticketId);
            }

            var systemNote = new Note
            {
                TicketId = ticketId,
                Description = description,
                IsSystemNote = true,
                IsPrivate = false,
                SupportAgentId = null
            };

            var createdNote = await _unitOfWork.Notes.AddAsync(systemNote);

            _logger.LogInformation("System note created with ID: {NoteId} for Ticket: {TicketId}", 
                createdNote.Id, ticketId);

            return _mapper.Map<NoteDto>(createdNote);
        }
    }
}