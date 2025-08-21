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
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrganizationService> _logger;

        public OrganizationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<OrganizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrganizationDto?> GetByIdAsync(int id)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            return organization != null ? _mapper.Map<OrganizationDto>(organization) : null;
        }

        public async Task<OrganizationDto?> GetWithBranchesAsync(int id)
        {
            var organization = await _unitOfWork.Organizations.GetWithBranchesAsync(id);
            return organization != null ? _mapper.Map<OrganizationDto>(organization) : null;
        }

        public async Task<OrganizationDto?> GetWithTicketCategoriesAsync(int id)
        {
            var organization = await _unitOfWork.Organizations.GetWithTicketCategoriesAsync(id);
            return organization != null ? _mapper.Map<OrganizationDto>(organization) : null;
        }

        public async Task<PaginatedResult<OrganizationDto>> GetOrganizationsAsync(PaginationRequest request)
        {
            var organizations = await _unitOfWork.Organizations.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                organizations = organizations.Where(o =>
                    o.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (o.Description != null && o.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (o.Email != null && o.Email.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            organizations = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? organizations.OrderByDescending(o => o.Name) : organizations.OrderBy(o => o.Name),
                "email" => request.SortDescending ? organizations.OrderByDescending(o => o.Email) : organizations.OrderBy(o => o.Email),
                "createdat" => request.SortDescending ? organizations.OrderByDescending(o => o.CreatedAt) : organizations.OrderBy(o => o.CreatedAt),
                _ => organizations.OrderBy(o => o.Name)
            };

            var totalItems = organizations.Count();
            var items = organizations
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var organizationDtos = _mapper.Map<List<OrganizationDto>>(items);

            return new PaginatedResult<OrganizationDto>
            {
                Items = organizationDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto createDto)
        {
            var organization = _mapper.Map<Organization>(createDto);
            var createdOrganization = await _unitOfWork.Organizations.AddAsync(organization);

            _logger.LogInformation("Organization created with ID: {OrganizationId}, Name: {Name}", 
                createdOrganization.Id, createdOrganization.Name);

            return _mapper.Map<OrganizationDto>(createdOrganization);
        }

        public async Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto updateDto)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
            {
                throw new NotFoundException("Organization", id);
            }

            _mapper.Map(updateDto, organization);
            await _unitOfWork.Organizations.UpdateAsync(organization);

            _logger.LogInformation("Organization updated with ID: {OrganizationId}", id);

            return _mapper.Map<OrganizationDto>(organization);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
            {
                return false;
            }

            await _unitOfWork.Organizations.DeleteAsync(organization);

            _logger.LogInformation("Organization deleted with ID: {OrganizationId}", id);

            return true;
        }

        public async Task<IEnumerable<OrganizationDto>> GetActiveOrganizationsAsync()
        {
            var organizations = await _unitOfWork.Organizations.GetActiveOrganizationsAsync();
            return _mapper.Map<IEnumerable<OrganizationDto>>(organizations);
        }
    }
}