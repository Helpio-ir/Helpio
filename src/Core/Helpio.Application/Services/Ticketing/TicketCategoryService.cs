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
    public class TicketCategoryService : ITicketCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketCategoryService> _logger;

        public TicketCategoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TicketCategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TicketCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.TicketCategories.GetByIdAsync(id);
            return category != null ? _mapper.Map<TicketCategoryDto>(category) : null;
        }

        public async Task<PaginatedResult<TicketCategoryDto>> GetCategoriesAsync(PaginationRequest request)
        {
            var categories = await _unitOfWork.TicketCategories.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                categories = categories.Where(c =>
                    c.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            categories = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? categories.OrderByDescending(c => c.Name) : categories.OrderBy(c => c.Name),
                "createdat" => request.SortDescending ? categories.OrderByDescending(c => c.CreatedAt) : categories.OrderBy(c => c.CreatedAt),
                _ => categories.OrderBy(c => c.Name)
            };

            var totalItems = categories.Count();
            var items = categories
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var categoryDtos = _mapper.Map<List<TicketCategoryDto>>(items);

            return new PaginatedResult<TicketCategoryDto>
            {
                Items = categoryDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TicketCategoryDto> CreateAsync(CreateTicketCategoryDto createDto)
        {
            // Validate organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", createDto.OrganizationId);
            }

            // Check if category name already exists for this organization
            var existingCategories = await _unitOfWork.TicketCategories.GetByOrganizationIdAsync(createDto.OrganizationId);
            if (existingCategories.Any(c => c.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("A ticket category with this name already exists for this organization");
            }

            var category = _mapper.Map<TicketCategory>(createDto);
            var createdCategory = await _unitOfWork.TicketCategories.AddAsync(category);

            _logger.LogInformation("Ticket category created with ID: {CategoryId}, Name: {Name}", 
                createdCategory.Id, createdCategory.Name);

            return _mapper.Map<TicketCategoryDto>(createdCategory);
        }

        public async Task<TicketCategoryDto> UpdateAsync(int id, UpdateTicketCategoryDto updateDto)
        {
            var category = await _unitOfWork.TicketCategories.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException("TicketCategory", id);
            }

            // Check if category name already exists for this organization (excluding current)
            var existingCategories = await _unitOfWork.TicketCategories.GetByOrganizationIdAsync(category.OrganizationId);
            if (existingCategories.Any(c => c.Id != id && c.Name.Equals(updateDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("A ticket category with this name already exists for this organization");
            }

            _mapper.Map(updateDto, category);
            await _unitOfWork.TicketCategories.UpdateAsync(category);

            _logger.LogInformation("Ticket category updated with ID: {CategoryId}", id);

            return _mapper.Map<TicketCategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.TicketCategories.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category is being used by tickets
            var allTickets = await _unitOfWork.Tickets.GetAllAsync();
            var hasTickets = allTickets.Any(t => t.TicketCategoryId == id);
            if (hasTickets)
            {
                throw new InvalidOperationException("Cannot delete ticket category that is being used by tickets");
            }

            await _unitOfWork.TicketCategories.DeleteAsync(category);

            _logger.LogInformation("Ticket category deleted with ID: {CategoryId}", id);

            return true;
        }

        public async Task<IEnumerable<TicketCategoryDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var categories = await _unitOfWork.TicketCategories.GetByOrganizationIdAsync(organizationId);
            return _mapper.Map<IEnumerable<TicketCategoryDto>>(categories);
        }

        public async Task<IEnumerable<TicketCategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.TicketCategories.GetActiveCategoriesAsync();
            return _mapper.Map<IEnumerable<TicketCategoryDto>>(categories);
        }
    }
}