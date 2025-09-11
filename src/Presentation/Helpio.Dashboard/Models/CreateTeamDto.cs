using System.ComponentModel.DataAnnotations;

namespace Helpio.Dashboard.Models
{
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "نام تیم الزامی است")]
        [StringLength(100, ErrorMessage = "نام تیم نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "انتخاب شاخه الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "لطفا شاخه را انتخاب کنید")]
        public int BranchId { get; set; }

        public int? TeamLeadId { get; set; }

        public int? SupervisorId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}