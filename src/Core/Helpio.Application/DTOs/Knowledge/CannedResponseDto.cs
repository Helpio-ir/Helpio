using System.ComponentModel.DataAnnotations;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Knowledge
{
    public class CannedResponseDto : BaseDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }

        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }

        // Computed Properties
        public string[] TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        public string ShortContent => Content.Length > 100 ? Content[..100] + "..." : Content;
    }

    public class CreateCannedResponseDto
    {
        [Required(ErrorMessage = "انتخاب سازمان الزامی است")]
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "نام پاسخ آماده الزامی است")]
        [StringLength(200, ErrorMessage = "نام پاسخ آماده نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "محتوای پاسخ الزامی است")]
        [StringLength(5000, ErrorMessage = "محتوای پاسخ نمی‌تواند بیش از 5000 کاراکتر باشد")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "برچسب‌ها نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string? Tags { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCannedResponseDto
    {
        [Required(ErrorMessage = "نام پاسخ آماده الزامی است")]
        [StringLength(200, ErrorMessage = "نام پاسخ آماده نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "محتوای پاسخ الزامی است")]
        [StringLength(5000, ErrorMessage = "محتوای پاسخ نمی‌تواند بیش از 5000 کاراکتر باشد")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "برچسب‌ها نمی‌تواند بیش از 200 کاراکتر باشد")]
        public string? Tags { get; set; }

        public bool IsActive { get; set; }
    }
}