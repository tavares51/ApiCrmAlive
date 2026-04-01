using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Companies;

public interface ICompanyRepository : IRepository<Company>
{
    Task<bool> CnpjExistsAsync(string normalizedCnpj, CancellationToken ct = default);
    Task<Company?> GetByCnpjAsync(string normalizedCnpj, CancellationToken ct = default);
}

