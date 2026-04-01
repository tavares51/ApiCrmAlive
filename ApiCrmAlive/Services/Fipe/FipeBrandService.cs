using ApiCrmAlive.DTOs.Fipe;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Fipe;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Fipe;

public sealed class FipeBrandService(IFipeBrandRepository brands, IUnitOfWork uow) : IFipeBrandService
{
    public async Task<IReadOnlyList<FipeBrandDto>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var q = brands.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{term}%"));
        }

        var list = await q.OrderBy(x => x.Name).ToListAsync(ct);
        return [.. list.Select(ToDto)];
    }

    public async Task<FipeBrandDto> GetByCodeAsync(int brandCode, CancellationToken ct = default)
    {
        var entity = await brands.GetByCodeAsync(brandCode, ct) ?? throw new KeyNotFoundException("Marca FIPE não encontrada.");
        return ToDto(entity);
    }

    public async Task<FipeBrandDto> CreateAsync(FipeBrandCreateDto input, CancellationToken ct = default)
    {
        var name = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Nome é obrigatório.");

        if (input.BrandCode.HasValue && await brands.ExistsAsync(input.BrandCode.Value, ct))
            throw new InvalidOperationException("Já existe uma marca com esse código.");

        var entity = new FipeBrand
        {
            BrandCode = input.BrandCode ?? 0, // 0 -> identidade do banco (IdentityByDefault)
            Name = name
        };

        await brands.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<FipeBrandDto> UpdateAsync(int brandCode, FipeBrandUpdateDto input, CancellationToken ct = default)
    {
        var entity = await brands.GetByCodeAsync(brandCode, ct) ?? throw new KeyNotFoundException("Marca FIPE não encontrada.");

        var name = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Nome é obrigatório.");

        entity.Name = name;
        brands.Update(entity);
        await uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task DeleteAsync(int brandCode, CancellationToken ct = default)
    {
        var entity = await brands.GetByCodeAsync(brandCode, ct) ?? throw new KeyNotFoundException("Marca FIPE não encontrada.");
        brands.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    private static FipeBrandDto ToDto(FipeBrand x) => new()
    {
        BrandCode = x.BrandCode,
        Name = x.Name
    };
}

