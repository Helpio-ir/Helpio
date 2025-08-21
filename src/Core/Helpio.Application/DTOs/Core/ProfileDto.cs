using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class ProfileDto : BaseDto
    {
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
    }

    public class CreateProfileDto
    {
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
    }
}