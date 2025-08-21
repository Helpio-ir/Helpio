using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Core;
using System.Security.Cryptography;
using System.Text;

namespace Helpio.Ir.Application.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserService> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<PaginatedResult<UserDto>> GetUsersAsync(PaginationRequest request)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                users = users.Where(u => 
                    u.FirstName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                users = request.SortBy.ToLower() switch
                {
                    "firstname" => request.SortDescending ? users.OrderByDescending(u => u.FirstName) : users.OrderBy(u => u.FirstName),
                    "lastname" => request.SortDescending ? users.OrderByDescending(u => u.LastName) : users.OrderBy(u => u.LastName),
                    "email" => request.SortDescending ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    "createdat" => request.SortDescending ? users.OrderByDescending(u => u.CreatedAt) : users.OrderBy(u => u.CreatedAt),
                    _ => users.OrderBy(u => u.Id)
                };
            }
            else
            {
                users = users.OrderBy(u => u.Id);
            }

            var totalItems = users.Count();
            var items = users
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var userDtos = _mapper.Map<List<UserDto>>(items);

            return new PaginatedResult<UserDto>
            {
                Items = userDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createDto)
        {
            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(createDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = HashPassword(createDto.Password);

            var createdUser = await _unitOfWork.Users.AddAsync(user);
            
            _logger.LogInformation("User created with ID: {UserId}", createdUser.Id);
            
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User", id);
            }

            _mapper.Map(updateDto, user);
            await _unitOfWork.Users.UpdateAsync(user);

            _logger.LogInformation("User updated with ID: {UserId}", id);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _unitOfWork.Users.DeleteAsync(user);
            
            _logger.LogInformation("User deleted with ID: {UserId}", id);
            
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _unitOfWork.Users.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _unitOfWork.Users.GetActiveUsersAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Here you would generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24); // 24 hour expiration

            _logger.LogInformation("User logged in: {UserEmail}", user.Email);

            return new UserLoginResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!VerifyPassword(oldPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _unitOfWork.Users.UpdateAsync(user);

            _logger.LogInformation("Password changed for user: {UserId}", userId);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                return false; // Don't reveal if email exists
            }

            // Generate temporary password or reset token
            var tempPassword = GenerateTemporaryPassword();
            user.PasswordHash = HashPassword(tempPassword);
            await _unitOfWork.Users.UpdateAsync(user);

            // Here you would send email with temporary password
            _logger.LogInformation("Password reset for user: {UserEmail}", email);

            return true;
        }

        #region Private Methods

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SALT"));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        private static string GenerateJwtToken(User user)
        {
            // JWT token generation would be implemented here
            // For now, return a placeholder
            return $"jwt_token_for_{user.Id}_{DateTime.UtcNow.Ticks}";
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}