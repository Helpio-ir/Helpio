using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProfileController(
            ICurrentUserContext userContext,
            ApplicationDbContext context,
            UserManager<User> userManager)
            : base(userContext)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (UserContext.UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(UserContext.UserId.ToString()!);
            if (user == null)
            {
                return NotFound();
            }

            var supportAgent = await _context.SupportAgents
                .Include(sa => sa.Profile)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                        .ThenInclude(b => b.Organization)
                .FirstOrDefaultAsync(sa => sa.UserId == user.Id);

            var model = new ProfileViewModel
            {
                User = user,
                SupportAgent = supportAgent,
                UserRoles = await _userManager.GetRolesAsync(user)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (UserContext.UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(UserContext.UserId.ToString()!);
            if (user == null)
            {
                return NotFound();
            }

            var supportAgent = await _context.SupportAgents
                .Include(sa => sa.Profile)
                .FirstOrDefaultAsync(sa => sa.UserId == user.Id);

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Bio = supportAgent?.Profile?.Bio ?? "",
                Skills = supportAgent?.Profile?.Skills ?? "",
                Avatar = supportAgent?.Profile?.Avatar ?? "default.png"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (UserContext.UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(UserContext.UserId.ToString()!);
                if (user == null)
                {
                    return NotFound();
                }

                // Update user info
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update profile if exists
                    var supportAgent = await _context.SupportAgents
                        .Include(sa => sa.Profile)
                        .FirstOrDefaultAsync(sa => sa.UserId == user.Id);

                    if (supportAgent?.Profile != null)
                    {
                        supportAgent.Profile.Bio = model.Bio;
                        supportAgent.Profile.Skills = model.Skills;
                        supportAgent.Profile.Avatar = model.Avatar;
                        supportAgent.Profile.UpdatedAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = "پروفایل شما با موفقیت به‌روزرسانی شد.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }

    public class ProfileViewModel
    {
        public User User { get; set; } = null!;
        public SupportAgent? SupportAgent { get; set; }
        public IList<string> UserRoles { get; set; } = new List<string>();
    }

    public class EditProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Avatar { get; set; } = "default.png";
    }
}