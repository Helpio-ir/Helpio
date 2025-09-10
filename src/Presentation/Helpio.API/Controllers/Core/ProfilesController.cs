using FluentValidation;
using Helpio.Ir.API.Services;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace Helpio.Ir.API.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ISupportAgentService _supportAgentService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateProfileDto> _createValidator;
        private readonly IValidator<UpdateProfileDto> _updateValidator;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(
            IProfileService profileService,
            ISupportAgentService supportAgentService,
            IOrganizationContext organizationContext,
            IValidator<CreateProfileDto> createValidator,
            IValidator<UpdateProfileDto> updateValidator,
            ILogger<ProfilesController> logger)
        {
            _profileService = profileService;
            _supportAgentService = supportAgentService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all profiles with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ProfileDto>>> GetProfiles([FromQuery] PaginationRequest request)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _profileService.GetProfilesAsync(request);

                // فیلتر پروفایل‌ها بر اساس سازمان (از طریق SupportAgent)
                if (_organizationContext.OrganizationId.HasValue)
                {
                    var filteredProfiles = new List<ProfileDto>();

                    foreach (var profile in result.Items)
                    {
                        if (await HasProfileAccessAsync(profile))
                        {
                            filteredProfiles.Add(profile);
                        }
                    }

                    result.Items = filteredProfiles;
                    result.TotalItems = filteredProfiles.Count;
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles");
                return BadRequest("Error retrieving profiles");
            }
        }

        /// <summary>
        /// Get profile by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileDto>> GetProfile(int id)
        {
            var profile = await _profileService.GetByIdAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            // بررسی دسترسی سازمانی
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasProfileAccessAsync(profile))
                {
                    return Forbid("Access denied to this profile");
                }
            }

            return Ok(profile);
        }

        /// <summary>
        /// Get profile by support agent ID
        /// </summary>
        [HttpGet("agent/{supportAgentId}")]
        public async Task<ActionResult<ProfileDto>> GetProfileByAgent(int supportAgentId)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // بررسی دسترسی به نماینده پشتیبانی
            var agent = await _supportAgentService.GetByIdAsync(supportAgentId);
            if (agent == null)
            {
                return NotFound("Support agent not found");
            }

            if (!HasAgentAccess(agent))
            {
                return Forbid("Access denied to this support agent");
            }

            var profile = await _profileService.GetBySupportAgentIdAsync(supportAgentId);
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        /// <summary>
        /// Create a new profile
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProfileDto>> CreateProfile(CreateProfileDto createDto)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _profileService.CreateAsync(createDto);

                _logger.LogInformation("Profile created: {ProfileId}", result.Id);

                return CreatedAtAction(nameof(GetProfile), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating profile");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update profile
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProfileDto>> UpdateProfile(int id, UpdateProfileDto updateDto)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // بررسی وجود پروفایل و دسترسی سازمانی
            var existingProfile = await _profileService.GetByIdAsync(id);
            if (existingProfile == null)
            {
                return NotFound();
            }

            if (_organizationContext.OrganizationId.HasValue && !await HasProfileAccessAsync(existingProfile))
            {
                return Forbid("Access denied to this profile");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _profileService.UpdateAsync(id, updateDto);

                _logger.LogInformation("Profile updated: {ProfileId}", id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile: {ProfileId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete profile
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // بررسی وجود پروفایل و دسترسی سازمانی
            var existingProfile = await _profileService.GetByIdAsync(id);
            if (existingProfile == null)
            {
                return NotFound();
            }

            if (_organizationContext.OrganizationId.HasValue && !await HasProfileAccessAsync(existingProfile))
            {
                return Forbid("Access denied to this profile");
            }

            try
            {
                var result = await _profileService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Profile deleted: {ProfileId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile: {ProfileId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Search profiles by skills
        /// </summary>
        [HttpGet("skills/{skills}")]
        public async Task<ActionResult<IEnumerable<ProfileDto>>> GetProfilesBySkills(string skills)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var profiles = await _profileService.GetBySkillsAsync(skills);

                // فیلتر بر اساس دسترسی سازمانی
                if (_organizationContext.OrganizationId.HasValue)
                {
                    var filteredProfiles = new List<ProfileDto>();

                    foreach (var profile in profiles)
                    {
                        if (await HasProfileAccessAsync(profile))
                        {
                            filteredProfiles.Add(profile);
                        }
                    }

                    profiles = filteredProfiles;
                }

                return Ok(profiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profiles by skills: {Skills}", skills);
                return BadRequest("Error retrieving profiles");
            }
        }

        /// <summary>
        /// Update last login date for profile
        /// </summary>
        [HttpPost("{id}/update-login")]
        public async Task<IActionResult> UpdateLastLoginDate(int id)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // بررسی وجود پروفایل و دسترسی سازمانی
            var existingProfile = await _profileService.GetByIdAsync(id);
            if (existingProfile == null)
            {
                return NotFound();
            }

            if (_organizationContext.OrganizationId.HasValue && !await HasProfileAccessAsync(existingProfile))
            {
                return Forbid("Access denied to this profile");
            }

            try
            {
                var result = await _profileService.UpdateLastLoginDateAsync(id);
                if (!result)
                {
                    return BadRequest("Failed to update last login date");
                }

                _logger.LogInformation("Last login date updated for profile: {ProfileId}", id);

                return Ok(new { message = "Last login date updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login date for profile: {ProfileId}", id);
                return BadRequest("Error updating last login date");
            }
        }

        /// <summary>
        /// Get profile statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetProfileStatistics()
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var allProfiles = await _profileService.GetProfilesAsync(new PaginationRequest
                {
                    PageNumber = 1,
                    PageSize = int.MaxValue
                });

                // فیلتر بر اساس دسترسی سازمانی
                var profiles = allProfiles.Items;
                if (_organizationContext.OrganizationId.HasValue)
                {
                    var filteredProfiles = new List<ProfileDto>();

                    foreach (var profile in profiles)
                    {
                        if (await HasProfileAccessAsync(profile))
                        {
                            filteredProfiles.Add(profile);
                        }
                    }

                    profiles = filteredProfiles;
                }

                var statistics = new
                {
                    TotalProfiles = profiles.Count(),
                    ProfilesWithAvatar = profiles.Count(p => !string.IsNullOrEmpty(p.Avatar)),
                    ProfilesWithBio = profiles.Count(p => !string.IsNullOrEmpty(p.Bio)),
                    ProfilesWithSkills = profiles.Count(p => !string.IsNullOrEmpty(p.Skills)),
                    ProfilesWithCertifications = profiles.Count(p => !string.IsNullOrEmpty(p.Certifications)),
                    RecentlyActive = profiles.Count(p => p.LastLoginDate.HasValue && p.LastLoginDate > DateTime.UtcNow.AddDays(-30)),
                    TopSkills = profiles
                        .Where(p => !string.IsNullOrEmpty(p.Skills))
                        .SelectMany(p => p.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim())
                        .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { Skill = g.Key, Count = g.Count() })
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile statistics");
                return BadRequest("Error retrieving statistics");
            }
        }

        /// <summary>
        /// بررسی دسترسی کاربر به پروفایل بر اساس سازمان
        /// </summary>
        private async Task<bool> HasProfileAccessAsync(ProfileDto profile)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // پیدا کردن SupportAgent مرتبط با این پروفایل
            var allAgents = await _supportAgentService.GetAgentsAsync(new PaginationRequest { PageNumber = 1, PageSize = int.MaxValue });
            var agent = allAgents.Items.FirstOrDefault(a => a.Profile?.Id == profile.Id);

            if (agent == null)
                return true; // اگر پروفایل به agent مرتبط نیست، دسترسی آزاد است

            return HasAgentAccess(agent);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به نماینده پشتیبانی بر اساس سازمان
        /// </summary>
        private bool HasAgentAccess(SupportAgentDto agent)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // بررسی دسترسی از طریق Team -> Branch -> Organization
            return agent.Team?.Branch?.OrganizationId == _organizationContext.OrganizationId.Value;
        }
    }
}