using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class CustomerDto : BaseDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public int TotalTickets { get; set; }
        
        // ????? ?? ???? ??? SaaS
        public int? OrganizationId { get; set; }
        public OrganizationDto? Organization { get; set; }
    }

    public class CreateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        
        // ????? ?? - ????? ???? ?? ??????? ???? ????? ????
        public int? OrganizationId { get; set; }
    }

    public class UpdateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        
        // OrganizationId ????????? ????? ???
    }
}