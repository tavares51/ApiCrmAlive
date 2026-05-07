using ApiCrmAlive.Context;
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
                   IUnitOfWork uow,
                   AppDbContext db) : IUserService
{
    private static readonly HashSet<string> AllowedRoles = [.. new[] { "admin", "gerente", "vendedor", "sdr" }];

    public async Task<UserDto> CreateAsync(UserCreateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        if (await repo.EmailExistsAsync(input.Email.Trim().ToLowerInvariant(), ct))
            throw new InvalidOperationException("E-mail já cadastrado.");

        var role = string.IsNullOrWhiteSpace(input.Role) ? "vendedor" : input.Role!.Trim().ToLowerInvariant();
        if (!AllowedRoles.Contains(role)) throw new ArgumentException("Role inválida. Use admin, gerente, vendedor ou sdr.");

        AuthHelper.CreatePasswordHash(input.Password, out var hash, out var salt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = input.Name.Trim(),
            Email = input.Email.Trim().ToLowerInvariant(),
            PasswordHash = hash,
            PasswordSalt = salt,
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
            if (!AllowedRoles.Contains(role)) throw new ArgumentException("Role inválida. Use admin, gerente, vendedor ou sdr.");
            user.Role = role;
        }

        if (input.Name is not null) user.Name = input.Name.Trim();
        if (input.Phone is not null) user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        if (input.ReceiveNotifications is not null) user.ReceiveNotifications = input.ReceiveNotifications.Value;
        if (input.Email is not null) user.Email = input.Email.Trim();

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

        var hasAssociatedLeads = await db.Leads
            .AsNoTracking()
            .AnyAsync(l => l.SellerId == id, ct);

        if (hasAssociatedLeads)
            throw new InvalidOperationException("Não é possível excluir o usuário, pois existem leads associadas a ele.");

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
        var roleNorm = string.IsNullOrWhiteSpace(role) ? null : role.Trim().ToLowerInvariant();
        return await repo.Query()
            .AsNoTracking()
            // EF Core nao traduz string.Equals(StringComparison) para SQL; normalize e compare de forma traduzivel.
            .Where(u => roleNorm == null || u.Role.ToLower() == roleNorm)
            .Where(u => isActive == null || u.IsActive == isActive.Value)
            .Where(u => search == null || u.Name.Contains(search) || u.Email.Contains(search) || (u.Phone != null && u.Phone.Contains(search)))
            .OrderBy(u => u.Name)
            .Select(u => ToDto(u))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(userId, ct);
        if (user == null)
            return false;

        var isValid = AuthHelper.VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
            throw new ArgumentException("Senha atual incorreta.");

        AuthHelper.CreatePasswordHash(newPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        repo.Update(user);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword, Guid updatedBy, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(userId, ct);
        if (user == null)
            return false;

        AuthHelper.CreatePasswordHash(newPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = updatedBy;

        repo.Update(user);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<User> GetEmailAsync(string email, CancellationToken ct = default)
    {
        return await repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
    }
}
