using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Users;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Users;

public class UserRepository(AppDbContext ctx) : Repository<User>(ctx), IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(u => u.Email == email, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(u => u.Email == email, ct);
}
