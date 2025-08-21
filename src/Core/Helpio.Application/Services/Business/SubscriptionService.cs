using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IDateTime _dateTime;

        public SubscriptionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SubscriptionService> logger,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<SubscriptionDto?> GetByIdAsync(int id)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id);
            return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
        }

        public async Task<PaginatedResult<SubscriptionDto>> GetSubscriptionsAsync(PaginationRequest request)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                subscriptions = subscriptions.Where(s =>
                    s.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (s.Description != null && s.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            subscriptions = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? subscriptions.OrderByDescending(s => s.Name) : subscriptions.OrderBy(s => s.Name),
                "price" => request.SortDescending ? subscriptions.OrderByDescending(s => s.Price) : subscriptions.OrderBy(s => s.Price),
                "startdate" => request.SortDescending ? subscriptions.OrderByDescending(s => s.StartDate) : subscriptions.OrderBy(s => s.StartDate),
                "enddate" => request.SortDescending ? subscriptions.OrderByDescending(s => s.EndDate) : subscriptions.OrderBy(s => s.EndDate),
                "status" => request.SortDescending ? subscriptions.OrderByDescending(s => s.Status) : subscriptions.OrderBy(s => s.Status),
                _ => subscriptions.OrderByDescending(s => s.CreatedAt)
            };

            var totalItems = subscriptions.Count();
            var items = subscriptions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var subscriptionDtos = _mapper.Map<List<SubscriptionDto>>(items);

            return new PaginatedResult<SubscriptionDto>
            {
                Items = subscriptionDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto createDto)
        {
            // Validate organization exists if provided
            if (createDto.OrganizationId.HasValue)
            {
                var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId.Value);
                if (organization == null)
                {
                    throw new NotFoundException("Organization", createDto.OrganizationId.Value);
                }
            }

            var subscription = _mapper.Map<Subscription>(createDto);
            var createdSubscription = await _unitOfWork.Subscriptions.AddAsync(subscription);

            _logger.LogInformation("Subscription created with ID: {SubscriptionId}, Name: {Name}", 
                createdSubscription.Id, createdSubscription.Name);

            return _mapper.Map<SubscriptionDto>(createdSubscription);
        }

        public async Task<SubscriptionDto> UpdateAsync(int id, UpdateSubscriptionDto updateDto)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id);
            if (subscription == null)
            {
                throw new NotFoundException("Subscription", id);
            }

            _mapper.Map(updateDto, subscription);
            await _unitOfWork.Subscriptions.UpdateAsync(subscription);

            _logger.LogInformation("Subscription updated with ID: {SubscriptionId}", id);

            return _mapper.Map<SubscriptionDto>(subscription);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id);
            if (subscription == null)
            {
                return false;
            }

            await _unitOfWork.Subscriptions.DeleteAsync(subscription);

            _logger.LogInformation("Subscription deleted with ID: {SubscriptionId}", id);

            return true;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetByOrganizationIdAsync(organizationId);
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetByStatusAsync(SubscriptionStatusDto status)
        {
            var domainStatus = _mapper.Map<SubscriptionStatus>(status);
            var subscriptions = await _unitOfWork.Subscriptions.GetByStatusAsync(domainStatus);
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetActiveSubscriptionsAsync()
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetActiveSubscriptionsAsync();
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetExpiringSubscriptionsAsync(DateTime expiryDate)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetExpiringSubscriptionsAsync(expiryDate);
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetExpiredSubscriptionsAsync()
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetExpiredSubscriptionsAsync();
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<bool> ExtendSubscriptionAsync(int subscriptionId, DateTime newEndDate)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException("Subscription", subscriptionId);
            }

            subscription.EndDate = newEndDate;
            if (subscription.Status == SubscriptionStatus.Expired)
            {
                subscription.Status = SubscriptionStatus.Active;
            }

            await _unitOfWork.Subscriptions.UpdateAsync(subscription);

            _logger.LogInformation("Subscription {SubscriptionId} extended to {EndDate}", 
                subscriptionId, newEndDate);

            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException("Subscription", subscriptionId);
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.IsActive = false;
            await _unitOfWork.Subscriptions.UpdateAsync(subscription);

            _logger.LogInformation("Subscription {SubscriptionId} cancelled", subscriptionId);

            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException("Subscription", subscriptionId);
            }

            // Extend for another billing cycle
            var currentEndDate = subscription.EndDate ?? _dateTime.UtcNow;
            subscription.EndDate = currentEndDate.AddDays(subscription.BillingCycleDays);
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsActive = true;

            await _unitOfWork.Subscriptions.UpdateAsync(subscription);

            _logger.LogInformation("Subscription {SubscriptionId} renewed until {EndDate}", 
                subscriptionId, subscription.EndDate);

            return true;
        }
    }
}