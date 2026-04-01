using ApiCrmAlive.Context;
using ApiCrmAlive.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Repositories.Companies;

public sealed class CompanyRepository(AppDbContext ctx) : Repository<Company>(ctx), ICompanyRepository
{
    public Task<bool> CnpjExistsAsync(string normalizedCnpj, CancellationToken ct = default)
        => _db.AsNoTracking().AnyAsync(c => c.Cnpj == normalizedCnpj, ct);

    public Task<Company?> GetByCnpjAsync(string normalizedCnpj, CancellationToken ct = default)
        => _db.FirstOrDefaultAsync(c => c.Cnpj == normalizedCnpj, ct);
}

