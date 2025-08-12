using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Customers;

public class CustomerRepository(AppDbContext ctx) : Repository<Customer>(ctx), ICustomerRepository
{
    public Task<bool> CpfExistsAsync(string cpf, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(c => c.Document == cpf, ct);

    public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(c => c.Document == cpf, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(c => c.Email != null && c.Email == email, ct);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(c => c.Email == email, ct);
}