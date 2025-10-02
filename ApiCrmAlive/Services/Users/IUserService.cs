using ApiCrmAlive.DTOs.Users;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Services.Users;

public interface IUserService
{
    Task<UserDto> CreateAsync(UserCreateDto input, Guid updatedBy, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UserUpdateDto input, Guid updatedBy, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task ActivateAsync(Guid id, Guid updatedBy, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, Guid updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<User> GetEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(
    string? role = null,
    bool? isActive = null,
    string? search = null,
    CancellationToken ct = default);
    Task<bool> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword);
}