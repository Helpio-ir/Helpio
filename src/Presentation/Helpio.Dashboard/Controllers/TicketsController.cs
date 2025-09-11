using Helpio.Dashboard.Services;
using Helpio.Dashboard.Models;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tickets = await GetAccessibleTicketsAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var categories = await GetAccessibleCategoriesAsync();
            var customers = await GetAccessibleCustomersAsync();
            var teams = await GetAccessibleTeamsAsync();

            // Debug: Check if essential data exists
            if (!categories.Any())
            {
                TempData["Warning"] = "هیچ دسته‌بندی تیکت وجود ندارد. لطفا ابتدا دسته‌بندی ایجاد کنید.";
            }

            if (!customers.Any())
            {
                TempData["Warning"] = (TempData["Warning"] ?? "") + " هیچ مشتری وجود ندارد. لطفا ابتدا مشتری ایجاد کنید.";
            }

            if (!teams.Any())
            {
                TempData["Warning"] = (TempData["Warning"] ?? "") + " هیچ تیمی وجود ندارد. لطفا ابتدا تیم ایجاد کنید.";
            }

            ViewBag.Categories = categories;
            ViewBag.Customers = customers;
            ViewBag.Teams = teams;
            
            return View(new CreateTicketDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketDto dto)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Map DTO to Entity
                    var ticket = new Ticket
                    {
                        Title = dto.Title,
                        Description = dto.Description,
                        CustomerId = dto.CustomerId,
                        TicketCategoryId = dto.TicketCategoryId,
                        Priority = dto.Priority,
                        DueDate = dto.DueDate,
                        EstimatedHours = dto.EstimatedHours,
                        CreatedAt = DateTime.UtcNow,
                        TicketStateId = 1 // Default to "Open"
                    };

                    // Set TeamId based on user role and organization
                    if (!IsCurrentUserAdmin && CurrentTeamId.HasValue)
                    {
                        ticket.TeamId = CurrentTeamId.Value;
                    }
                    else if (dto.TeamId.HasValue && dto.TeamId.Value > 0)
                    {
                        ticket.TeamId = dto.TeamId.Value;
                    }
                    else
                    {
                        // If no team specified, try to assign to a default team
                        var defaultTeam = await GetDefaultTeamAsync();
                        if (defaultTeam != null)
                        {
                            ticket.TeamId = defaultTeam.Id;
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(dto.TeamId), "هیچ تیمی برای تخصیص تیکت یافت نشد. لطفا با مدیر سیستم تماس بگیرید.");
                            ViewBag.Categories = await GetAccessibleCategoriesAsync();
                            ViewBag.Customers = await GetAccessibleCustomersAsync();
                            ViewBag.Teams = await GetAccessibleTeamsAsync();
                            return View(dto);
                        }
                    }

                    _context.Tickets.Add(ticket);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تیکت با موفقیت ایجاد شد.";
                    return RedirectToAction(nameof(Details), new { id = ticket.Id });
                }
                catch (Exception ex)
                {
                    // Log the exception
                    TempData["Error"] = "خطا در ایجاد تیکت. لطفا مجدداً تلاش کنید.";
                    // For debugging - remove in production
                    ModelState.AddModelError("", $"خطا: {ex.Message}");
                }
            }
            else
            {
                // Debug: Show validation errors
                TempData["Debug"] = "";
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            TempData["Debug"] += $"{modelError.Key}: {error.ErrorMessage}\n";
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Categories = await GetAccessibleCategoriesAsync();
            ViewBag.Customers = await GetAccessibleCustomersAsync();
            ViewBag.Teams = await GetAccessibleTeamsAsync();

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            // Check permissions for editing
            if (!IsCurrentUserAdmin && !IsCurrentUserManager &&
                !(IsCurrentUserAgent && ticket.SupportAgentId == UserContext.CurrentSupportAgent?.Id))
            {
                return Forbid();
            }

            ViewBag.Categories = await GetAccessibleCategoriesAsync();
            ViewBag.Agents = await GetAccessibleAgentsAsync();
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

            var existingTicket = await GetTicketByIdAsync(id);
            if (existingTicket == null || !CanAccessTicket(existingTicket))
            {
                return NotFound();
            }

            // Check permissions for editing
            if (!IsCurrentUserAdmin && !IsCurrentUserManager &&
                !(IsCurrentUserAgent && existingTicket.SupportAgentId == UserContext.CurrentSupportAgent?.Id))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only allowed fields
                    existingTicket.Title = ticket.Title;
                    existingTicket.Description = ticket.Description;
                    existingTicket.Priority = ticket.Priority;
                    existingTicket.TicketCategoryId = ticket.TicketCategoryId;
                    existingTicket.DueDate = ticket.DueDate;
                    existingTicket.EstimatedHours = ticket.EstimatedHours;

                    // Only Admin/Manager can change assignment
                    if (IsCurrentUserAdmin || IsCurrentUserManager)
                    {
                        existingTicket.SupportAgentId = ticket.SupportAgentId;
                    }

                    // Only set resolution if provided
                    if (!string.IsNullOrEmpty(ticket.Resolution))
                    {
                        existingTicket.Resolution = ticket.Resolution;
                    }

                    existingTicket.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تیکت با موفقیت بروزرسانی شد.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExistsAsync(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = await GetAccessibleCategoriesAsync();
            ViewBag.Agents = await GetAccessibleAgentsAsync();
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            if (UserContext.CurrentSupportAgent == null)
            {
                return BadRequest("شما کارشناس پشتیبانی نیستید.");
            }

            ticket.SupportAgentId = UserContext.CurrentSupportAgent.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تیکت به شما اختصاص داده شد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(int id, string content)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "محتوای پاسخ نمی‌تواند خالی باشد.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var response = new Response
            {
                TicketId = id,
                UserId = UserContext.UserId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            TempData["Success"] = "پاسخ با موفقیت ثبت شد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int id, string content)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "محتوای یادداشت نمی‌تواند خالی باشد.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (UserContext.CurrentSupportAgent == null)
            {
                return BadRequest("فقط کارشناسان می‌توانند یادداشت اضافه کنند.");
            }

            var note = new Note
            {
                TicketId = id,
                SupportAgentId = UserContext.CurrentSupportAgent.Id,
                Description = content,
                IsPrivate = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            TempData["Success"] = "یادداشت با موفقیت ثبت شد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<List<Ticket>> GetAccessibleTicketsAsync()
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                .AsQueryable();

            if (IsCurrentUserAdmin)
            {
                // Admin sees all tickets
                return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            }
            else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                // Manager sees organization tickets
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }
            else if (IsCurrentUserAgent)
            {
                // Agent sees only assigned tickets or team tickets
                query = query.Where(t =>
                    t.SupportAgentId == UserContext.CurrentSupportAgent!.Id ||
                    (t.TeamId == CurrentTeamId && t.SupportAgentId == null));
            }
            else
            {
                return new List<Ticket>();
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        private async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                .Include(t => t.Responses)
                    .ThenInclude(r => r.User)
                .Include(t => t.Notes)
                    .ThenInclude(n => n.SupportAgent)
                        .ThenInclude(sa => sa.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        private bool CanAccessTicket(Ticket ticket)
        {
            if (IsCurrentUserAdmin) return true;

            if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                return ticket.Team?.Branch?.OrganizationId == CurrentOrganizationId.Value;
            }

            if (IsCurrentUserAgent)
            {
                return ticket.SupportAgentId == UserContext.CurrentSupportAgent?.Id ||
                       (ticket.TeamId == CurrentTeamId && ticket.SupportAgentId == null);
            }

            return false;
        }

        private async Task<List<TicketCategory>> GetAccessibleCategoriesAsync()
        {
            var query = _context.TicketCategories.AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(tc => tc.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.ToListAsync();
        }

        private async Task<List<Customer>> GetAccessibleCustomersAsync()
        {
            var query = _context.Customers.AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                // Filter customers based on organization access
                query = query.Where(c => c.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.Take(100).ToListAsync();
        }

        private async Task<List<Team>> GetAccessibleTeamsAsync()
        {
            var query = _context.Teams
                .Include(t => t.Branch)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.ToListAsync();
        }

        private async Task<List<SupportAgent>> GetAccessibleAgentsAsync()
        {
            var query = _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(sa => sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.ToListAsync();
        }

        private async Task<bool> TicketExistsAsync(int id)
        {
            return await _context.Tickets.AnyAsync(e => e.Id == id);
        }

        private async Task<Team?> GetDefaultTeamAsync()
        {
            var query = _context.Teams
                .Include(t => t.Branch)
                .Where(t => t.IsActive)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Action for testing - creates sample data if missing
        /// Remove this in production
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSampleData()
        {
            if (!IsCurrentUserAdmin)
            {
                return Forbid();
            }

            try
            {
                // Create sample customer if none exists
                if (!await _context.Customers.AnyAsync())
                {
                    var sampleCustomer = new Customer
                    {
                        FirstName = "احمد",
                        LastName = "محمدی",
                        Email = "ahmad@example.com",
                        PhoneNumber = "09123456789",
                        CompanyName = "شرکت نمونه",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Customers.Add(sampleCustomer);
                }

                // Create sample ticket category if none exists
                if (!await _context.TicketCategories.AnyAsync())
                {
                    var sampleCategory = new TicketCategory
                    {
                        Name = "پشتیبانی فنی",
                        Description = "مسائل فنی و پشتیبانی",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TicketCategories.Add(sampleCategory);
                }

                // Create sample ticket state if none exists
                if (!await _context.TicketStates.AnyAsync())
                {
                    var ticketStates = new[]
                    {
                        new TicketState { Name = "Open", Description = "باز", CreatedAt = DateTime.UtcNow },
                        new TicketState { Name = "In Progress", Description = "در حال پیگیری", CreatedAt = DateTime.UtcNow },
                        new TicketState { Name = "Closed", Description = "بسته شده", CreatedAt = DateTime.UtcNow }
                    };
                    _context.TicketStates.AddRange(ticketStates);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "داده‌های نمونه با موفقیت ایجاد شدند.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"خطا در ایجاد داده‌های نمونه: {ex.Message}";
            }

            return RedirectToAction(nameof(Create));
        }
    }
}