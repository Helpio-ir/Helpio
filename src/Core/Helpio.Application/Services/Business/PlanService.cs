using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces;

namespace Helpio.Ir.Application.Services.Business
{
    public class PlanService : IPlanService
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlanService> _logger;

        public PlanService(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<PlanService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlanDto?> GetByIdAsync(int id)
        {
            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Id == id);

            return plan == null ? null : _mapper.Map<PlanDto>(plan);
        }

        public async Task<PlanDto?> GetByTypeAsync(PlanType type)
        {
            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Type == type && p.IsActive);

            return plan == null ? null : _mapper.Map<PlanDto>(plan);
        }

        public async Task<PaginatedResult<PlanDto>> GetPlansAsync(PaginationRequest request)
        {
            var query = _context.Plans.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                        (p.Description != null && p.Description.Contains(request.SearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var plans = await query
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .Skip(request.PageSize * (request.PageNumber - 1))
                .Take(request.PageSize)
                .ToListAsync();

            var planDtos = _mapper.Map<List<PlanDto>>(plans);

            return new PaginatedResult<PlanDto>
            {
                Items = planDtos,
                TotalItems = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PlanDto> CreateAsync(CreatePlanDto createDto)
        {
            // Check if plan type already exists
            var existingPlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Type == createDto.Type);

            if (existingPlan != null)
            {
                throw new BusinessRuleViolationException($"Plan with type {createDto.Type} already exists");
            }

            var plan = _mapper.Map<Plan>(createDto);
            plan.CreatedAt = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plan created: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return _mapper.Map<PlanDto>(plan);
        }

        public async Task<PlanDto> UpdateAsync(int id, UpdatePlanDto updateDto)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan == null)
            {
                throw new NotFoundException($"Plan with ID {id} not found");
            }

            _mapper.Map(updateDto, plan);
            plan.UpdatedAt = DateTime.UtcNow;

            _context.Update(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plan updated: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return _mapper.Map<PlanDto>(plan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await _context.Plans
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
            {
                return false;
            }

            // Check if plan has active subscriptions
            if (plan.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active))
            {
                throw new BusinessRuleViolationException("Cannot delete plan with active subscriptions");
            }

            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plan deleted: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);

            return true;
        }

        public async Task<IEnumerable<PlanDto>> GetActivePlansAsync()
        {
            var plans = await _context.Plans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return _mapper.Map<List<PlanDto>>(plans);
        }

        public async Task<IEnumerable<PlanDto>> GetPublicPlansAsync()
        {
            var plans = await _context.Plans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return _mapper.Map<List<PlanDto>>(plans);
        }

        public async Task<PlanDto?> GetDefaultFreemiumPlanAsync()
        {
            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Type == PlanType.Freemium && p.IsActive);

            return plan == null ? null : _mapper.Map<PlanDto>(plan);
        }

        public async Task<PlanDto?> GetRecommendedPlanAsync()
        {
            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.IsRecommended && p.IsActive);

            return plan == null ? null : _mapper.Map<PlanDto>(plan);
        }

        public async Task<bool> SetRecommendedPlanAsync(int planId)
        {
            var plan = await _context.Plans.FindAsync(planId);
            if (plan == null || !plan.IsActive)
            {
                return false;
            }

            // Remove recommended flag from all other plans
            var otherPlans = await _context.Plans
                .Where(p => p.Id != planId && p.IsRecommended)
                .ToListAsync();

            foreach (var otherPlan in otherPlans)
            {
                otherPlan.IsRecommended = false;
                otherPlan.UpdatedAt = DateTime.UtcNow;
            }

            // Set new recommended plan
            plan.IsRecommended = true;
            plan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Plan {PlanName} (ID: {PlanId}) set as recommended", plan.Name, plan.Id);

            return true;
        }

        public async Task<bool> ReorderPlansAsync(Dictionary<int, int> planOrders)
        {
            foreach (var (planId, order) in planOrders)
            {
                var plan = await _context.Plans.FindAsync(planId);
                if (plan != null)
                {
                    plan.DisplayOrder = order;
                    plan.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Plans reordered: {PlanCount} plans updated", planOrders.Count);

            return true;
        }
    }
}