using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using CoreProfile = Helpio.Ir.Domain.Entities.Core.Profile;

namespace Helpio.Ir.Application.Services.Core
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileService> _logger;
        private readonly IDateTime _dateTime;

        public ProfileService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProfileService> logger,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ProfileDto?> GetByIdAsync(int id)
        {
            var profile = await _unitOfWork.Profiles.GetByIdAsync(id);
            return profile != null ? _mapper.Map<ProfileDto>(profile) : null;
        }

        public async Task<ProfileDto?> GetBySupportAgentIdAsync(int supportAgentId)
        {
            var profile = await _unitOfWork.Profiles.GetBySupportAgentIdAsync(supportAgentId);
            return profile != null ? _mapper.Map<ProfileDto>(profile) : null;
        }

        public async Task<PaginatedResult<ProfileDto>> GetProfilesAsync(PaginationRequest request)
        {
            var profiles = await _unitOfWork.Profiles.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                profiles = profiles.Where(p =>
                    p.Bio.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Skills.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Certifications.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            profiles = request.SortBy?.ToLower() switch
            {
                "bio" => request.SortDescending ? profiles.OrderByDescending(p => p.Bio) : profiles.OrderBy(p => p.Bio),
                "skills" => request.SortDescending ? profiles.OrderByDescending(p => p.Skills) : profiles.OrderBy(p => p.Skills),
                "lastlogindate" => request.SortDescending ? profiles.OrderByDescending(p => p.LastLoginDate) : profiles.OrderBy(p => p.LastLoginDate),
                "createdat" => request.SortDescending ? profiles.OrderByDescending(p => p.CreatedAt) : profiles.OrderBy(p => p.CreatedAt),
                _ => profiles.OrderBy(p => p.Id)
            };

            var totalItems = profiles.Count();
            var items = profiles
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var profileDtos = _mapper.Map<List<ProfileDto>>(items);

            return new PaginatedResult<ProfileDto>
            {
                Items = profileDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ProfileDto> CreateAsync(CreateProfileDto createDto)
        {
            var profile = _mapper.Map<CoreProfile>(createDto);
            var createdProfile = await _unitOfWork.Profiles.AddAsync(profile);

            _logger.LogInformation("Profile created with ID: {ProfileId}", createdProfile.Id);

            return _mapper.Map<ProfileDto>(createdProfile);
        }

        public async Task<ProfileDto> UpdateAsync(int id, UpdateProfileDto updateDto)
        {
            var profile = await _unitOfWork.Profiles.GetByIdAsync(id);
            if (profile == null)
            {
                throw new NotFoundException("Profile", id);
            }

            _mapper.Map(updateDto, profile);
            await _unitOfWork.Profiles.UpdateAsync(profile);

            _logger.LogInformation("Profile updated with ID: {ProfileId}", id);

            return _mapper.Map<ProfileDto>(profile);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var profile = await _unitOfWork.Profiles.GetByIdAsync(id);
            if (profile == null)
            {
                return false;
            }

            await _unitOfWork.Profiles.DeleteAsync(profile);

            _logger.LogInformation("Profile deleted with ID: {ProfileId}", id);

            return true;
        }

        public async Task<IEnumerable<ProfileDto>> GetBySkillsAsync(string skills)
        {
            var profiles = await _unitOfWork.Profiles.GetBySkillsAsync(skills);
            return _mapper.Map<IEnumerable<ProfileDto>>(profiles);
        }

        public async Task<bool> UpdateLastLoginDateAsync(int profileId)
        {
            var profile = await _unitOfWork.Profiles.GetByIdAsync(profileId);
            if (profile == null)
            {
                return false;
            }

            profile.LastLoginDate = _dateTime.UtcNow;
            await _unitOfWork.Profiles.UpdateAsync(profile);

            return true;
        }
    }
}