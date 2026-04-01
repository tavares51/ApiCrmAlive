using ApiCrmAlive.DTOs.Companies;

namespace ApiCrmAlive.Services.Companies;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken ct = default);
    Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CompanyDto> CreateAsync(CompanyCreateDto input, Guid updatedBy, CancellationToken ct = default);
    Task<CompanyDto> UpdateAsync(Guid id, CompanyPutDto input, Guid updatedBy, CancellationToken ct = default);
    Task<CompanyDto> PatchAsync(Guid id, CompanyPatchDto input, Guid updatedBy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

