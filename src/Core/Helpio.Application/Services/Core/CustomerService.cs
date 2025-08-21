using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Core;

namespace Helpio.Ir.Application.Services.Core
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto?> GetByEmailAsync(string email)
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<PaginatedResult<CustomerDto>> GetCustomersAsync(PaginationRequest request)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                customers = customers.Where(c =>
                    c.FirstName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.LastName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.CompanyName != null && c.CompanyName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                customers = request.SortBy.ToLower() switch
                {
                    "firstname" => request.SortDescending ? customers.OrderByDescending(c => c.FirstName) : customers.OrderBy(c => c.FirstName),
                    "lastname" => request.SortDescending ? customers.OrderByDescending(c => c.LastName) : customers.OrderBy(c => c.LastName),
                    "email" => request.SortDescending ? customers.OrderByDescending(c => c.Email) : customers.OrderBy(c => c.Email),
                    "company" => request.SortDescending ? customers.OrderByDescending(c => c.CompanyName) : customers.OrderBy(c => c.CompanyName),
                    "createdat" => request.SortDescending ? customers.OrderByDescending(c => c.CreatedAt) : customers.OrderBy(c => c.CreatedAt),
                    _ => customers.OrderBy(c => c.Id)
                };
            }
            else
            {
                customers = customers.OrderBy(c => c.Id);
            }

            var totalItems = customers.Count();
            var items = customers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var customerDtos = _mapper.Map<List<CustomerDto>>(items);

            return new PaginatedResult<CustomerDto>
            {
                Items = customerDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto createDto)
        {
            // Check if email already exists
            var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(createDto.Email);
            if (existingCustomer != null)
            {
                throw new ArgumentException("Email already exists");
            }

            var customer = _mapper.Map<Customer>(createDto);
            var createdCustomer = await _unitOfWork.Customers.AddAsync(customer);

            _logger.LogInformation("Customer created with ID: {CustomerId}", createdCustomer.Id);

            return _mapper.Map<CustomerDto>(createdCustomer);
        }

        public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto updateDto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                throw new NotFoundException("Customer", id);
            }

            _mapper.Map(updateDto, customer);
            await _unitOfWork.Customers.UpdateAsync(customer);

            _logger.LogInformation("Customer updated with ID: {CustomerId}", id);

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                return false;
            }

            await _unitOfWork.Customers.DeleteAsync(customer);

            _logger.LogInformation("Customer deleted with ID: {CustomerId}", id);

            return true;
        }

        public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _unitOfWork.Customers.SearchCustomersAsync(searchTerm);
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersWithTicketsAsync()
        {
            var customers = await _unitOfWork.Customers.GetCustomersWithTicketsAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(email);
            return customer != null;
        }
    }
}