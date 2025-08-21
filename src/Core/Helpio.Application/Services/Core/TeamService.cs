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
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamService> _logger;

        public TeamService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TeamService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TeamDto?> GetByIdAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            return team != null ? _mapper.Map<TeamDto>(team) : null;
        }

        public async Task<TeamDto?> GetWithSupportAgentsAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetWithSupportAgentsAsync(id);
            return team != null ? _mapper.Map<TeamDto>(team) : null;
        }

        public async Task<PaginatedResult<TeamDto>> GetTeamsAsync(PaginationRequest request)
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                teams = teams.Where(t =>
                    t.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            teams = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? teams.OrderByDescending(t => t.Name) : teams.OrderBy(t => t.Name),
                "createdat" => request.SortDescending ? teams.OrderByDescending(t => t.CreatedAt) : teams.OrderBy(t => t.CreatedAt),
                _ => teams.OrderBy(t => t.Name)
            };

            var totalItems = teams.Count();
            var items = teams
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var teamDtos = _mapper.Map<List<TeamDto>>(items);

            return new PaginatedResult<TeamDto>
            {
                Items = teamDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TeamDto> CreateAsync(CreateTeamDto createDto)
        {
            // Validate branch exists
            var branch = await _unitOfWork.Branches.GetByIdAsync(createDto.BranchId);
            if (branch == null)
            {
                throw new NotFoundException("Branch", createDto.BranchId);
            }

            // Validate team lead exists if provided
            if (createDto.TeamLeadId.HasValue)
            {
                var teamLead = await _unitOfWork.SupportAgents.GetByIdAsync(createDto.TeamLeadId.Value);
                if (teamLead == null)
                {
                    throw new NotFoundException("SupportAgent", createDto.TeamLeadId.Value);
                }
            }

            // Validate supervisor exists if provided
            if (createDto.SupervisorId.HasValue)
            {
                var supervisor = await _unitOfWork.SupportAgents.GetByIdAsync(createDto.SupervisorId.Value);
                if (supervisor == null)
                {
                    throw new NotFoundException("SupportAgent", createDto.SupervisorId.Value);
                }
            }

            var team = _mapper.Map<Team>(createDto);
            var createdTeam = await _unitOfWork.Teams.AddAsync(team);

            _logger.LogInformation("Team created with ID: {TeamId}, Name: {Name}", 
                createdTeam.Id, createdTeam.Name);

            return _mapper.Map<TeamDto>(createdTeam);
        }

        public async Task<TeamDto> UpdateAsync(int id, UpdateTeamDto updateDto)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            if (team == null)
            {
                throw new NotFoundException("Team", id);
            }

            // Validate team lead exists if provided
            if (updateDto.TeamLeadId.HasValue)
            {
                var teamLead = await _unitOfWork.SupportAgents.GetByIdAsync(updateDto.TeamLeadId.Value);
                if (teamLead == null)
                {
                    throw new NotFoundException("SupportAgent", updateDto.TeamLeadId.Value);
                }
            }

            // Validate supervisor exists if provided
            if (updateDto.SupervisorId.HasValue)
            {
                var supervisor = await _unitOfWork.SupportAgents.GetByIdAsync(updateDto.SupervisorId.Value);
                if (supervisor == null)
                {
                    throw new NotFoundException("SupportAgent", updateDto.SupervisorId.Value);
                }
            }

            _mapper.Map(updateDto, team);
            await _unitOfWork.Teams.UpdateAsync(team);

            _logger.LogInformation("Team updated with ID: {TeamId}", id);

            return _mapper.Map<TeamDto>(team);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            if (team == null)
            {
                return false;
            }

            // Check if team has support agents
            var allAgents = await _unitOfWork.SupportAgents.GetAllAsync();
            var hasAgents = allAgents.Any(a => a.TeamId == id);
            if (hasAgents)
            {
                throw new InvalidOperationException("Cannot delete team that has support agents");
            }

            // Check if team has tickets
            var allTickets = await _unitOfWork.Tickets.GetAllAsync();
            var hasTickets = allTickets.Any(t => t.TeamId == id);
            if (hasTickets)
            {
                throw new InvalidOperationException("Cannot delete team that has tickets");
            }

            await _unitOfWork.Teams.DeleteAsync(team);

            _logger.LogInformation("Team deleted with ID: {TeamId}", id);

            return true;
        }

        public async Task<IEnumerable<TeamDto>> GetByBranchIdAsync(int branchId)
        {
            var teams = await _unitOfWork.Teams.GetByBranchIdAsync(branchId);
            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<IEnumerable<TeamDto>> GetActiveTeamsAsync()
        {
            var teams = await _unitOfWork.Teams.GetActiveTeamsAsync();
            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsByManagerAsync(int managerId)
        {
            var teams = await _unitOfWork.Teams.GetTeamsByManagerAsync(managerId);
            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }
    }
}