using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Knowledge;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Knowledge;

namespace Helpio.Ir.Application.Services.Knowledge
{
    public class CannedResponseService : ICannedResponseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CannedResponseService> _logger;

        public CannedResponseService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CannedResponseService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CannedResponseDto?> GetByIdAsync(int id)
        {
            var response = await _unitOfWork.CannedResponses.GetByIdAsync(id);
            return response != null ? _mapper.Map<CannedResponseDto>(response) : null;
        }

        public async Task<CannedResponseDto?> GetByNameAsync(string name)
        {
            var response = await _unitOfWork.CannedResponses.GetByNameAsync(name);
            return response != null ? _mapper.Map<CannedResponseDto>(response) : null;
        }

        public async Task<PaginatedResult<CannedResponseDto>> GetResponsesAsync(PaginationRequest request)
        {
            var responses = await _unitOfWork.CannedResponses.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                responses = responses.Where(r =>
                    r.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Content.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Tags != null && r.Tags.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            responses = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? responses.OrderByDescending(r => r.Name) : responses.OrderBy(r => r.Name),
                "usagecount" => request.SortDescending ? responses.OrderByDescending(r => r.UsageCount) : responses.OrderBy(r => r.UsageCount),
                "createdat" => request.SortDescending ? responses.OrderByDescending(r => r.CreatedAt) : responses.OrderBy(r => r.CreatedAt),
                _ => responses.OrderBy(r => r.Name)
            };

            var totalItems = responses.Count();
            var items = responses
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var responseDtos = _mapper.Map<List<CannedResponseDto>>(items);

            return new PaginatedResult<CannedResponseDto>
            {
                Items = responseDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<CannedResponseDto> CreateAsync(CreateCannedResponseDto createDto)
        {
            // Validate organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", createDto.OrganizationId);
            }

            // Check if name already exists for this organization
            var existingResponse = await _unitOfWork.CannedResponses.GetByNameAsync(createDto.Name);
            if (existingResponse != null && existingResponse.OrganizationId == createDto.OrganizationId)
            {
                throw new ArgumentException("A canned response with this name already exists for this organization");
            }

            var response = _mapper.Map<CannedResponse>(createDto);
            var createdResponse = await _unitOfWork.CannedResponses.AddAsync(response);

            _logger.LogInformation("Canned response created with ID: {ResponseId}, Name: {Name}", 
                createdResponse.Id, createdResponse.Name);

            return _mapper.Map<CannedResponseDto>(createdResponse);
        }

        public async Task<CannedResponseDto> UpdateAsync(int id, UpdateCannedResponseDto updateDto)
        {
            var response = await _unitOfWork.CannedResponses.GetByIdAsync(id);
            if (response == null)
            {
                throw new NotFoundException("CannedResponse", id);
            }

            _mapper.Map(updateDto, response);
            await _unitOfWork.CannedResponses.UpdateAsync(response);

            _logger.LogInformation("Canned response updated with ID: {ResponseId}", id);

            return _mapper.Map<CannedResponseDto>(response);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _unitOfWork.CannedResponses.GetByIdAsync(id);
            if (response == null)
            {
                return false;
            }

            await _unitOfWork.CannedResponses.DeleteAsync(response);

            _logger.LogInformation("Canned response deleted with ID: {ResponseId}", id);

            return true;
        }

        public async Task<IEnumerable<CannedResponseDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var responses = await _unitOfWork.CannedResponses.GetByOrganizationIdAsync(organizationId);
            return _mapper.Map<IEnumerable<CannedResponseDto>>(responses);
        }

        public async Task<IEnumerable<CannedResponseDto>> GetActiveResponsesAsync()
        {
            var responses = await _unitOfWork.CannedResponses.GetActiveResponsesAsync();
            return _mapper.Map<IEnumerable<CannedResponseDto>>(responses);
        }

        public async Task<IEnumerable<CannedResponseDto>> SearchByTagsAsync(string tags)
        {
            var responses = await _unitOfWork.CannedResponses.SearchByTagsAsync(tags);
            return _mapper.Map<IEnumerable<CannedResponseDto>>(responses);
        }

        public async Task<IEnumerable<CannedResponseDto>> GetMostUsedResponsesAsync(int count)
        {
            var responses = await _unitOfWork.CannedResponses.GetMostUsedResponsesAsync(count);
            return _mapper.Map<IEnumerable<CannedResponseDto>>(responses);
        }

        public async Task<bool> IncrementUsageAsync(int responseId)
        {
            var response = await _unitOfWork.CannedResponses.GetByIdAsync(responseId);
            if (response == null)
            {
                throw new NotFoundException("CannedResponse", responseId);
            }

            response.UsageCount++;
            await _unitOfWork.CannedResponses.UpdateAsync(response);

            _logger.LogInformation("Canned response {ResponseId} usage incremented to {UsageCount}", 
                responseId, response.UsageCount);

            return true;
        }
    }
}