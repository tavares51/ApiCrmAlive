using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Customers;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> CpfExistsAsync(string cpf, CancellationToken ct = default);
    Task<Customer?> GetByCpfAsync(string cpf, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
}
