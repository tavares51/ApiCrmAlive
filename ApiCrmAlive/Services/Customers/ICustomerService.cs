using ApiCrmAlive.DTOs.Customers;

namespace ApiCrmAlive.Services.Customers;

public interface ICustomerService
{
    // GET /api/customers
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default);

    // GET /api/customers/:id
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    // POST /api/customers
    Task<CustomerDto> CreateAsync(CustomerCreateDto input, Guid updatedBy, CancellationToken ct = default);

    // PUT /api/customers/:id
    Task<CustomerDto> UpdateAsync(Guid id, CustomerUpdateDto input, Guid updatedBy, CancellationToken ct = default);

    // DELETE /api/customers/:id
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // GET /api/customers/search?q=term
    Task<IReadOnlyList<CustomerDto>> SearchAsync(string q, CancellationToken ct = default);

    // GET /api/customers/:id/purchase-history
    Task<IReadOnlyList<CustomerPurchaseDto>> GetPurchaseHistoryAsync(Guid id, CancellationToken ct = default);
}
