using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Ticketing;

namespace Helpio.Ir.Application.Services.Ticketing
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;
        private readonly ISubscriptionLimitService _subscriptionLimitService;

        public TicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TicketService> logger,
            ICurrentUserService currentUserService,
            IDateTime dateTime,
            ISubscriptionLimitService subscriptionLimitService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
            _subscriptionLimitService = subscriptionLimitService;
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        public async Task<TicketDto?> GetWithDetailsAsync(int id)
        {
            var ticket = await _unitOfWork.Tickets.GetWithDetailsAsync(id);
            return ticket != null ? _mapper.Map<TicketDto>(ticket) : null;
        }

        public async Task<PaginatedResult<TicketDto>> GetTicketsAsync(PaginationRequest request)
        {
            var tickets = await _unitOfWork.Tickets.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                tickets = tickets.Where(t =>
                    t.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                tickets = request.SortBy.ToLower() switch
                {
                    "title" => request.SortDescending ? tickets.OrderByDescending(t => t.Title) : tickets.OrderBy(t => t.Title),
                    "priority" => request.SortDescending ? tickets.OrderByDescending(t => t.Priority) : tickets.OrderBy(t => t.Priority),
                    "createdat" => request.SortDescending ? tickets.OrderByDescending(t => t.CreatedAt) : tickets.OrderBy(t => t.CreatedAt),
                    "duedate" => request.SortDescending ? tickets.OrderByDescending(t => t.DueDate) : tickets.OrderBy(t => t.DueDate),
                    _ => tickets.OrderByDescending(t => t.CreatedAt)
                };
            }
            else
            {
                tickets = tickets.OrderByDescending(t => t.CreatedAt);
            }

            var totalItems = tickets.Count();
            var items = tickets
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);

            return new PaginatedResult<TicketDto>
            {
                Items = ticketDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TicketDto> CreateAsync(CreateTicketDto createDto)
        {
            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException("Customer", createDto.CustomerId);
            }

            // Validate team exists
            var team = await _unitOfWork.Teams.GetByIdAsync(createDto.TeamId);
            if (team == null)
            {
                throw new NotFoundException("Team", createDto.TeamId);
            }

            // Validate category exists
            var category = await _unitOfWork.TicketCategories.GetByIdAsync(createDto.TicketCategoryId);
            if (category == null)
            {
                throw new NotFoundException("TicketCategory", createDto.TicketCategoryId);
            }

            // Get organization ID - prioritize customer's organization, fallback to team's organization via branch
            int? organizationId = customer.OrganizationId;
            if (!organizationId.HasValue)
            {
                // Load branch with organization to get organizationId
                var branch = await _unitOfWork.Branches.GetByIdAsync(team.BranchId);
                organizationId = branch?.OrganizationId;
            }

            if (!organizationId.HasValue)
            {
                throw new InvalidOperationException("Could not determine organization for ticket creation");
            }

            // Check subscription limits
            var canCreateTicket = await _subscriptionLimitService.CanCreateTicketAsync(organizationId.Value);
            if (!canCreateTicket)
            {
                var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(organizationId.Value);
                throw new BusinessRuleViolationException(
                    limitInfo.LimitationMessage ?? 
                    $"شما به حد مجاز ماهانه {limitInfo.MonthlyLimit} تیکت رسیده‌اید. لطفاً برای ایجاد تیکت بیشتر، اشتراک خود را ارتقا دهید.");
            }

            // Get default state
            var defaultState = await _unitOfWork.TicketStates.GetDefaultStateAsync();
            if (defaultState == null)
            {
                throw new InvalidOperationException("No default ticket state found");
            }

            var ticket = _mapper.Map<Ticket>(createDto);
            ticket.TicketStateId = defaultState.Id;

            var createdTicket = await _unitOfWork.Tickets.AddAsync(ticket);

            // Increment ticket count for subscription limits
            await _subscriptionLimitService.IncrementTicketCountAsync(organizationId.Value);

            _logger.LogInformation("Ticket created with ID: {TicketId} for Customer: {CustomerId}, Organization: {OrganizationId}", 
                createdTicket.Id, createDto.CustomerId, organizationId.Value);

            // Create system note for ticket creation
            await CreateSystemNoteAsync(createdTicket.Id, $"Ticket created with priority: {createDto.Priority}");

            return _mapper.Map<TicketDto>(createdTicket);
        }

        public async Task<TicketDto> UpdateAsync(int id, UpdateTicketDto updateDto)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", id);
            }

            var oldPriority = ticket.Priority;
            var oldSupportAgentId = ticket.SupportAgentId;

            _mapper.Map(updateDto, ticket);
            await _unitOfWork.Tickets.UpdateAsync(ticket);

            _logger.LogInformation("Ticket updated with ID: {TicketId}", id);

            // Create system notes for changes
            if (oldPriority != ticket.Priority)
            {
                await CreateSystemNoteAsync(id, $"Priority changed from {oldPriority} to {ticket.Priority}");
            }

            if (oldSupportAgentId != ticket.SupportAgentId)
            {
                var agentName = ticket.SupportAgentId.HasValue ? 
                    (await _unitOfWork.SupportAgents.GetByIdAsync(ticket.SupportAgentId.Value))?.User?.FirstName ?? "Unknown" : 
                    "Unassigned";
                await CreateSystemNoteAsync(id, $"Assigned to: {agentName}");
            }

            return _mapper.Map<TicketDto>(ticket);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket == null)
            {
                return false;
            }

            await _unitOfWork.Tickets.DeleteAsync(ticket);
            
            _logger.LogInformation("Ticket deleted with ID: {TicketId}", id);
            
            return true;
        }

        #region Filtering Methods

        public async Task<IEnumerable<TicketDto>> GetByCustomerIdAsync(int customerId)
        {
            var tickets = await _unitOfWork.Tickets.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetByTeamIdAsync(int teamId)
        {
            var tickets = await _unitOfWork.Tickets.GetByTeamIdAsync(teamId);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetBySupportAgentIdAsync(int supportAgentId)
        {
            var tickets = await _unitOfWork.Tickets.GetBySupportAgentIdAsync(supportAgentId);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetByStateIdAsync(int stateId)
        {
            var tickets = await _unitOfWork.Tickets.GetByStateIdAsync(stateId);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetByCategoryIdAsync(int categoryId)
        {
            var tickets = await _unitOfWork.Tickets.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetByPriorityAsync(TicketPriorityDto priority)
        {
            var domainPriority = _mapper.Map<TicketPriority>(priority);
            var tickets = await _unitOfWork.Tickets.GetByPriorityAsync(domainPriority);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetOverdueTicketsAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetOverdueTicketsAsync();
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetUnassignedTicketsAsync()
        {
            var tickets = await _unitOfWork.Tickets.GetUnassignedTicketsAsync();
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsDueSoonAsync(DateTime dueDate)
        {
            var tickets = await _unitOfWork.Tickets.GetTicketsDueSoonAsync(dueDate);
            return _mapper.Map<IEnumerable<TicketDto>>(tickets);
        }

        #endregion

        #region Actions

        public async Task<bool> AssignTicketAsync(AssignTicketDto assignDto)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(assignDto.TicketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", assignDto.TicketId);
            }

            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(assignDto.SupportAgentId);
            if (agent == null)
            {
                throw new NotFoundException("SupportAgent", assignDto.SupportAgentId);
            }

            if (!agent.IsAvailable)
            {
                throw new InvalidOperationException("Support agent is not available");
            }

            ticket.SupportAgentId = assignDto.SupportAgentId;
            await _unitOfWork.Tickets.UpdateAsync(ticket);

            // Update agent workload
            agent.CurrentTicketCount++;
            await _unitOfWork.SupportAgents.UpdateAsync(agent);

            await CreateSystemNoteAsync(assignDto.TicketId, $"Assigned to {agent.User?.FirstName ?? "Agent"}");

            _logger.LogInformation("Ticket {TicketId} assigned to agent {AgentId}", 
                assignDto.TicketId, assignDto.SupportAgentId);

            return true;
        }

        public async Task<bool> ResolveTicketAsync(ResolveTicketDto resolveDto)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(resolveDto.TicketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", resolveDto.TicketId);
            }

            // Get final state
            var finalStates = await _unitOfWork.TicketStates.GetFinalStatesAsync();
            var resolvedState = finalStates.FirstOrDefault();
            if (resolvedState == null)
            {
                throw new InvalidOperationException("No final ticket state found");
            }

            ticket.TicketStateId = resolvedState.Id;
            ticket.ResolvedDate = _dateTime.UtcNow;
            ticket.Resolution = resolveDto.Resolution;
            ticket.ActualHours = resolveDto.ActualHours;

            await _unitOfWork.Tickets.UpdateAsync(ticket);

            // Update agent workload
            if (ticket.SupportAgentId.HasValue)
            {
                var agent = await _unitOfWork.SupportAgents.GetByIdAsync(ticket.SupportAgentId.Value);
                if (agent != null)
                {
                    agent.CurrentTicketCount = Math.Max(0, agent.CurrentTicketCount - 1);
                    await _unitOfWork.SupportAgents.UpdateAsync(agent);
                }
            }

            await CreateSystemNoteAsync(resolveDto.TicketId, $"Ticket resolved. Resolution: {resolveDto.Resolution}");

            _logger.LogInformation("Ticket {TicketId} resolved", resolveDto.TicketId);

            return true;
        }

        public async Task<bool> ChangeStateAsync(int ticketId, int newStateId)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", ticketId);
            }

            var newState = await _unitOfWork.TicketStates.GetByIdAsync(newStateId);
            if (newState == null)
            {
                throw new NotFoundException("TicketState", newStateId);
            }

            var oldStateId = ticket.TicketStateId;
            ticket.TicketStateId = newStateId;
            await _unitOfWork.Tickets.UpdateAsync(ticket);

            await CreateSystemNoteAsync(ticketId, $"State changed to: {newState.Name}");

            return true;
        }

        public async Task<bool> ChangePriorityAsync(int ticketId, TicketPriorityDto priority)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", ticketId);
            }

            var oldPriority = ticket.Priority;
            ticket.Priority = _mapper.Map<TicketPriority>(priority);
            await _unitOfWork.Tickets.UpdateAsync(ticket);

            await CreateSystemNoteAsync(ticketId, $"Priority changed from {oldPriority} to {priority}");

            return true;
        }

        public async Task<bool> SetDueDateAsync(int ticketId, DateTime? dueDate)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new NotFoundException("Ticket", ticketId);
            }

            ticket.DueDate = dueDate;
            await _unitOfWork.Tickets.UpdateAsync(ticket);

            var dueDateText = dueDate?.ToString("yyyy-MM-dd") ?? "Removed";
            await CreateSystemNoteAsync(ticketId, $"Due date set to: {dueDateText}");

            return true;
        }

        #endregion

        #region Statistics

        public async Task<int> GetTicketCountByAgentAsync(int agentId)
        {
            return await _unitOfWork.Tickets.GetTicketCountByAgentAsync(agentId);
        }

        public async Task<Dictionary<string, int>> GetTicketStatisticsAsync()
        {
            var allTickets = await _unitOfWork.Tickets.GetAllAsync();
            
            return new Dictionary<string, int>
            {
                ["Total"] = allTickets.Count(),
                ["Open"] = allTickets.Count(t => t.ResolvedDate == null),
                ["Resolved"] = allTickets.Count(t => t.ResolvedDate != null),
                ["Overdue"] = allTickets.Count(t => t.DueDate.HasValue && t.DueDate < _dateTime.UtcNow && t.ResolvedDate == null),
                ["Unassigned"] = allTickets.Count(t => t.SupportAgentId == null),
                ["High Priority"] = allTickets.Count(t => t.Priority == TicketPriority.High || t.Priority == TicketPriority.Critical)
            };
        }

        public async Task<IEnumerable<TicketDto>> GetRecentTicketsAsync(int count = 10)
        {
            var allTickets = await _unitOfWork.Tickets.GetAllAsync();
            var recentTickets = allTickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToList();

            return _mapper.Map<IEnumerable<TicketDto>>(recentTickets);
        }

        #endregion

        #region Private Methods

        private async Task CreateSystemNoteAsync(int ticketId, string description)
        {
            var systemNote = new Domain.Entities.Ticketing.Note
            {
                TicketId = ticketId,
                Description = description,
                IsSystemNote = true,
                IsPrivate = false
            };

            await _unitOfWork.Notes.AddAsync(systemNote);
        }

        #endregion
    }
}