using ApiCrmAlive.DTOs.Companies;
using ApiCrmAlive.Mappers.Companies;
using ApiCrmAlive.Repositories.Companies;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Companies;

public sealed class CompanyService(ICompanyRepository repo, IUnitOfWork uow) : ICompanyService
{
    private readonly ICompanyRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<IReadOnlyList<CompanyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(CompanyMapper.ToDto)];
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Empresa não encontrada.");
        return CompanyMapper.ToDto(c);
    }

    public async Task<CompanyDto> CreateAsync(CompanyCreateDto input, Guid updatedBy, CancellationToken ct = default)
    {
        ValidateRequired(input.Name, nameof(input.Name));
        ValidateRequired(input.Cnpj, nameof(input.Cnpj));
        ValidateRequired(input.Phone, nameof(input.Phone));
        ValidateRequired(input.Email, nameof(input.Email));
        if (input.Address is null) throw new ArgumentException("Address é obrigatório.", nameof(input.Address));

        var normalizedCnpj = CnpjUtils.Normalize(input.Cnpj);
        if (!CnpjUtils.IsValid(normalizedCnpj))
            throw new ArgumentException("CNPJ inválido.", nameof(input.Cnpj));

        if (await _repo.CnpjExistsAsync(normalizedCnpj, ct))
            throw new InvalidOperationException("CNPJ já cadastrado.");

        var entity = CompanyMapper.FromCreateDto(input, updatedBy);
        entity.Cnpj = normalizedCnpj;

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return CompanyMapper.ToDto(entity);
    }

    public async Task<CompanyDto> UpdateAsync(Guid id, CompanyPutDto input, Guid updatedBy, CancellationToken ct = default)
    {
        ValidateRequired(input.Name, nameof(input.Name));
        ValidateRequired(input.Cnpj, nameof(input.Cnpj));
        ValidateRequired(input.Phone, nameof(input.Phone));
        ValidateRequired(input.Email, nameof(input.Email));
        if (input.Address is null) throw new ArgumentException("Address é obrigatório.", nameof(input.Address));

        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Empresa não encontrada.");

        var normalizedCnpj = CnpjUtils.Normalize(input.Cnpj);
        if (!CnpjUtils.IsValid(normalizedCnpj))
            throw new ArgumentException("CNPJ inválido.", nameof(input.Cnpj));

        if (!string.Equals(normalizedCnpj, entity.Cnpj, StringComparison.Ordinal))
        {
            var cnpjEmUso = await _repo.Query()
                .AsNoTracking()
                .AnyAsync(x => x.Id != entity.Id && x.Cnpj == normalizedCnpj, ct);

            if (cnpjEmUso)
                throw new InvalidOperationException("CNPJ já cadastrado para outra empresa.");
        }

        CompanyMapper.UpdateEntity(entity, input, updatedBy);
        entity.Cnpj = normalizedCnpj;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return CompanyMapper.ToDto(entity);
    }

    public async Task<CompanyDto> PatchAsync(Guid id, CompanyPatchDto input, Guid updatedBy, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Empresa não encontrada.");

        if (input.Cnpj != null)
        {
            var normalizedCnpj = CnpjUtils.Normalize(input.Cnpj);
            if (!string.IsNullOrEmpty(normalizedCnpj) && !CnpjUtils.IsValid(normalizedCnpj))
                throw new ArgumentException("CNPJ inválido.", nameof(input.Cnpj));

            if (!string.IsNullOrEmpty(normalizedCnpj) && !string.Equals(normalizedCnpj, entity.Cnpj, StringComparison.Ordinal))
            {
                var cnpjEmUso = await _repo.Query()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id != entity.Id && x.Cnpj == normalizedCnpj, ct);

                if (cnpjEmUso)
                    throw new InvalidOperationException("CNPJ já cadastrado para outra empresa.");
            }
        }

        CompanyMapper.PatchEntity(entity, input, updatedBy);

        // required fields cannot be patched to empty
        ValidateRequired(entity.Name, nameof(entity.Name));
        ValidateRequired(entity.Cnpj, nameof(entity.Cnpj));
        ValidateRequired(entity.Phone, nameof(entity.Phone));
        ValidateRequired(entity.Email, nameof(entity.Email));

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return CompanyMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Empresa não encontrada.");
        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }

    private static void ValidateRequired(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} é obrigatório.", field);
    }
}

