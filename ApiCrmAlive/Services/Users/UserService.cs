using ApiCrmAlive.DTOs.Users;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Users;
using ApiCrmAlive.Services;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ApiCrmAlive.Services.Users;

public class UserService(IUserRepository repo,
                   IUnitOfWork uow) : IUserService
{
    private static readonly HashSet<string> AllowedRoles = [.. new[] { "admin", "gerente", "vendedor" }];

    public async Task<UserDto> CreateAsync(UserCreateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        if (await repo.EmailExistsAsync(input.Email.Trim().ToLowerInvariant(), ct))
            throw new InvalidOperationException("E-mail já cadastrado.");

        var role = string.IsNullOrWhiteSpace(input.Role) ? "vendedor" : input.Role!.Trim().ToLowerInvariant();
        if (!AllowedRoles.Contains(role)) throw new ArgumentException("Role inválida. Use admin, gerente ou vendedor.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = input.Name.Trim(),
            Email = input.Email.Trim().ToLowerInvariant(),
            PasswordHash = PasswordHasher.Hash(input.Password),
            Role = role,
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone!.Trim(),
            ReceiveNotifications = input.ReceiveNotifications ?? true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };

        await repo.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UserUpdateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(input.Role))
        {
            var role = input.Role.Trim().ToLowerInvariant();
            if (!AllowedRoles.Contains(role)) throw new ArgumentException("Role inválida. Use admin, gerente ou vendedor.");
            user.Role = role;
        }

        if (input.Name is not null) user.Name = input.Name.Trim();
        if (input.Phone is not null) user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        if (input.ReceiveNotifications is not null) user.ReceiveNotifications = input.ReceiveNotifications.Value;

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        repo.Update(user);
        await uow.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<UserDto> UpdatePasswordAsync(Guid id, UserPasswordUpdateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!PasswordHasher.Verify(input.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha atual inválida.");

        user.PasswordHash = PasswordHasher.Hash(input.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        repo.Update(user);
        await uow.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        return ToDto(user);
    }

 
    public async Task ActivateAsync(Guid id, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        if (!user.IsActive)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;
            repo.Update(user);
            await uow.SaveChangesAsync(ct);
        }
    }

    public async Task DeactivateAsync(Guid id, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        if (user.IsActive)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;
            repo.Update(user);
            await uow.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        repo.Remove(user);
        await uow.SaveChangesAsync(ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => repo.EmailExistsAsync(email.Trim().ToLowerInvariant(), ct);

    private static UserDto ToDto(User u) => new(
        u.Id, u.Name, u.Email, u.Role, u.Phone,
        u.IsActive, u.ReceiveNotifications,
        u.CreatedAt, u.UpdatedAt, u.UpdatedBy
    );

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(string? role = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        return await repo.Query()
            .AsNoTracking()
            .Where(u => role == null || u.Role.Equals(role.Trim(), StringComparison.InvariantCultureIgnoreCase))
            .Where(u => isActive == null || u.IsActive == isActive.Value)
            .Where(u => search == null || u.Name.Contains(search) || u.Email.Contains(search) || (u.Phone != null && u.Phone.Contains(search)))
            .OrderBy(u => u.Name)
            .Select(u => ToDto(u))
            .ToListAsync(ct);
    }
}
