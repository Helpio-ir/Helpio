using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IDateTime _dateTime;

        public SubscriptionService(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<SubscriptionService> logger,
            IDateTime dateTime)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<SubscriptionDto?> GetByIdAsync(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(s => s.Id == id);
            return subscription != null ? _mapper.Map<SubscriptionDto>(subscription) : null;
        }

        public async Task<PaginatedResult<SubscriptionDto>> GetSubscriptionsAsync(PaginationRequest request)
        {
            var query = _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Organization)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(s =>
                    s.Name.Contains(request.SearchTerm) ||
                    (s.Description != null && s.Description.Contains(request.SearchTerm)));
            }

            var totalItems = await query.CountAsync();

            // Apply sorting
            var subscriptions = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? 
                    await query.OrderByDescending(s => s.Name).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync() :
                    await query.OrderBy(s => s.Name).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(),
                "startdate" => request.SortDescending ?
                    await query.OrderByDescending(s => s.StartDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync() :
                    await query.OrderBy(s => s.StartDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(),
                "status" => request.SortDescending ?
                    await query.OrderByDescending(s => s.Status).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync() :
                    await query.OrderBy(s => s.Status).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(),
                _ => await query.OrderByDescending(s => s.CreatedAt).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync()
            };

            var subscriptionDtos = _mapper.Map<List<SubscriptionDto>>(subscriptions);

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
            // Validate organization exists
            var organization = await _context.Organizations.FindAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException($"Organization with ID {createDto.OrganizationId} not found");
            }

            // Validate plan exists
            var plan = await _context.Plans.FindAsync(createDto.PlanId);
            if (plan == null)
            {
                throw new NotFoundException($"Plan with ID {createDto.PlanId} not found");
            }

            var subscription = _mapper.Map<Subscription>(createDto);
            subscription.CreatedAt = DateTime.UtcNow;

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Load navigation properties for response by querying again
            var createdSubscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(s => s.Id == subscription.Id);

            _logger.LogInformation("Subscription created with ID: {SubscriptionId}, Name: {Name}", 
                subscription.Id, subscription.Name);

            return _mapper.Map<SubscriptionDto>(createdSubscription);
        }

        public async Task<SubscriptionDto> UpdateAsync(int id, UpdateSubscriptionDto updateDto)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subscription == null)
            {
                throw new NotFoundException($"Subscription with ID {id} not found");
            }

            _mapper.Map(updateDto, subscription);
            subscription.UpdatedAt = DateTime.UtcNow;

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription updated with ID: {SubscriptionId}", id);

            return _mapper.Map<SubscriptionDto>(subscription);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return false;
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription deleted with ID: {SubscriptionId}", id);

            return true;
        }

        public async Task<IEnumerable<SubscriptionDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.OrganizationId == organizationId)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetByStatusAsync(SubscriptionStatusDto status)
        {
            var domainStatus = (SubscriptionStatus)status;
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == domainStatus)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetActiveSubscriptionsAsync()
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active && s.IsActive)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetExpiringSubscriptionsAsync(DateTime expiryDate)
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.EndDate.HasValue && s.EndDate <= expiryDate && s.Status == SubscriptionStatus.Active)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetExpiredSubscriptionsAsync()
        {
            var now = _dateTime.UtcNow;
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.EndDate.HasValue && s.EndDate < now)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        }

        public async Task<bool> ExtendSubscriptionAsync(int subscriptionId, DateTime newEndDate)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException($"Subscription with ID {subscriptionId} not found");
            }

            subscription.EndDate = newEndDate;
            subscription.UpdatedAt = DateTime.UtcNow;
            if (subscription.Status == SubscriptionStatus.Expired)
            {
                subscription.Status = SubscriptionStatus.Active;
            }

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription {SubscriptionId} extended to {EndDate}", 
                subscriptionId, newEndDate);

            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException($"Subscription with ID {subscriptionId} not found");
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.IsActive = false;
            subscription.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription {SubscriptionId} cancelled", subscriptionId);

            return true;
        }

        public async Task<bool> RenewSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
            
            if (subscription == null)
            {
                throw new NotFoundException($"Subscription with ID {subscriptionId} not found");
            }

            // Extend for another billing cycle
            var currentEndDate = subscription.EndDate ?? _dateTime.UtcNow;
            subscription.EndDate = currentEndDate.AddDays(subscription.Plan?.BillingCycleDays ?? 30);
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsActive = true;
            subscription.UpdatedAt = DateTime.UtcNow;

            _context.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription {SubscriptionId} renewed until {EndDate}", 
                subscriptionId, subscription.EndDate);

            return true;
        }
    }
}