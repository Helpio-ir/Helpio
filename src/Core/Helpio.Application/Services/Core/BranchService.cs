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
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BranchService> _logger;

        public BranchService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BranchService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BranchDto?> GetByIdAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            return branch != null ? _mapper.Map<BranchDto>(branch) : null;
        }

        public async Task<BranchDto?> GetWithTeamsAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetWithTeamsAsync(id);
            return branch != null ? _mapper.Map<BranchDto>(branch) : null;
        }

        public async Task<PaginatedResult<BranchDto>> GetBranchesAsync(PaginationRequest request)
        {
            var branches = await _unitOfWork.Branches.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                branches = branches.Where(b =>
                    b.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (b.Address != null && b.Address.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (b.PhoneNumber != null && b.PhoneNumber.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            branches = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? branches.OrderByDescending(b => b.Name) : branches.OrderBy(b => b.Name),
                "address" => request.SortDescending ? branches.OrderByDescending(b => b.Address) : branches.OrderBy(b => b.Address),
                "createdat" => request.SortDescending ? branches.OrderByDescending(b => b.CreatedAt) : branches.OrderBy(b => b.CreatedAt),
                _ => branches.OrderBy(b => b.Name)
            };

            var totalItems = branches.Count();
            var items = branches
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var branchDtos = _mapper.Map<List<BranchDto>>(items);

            return new PaginatedResult<BranchDto>
            {
                Items = branchDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<BranchDto> CreateAsync(CreateBranchDto createDto)
        {
            // Validate organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", createDto.OrganizationId);
            }

            // Validate branch manager exists if provided
            if (createDto.BranchManagerId.HasValue)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(createDto.BranchManagerId.Value);
                if (manager == null)
                {
                    throw new NotFoundException("User", createDto.BranchManagerId.Value);
                }
            }

            var branch = _mapper.Map<Branch>(createDto);
            var createdBranch = await _unitOfWork.Branches.AddAsync(branch);

            _logger.LogInformation("Branch created with ID: {BranchId}, Name: {Name}", 
                createdBranch.Id, createdBranch.Name);

            return _mapper.Map<BranchDto>(createdBranch);
        }

        public async Task<BranchDto> UpdateAsync(int id, UpdateBranchDto updateDto)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                throw new NotFoundException("Branch", id);
            }

            // Validate branch manager exists if provided
            if (updateDto.BranchManagerId.HasValue)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(updateDto.BranchManagerId.Value);
                if (manager == null)
                {
                    throw new NotFoundException("User", updateDto.BranchManagerId.Value);
                }
            }

            _mapper.Map(updateDto, branch);
            await _unitOfWork.Branches.UpdateAsync(branch);

            _logger.LogInformation("Branch updated with ID: {BranchId}", id);

            return _mapper.Map<BranchDto>(branch);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                return false;
            }

            // Check if branch has teams
            var allTeams = await _unitOfWork.Teams.GetAllAsync();
            var hasTeams = allTeams.Any(t => t.BranchId == id);
            if (hasTeams)
            {
                throw new InvalidOperationException("Cannot delete branch that has teams");
            }

            await _unitOfWork.Branches.DeleteAsync(branch);

            _logger.LogInformation("Branch deleted with ID: {BranchId}", id);

            return true;
        }

        public async Task<IEnumerable<BranchDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var branches = await _unitOfWork.Branches.GetByOrganizationIdAsync(organizationId);
            return _mapper.Map<IEnumerable<BranchDto>>(branches);
        }

        public async Task<IEnumerable<BranchDto>> GetActiveBranchesAsync()
        {
            var branches = await _unitOfWork.Branches.GetActiveBranchesAsync();
            return _mapper.Map<IEnumerable<BranchDto>>(branches);
        }
    }
}