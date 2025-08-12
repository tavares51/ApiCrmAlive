using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Users
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    }
}
