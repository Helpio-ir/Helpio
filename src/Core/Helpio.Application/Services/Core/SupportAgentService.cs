using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Core;

namespace Helpio.Ir.Application.Services.Core
{
    public class SupportAgentService : ISupportAgentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupportAgentService> _logger;

        public SupportAgentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SupportAgentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SupportAgentDto?> GetByIdAsync(int id)
        {
            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(id);
            return agent != null ? _mapper.Map<SupportAgentDto>(agent) : null;
        }

        public async Task<SupportAgentDto?> GetByAgentCodeAsync(string agentCode)
        {
            var agent = await _unitOfWork.SupportAgents.GetByAgentCodeAsync(agentCode);
            return agent != null ? _mapper.Map<SupportAgentDto>(agent) : null;
        }

        public async Task<PaginatedResult<SupportAgentDto>> GetAgentsAsync(PaginationRequest request)
        {
            var agents = await _unitOfWork.SupportAgents.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                agents = agents.Where(a =>
                    a.AgentCode.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Department.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Position.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Specialization.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                agents = request.SortBy.ToLower() switch
                {
                    "agentcode" => request.SortDescending ? agents.OrderByDescending(a => a.AgentCode) : agents.OrderBy(a => a.AgentCode),
                    "department" => request.SortDescending ? agents.OrderByDescending(a => a.Department) : agents.OrderBy(a => a.Department),
                    "position" => request.SortDescending ? agents.OrderByDescending(a => a.Position) : agents.OrderBy(a => a.Position),
                    "supportlevel" => request.SortDescending ? agents.OrderByDescending(a => a.SupportLevel) : agents.OrderBy(a => a.SupportLevel),
                    "hiredate" => request.SortDescending ? agents.OrderByDescending(a => a.HireDate) : agents.OrderBy(a => a.HireDate),
                    _ => agents.OrderBy(a => a.Id)
                };
            }
            else
            {
                agents = agents.OrderBy(a => a.Id);
            }

            var totalItems = agents.Count();
            var items = agents
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var agentDtos = _mapper.Map<List<SupportAgentDto>>(items);

            return new PaginatedResult<SupportAgentDto>
            {
                Items = agentDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<SupportAgentDto> CreateAsync(CreateSupportAgentDto createDto)
        {
            // Validate User exists
            var user = await _unitOfWork.Users.GetByIdAsync(createDto.UserId);
            if (user == null)
            {
                throw new NotFoundException("User", createDto.UserId);
            }

            // Validate Team exists
            var team = await _unitOfWork.Teams.GetByIdAsync(createDto.TeamId);
            if (team == null)
            {
                throw new NotFoundException("Team", createDto.TeamId);
            }

            // Validate Profile exists
            var profile = await _unitOfWork.Profiles.GetByIdAsync(createDto.ProfileId);
            if (profile == null)
            {
                throw new NotFoundException("Profile", createDto.ProfileId);
            }

            // Check if agent code already exists
            var existingAgent = await _unitOfWork.SupportAgents.GetByAgentCodeAsync(createDto.AgentCode);
            if (existingAgent != null)
            {
                throw new ArgumentException("Agent code already exists");
            }

            var agent = _mapper.Map<SupportAgent>(createDto);
            var createdAgent = await _unitOfWork.SupportAgents.AddAsync(agent);

            _logger.LogInformation("Support agent created with ID: {AgentId}", createdAgent.Id);

            return _mapper.Map<SupportAgentDto>(createdAgent);
        }

        public async Task<SupportAgentDto> UpdateAsync(int id, UpdateSupportAgentDto updateDto)
        {
            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(id);
            if (agent == null)
            {
                throw new NotFoundException("SupportAgent", id);
            }

            // Validate Team exists
            var team = await _unitOfWork.Teams.GetByIdAsync(updateDto.TeamId);
            if (team == null)
            {
                throw new NotFoundException("Team", updateDto.TeamId);
            }

            _mapper.Map(updateDto, agent);
            await _unitOfWork.SupportAgents.UpdateAsync(agent);

            _logger.LogInformation("Support agent updated with ID: {AgentId}", id);

            return _mapper.Map<SupportAgentDto>(agent);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(id);
            if (agent == null)
            {
                return false;
            }

            await _unitOfWork.SupportAgents.DeleteAsync(agent);

            _logger.LogInformation("Support agent deleted with ID: {AgentId}", id);

            return true;
        }

        public async Task<IEnumerable<SupportAgentDto>> GetByTeamIdAsync(int teamId)
        {
            var agents = await _unitOfWork.SupportAgents.GetByTeamIdAsync(teamId);
            return _mapper.Map<IEnumerable<SupportAgentDto>>(agents);
        }

        public async Task<IEnumerable<SupportAgentDto>> GetAvailableAgentsAsync()
        {
            var agents = await _unitOfWork.SupportAgents.GetAvailableAgentsAsync();
            return _mapper.Map<IEnumerable<SupportAgentDto>>(agents);
        }

        public async Task<IEnumerable<SupportAgentDto>> GetBySpecializationAsync(string specialization)
        {
            var agents = await _unitOfWork.SupportAgents.GetBySpecializationAsync(specialization);
            return _mapper.Map<IEnumerable<SupportAgentDto>>(agents);
        }

        public async Task<IEnumerable<SupportAgentDto>> GetAgentsWithLowWorkloadAsync(int maxTickets)
        {
            var agents = await _unitOfWork.SupportAgents.GetAgentsWithLowWorkloadAsync(maxTickets);
            return _mapper.Map<IEnumerable<SupportAgentDto>>(agents);
        }

        public async Task<bool> SetAvailabilityAsync(int agentId, bool isAvailable)
        {
            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(agentId);
            if (agent == null)
            {
                throw new NotFoundException("SupportAgent", agentId);
            }

            agent.IsAvailable = isAvailable;
            await _unitOfWork.SupportAgents.UpdateAsync(agent);

            _logger.LogInformation("Agent {AgentId} availability set to {IsAvailable}", agentId, isAvailable);

            return true;
        }

        public async Task<bool> UpdateWorkloadAsync(int agentId, int ticketCount)
        {
            var agent = await _unitOfWork.SupportAgents.GetByIdAsync(agentId);
            if (agent == null)
            {
                throw new NotFoundException("SupportAgent", agentId);
            }

            agent.CurrentTicketCount = ticketCount;
            await _unitOfWork.SupportAgents.UpdateAsync(agent);

            return true;
        }

        public async Task<SupportAgentDto?> GetBestAvailableAgentAsync(int teamId, string? specialization = null)
        {
            var teamAgents = await _unitOfWork.SupportAgents.GetByTeamIdAsync(teamId);
            
            var availableAgents = teamAgents.Where(a => 
                a.IsAvailable && 
                a.IsActive && 
                a.CurrentTicketCount < a.MaxConcurrentTickets);

            if (!string.IsNullOrEmpty(specialization))
            {
                availableAgents = availableAgents.Where(a => 
                    a.Specialization.Contains(specialization, StringComparison.OrdinalIgnoreCase));
            }

            // Select agent with lowest workload and highest support level
            var bestAgent = availableAgents
                .OrderBy(a => a.CurrentTicketCount)
                .ThenByDescending(a => a.SupportLevel)
                .FirstOrDefault();

            return bestAgent != null ? _mapper.Map<SupportAgentDto>(bestAgent) : null;
        }
    }
}