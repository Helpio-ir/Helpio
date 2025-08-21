using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto?> GetByEmailAsync(string email);
        Task<PaginatedResult<CustomerDto>> GetCustomersAsync(PaginationRequest request);
        Task<CustomerDto> CreateAsync(CreateCustomerDto createDto);
        Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
        Task<IEnumerable<CustomerDto>> GetCustomersWithTicketsAsync();
        Task<bool> EmailExistsAsync(string email);
    }
}