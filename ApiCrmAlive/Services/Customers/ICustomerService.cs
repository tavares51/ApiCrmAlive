using ApiCrmAlive.DTOs.Customers;

namespace ApiCrmAlive.Services.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default);

    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CustomerDto> CreateAsync(CustomerCreateDto input, Guid updatedBy, CancellationToken ct = default);

    Task<CustomerDto> UpdateAsync(Guid id, CustomerUpdateDto input, Guid updatedBy, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CustomerDto>> SearchAsync(string q, CancellationToken ct = default);

    Task<IReadOnlyList<CustomerPurchaseDto>> GetPurchaseHistoryAsync(Guid id, CancellationToken ct = default);
}
