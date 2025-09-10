using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetByIdAsync(int id);
        Task<ProfileDto?> GetBySupportAgentIdAsync(int supportAgentId);
        Task<PaginatedResult<ProfileDto>> GetProfilesAsync(PaginationRequest request);
        Task<ProfileDto> CreateAsync(CreateProfileDto createDto);
        Task<ProfileDto> UpdateAsync(int id, UpdateProfileDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ProfileDto>> GetBySkillsAsync(string skills);
        Task<bool> UpdateLastLoginDateAsync(int profileId);
    }
}