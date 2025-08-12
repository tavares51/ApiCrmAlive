using ApiCrmAlive.DTOs.Customers;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Customers;
using ApiCrmAlive.Mappers.Customers;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Customers;

public class CustomerService(ICustomerRepository repo, IUnitOfWork uow) : ICustomerService
{
    private readonly ICustomerRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    // GET /api/customers
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(CustomerMapper.ToDto)];
    }

    // GET /api/customers/:id
    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Cliente não encontrado.");
        return CustomerMapper.ToDto(c);
    }

    // POST /api/customers
    public async Task<CustomerDto> CreateAsync(CustomerCreateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        // unicidade
        if (await _repo.CpfExistsAsync(input.Cpf, ct))
            throw new InvalidOperationException("CPF já cadastrado.");
        if (!string.IsNullOrWhiteSpace(input.Email) && await _repo.EmailExistsAsync(input.Email, ct))
            throw new InvalidOperationException("E-mail já cadastrado.");

        var entity = CustomerMapper.FromCreateDto(input, updatedBy);

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return CustomerMapper.ToDto(entity);
    }

    // PUT /api/customers/:id
    public async Task<CustomerDto> UpdateAsync(Guid id, CustomerUpdateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        var newEmail = string.IsNullOrWhiteSpace(input.Email)
            ? null
            : input.Email.Trim().ToLowerInvariant();

        if (newEmail is not null && !string.Equals(newEmail, c.Email, StringComparison.Ordinal))
        {
            var emailEmUso = await _repo.Query()
                .AnyAsync(x => x.Id != c.Id && x.Email == newEmail, ct);

            if (emailEmUso)
                throw new InvalidOperationException("E-mail já cadastrado para outro cliente.");
        }

        CustomerMapper.UpdateEntity(c, input, updatedBy);

        _repo.Update(c);
        await _uow.SaveChangesAsync(ct);

        return CustomerMapper.ToDto(c);
    }


    // DELETE /api/customers/:id
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Cliente não encontrado.");
        _repo.Remove(c);
        await _uow.SaveChangesAsync(ct);
    }

    // GET /api/customers/search?q=term
    public async Task<IReadOnlyList<CustomerDto>> SearchAsync(string q, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return [];

        var term = q.Trim().ToLowerInvariant();

        var results = await _repo.Query()
            .AsNoTracking()
            .Where(c =>
                c.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                c.Document.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                (c.Email != null && c.Email.Contains(term, StringComparison.CurrentCultureIgnoreCase)) ||
                c.Phone.Contains(term, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(c => c.Name)
            .Take(50) // limite de segurança; ajuste se quiser
            .ToListAsync(ct);

        return [.. results.Select(CustomerMapper.ToDto)];
    }

    // GET /api/customers/:id/purchase-history
    public async Task<IReadOnlyList<CustomerPurchaseDto>> GetPurchaseHistoryAsync(Guid id, CancellationToken ct = default)
    {
        var exists = await _repo.Query().AsNoTracking().AnyAsync(c => c.Id == id, ct);
        if (!exists) throw new KeyNotFoundException("Cliente não encontrado.");

        return [];
    }
}
