using ApiCrmAlive.DTOs.Fipe;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories.Fipe;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Fipe;

public sealed class FipeModelService(
    IFipeBrandRepository brands,
    IFipeModelRepository models,
    IUnitOfWork uow) : IFipeModelService
{
    public async Task<IReadOnlyList<FipeModelDto>> GetByBrandAsync(int brandCode, string? search = null, CancellationToken ct = default)
    {
        // garante 404 "marca não encontrada" ao invés de lista vazia
        if (!await brands.ExistsAsync(brandCode, ct))
            throw new KeyNotFoundException("Marca FIPE não encontrada.");

        var q = models.Query().AsNoTracking().Where(x => x.BrandCode == brandCode);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{term}%"));
        }

        var list = await q.OrderBy(x => x.Name).ToListAsync(ct);
        return [.. list.Select(ToDto)];
    }

    public async Task<FipeModelDto> GetByCodesAsync(int brandCode, int modelCode, CancellationToken ct = default)
    {
        var entity = await models.GetByCodesAsync(brandCode, modelCode, ct)
            ?? throw new KeyNotFoundException("Modelo FIPE não encontrado.");
        return ToDto(entity);
    }

    public async Task<FipeModelDto> CreateAsync(int brandCode, FipeModelCreateDto input, CancellationToken ct = default)
    {
        if (!await brands.ExistsAsync(brandCode, ct))
            throw new KeyNotFoundException("Marca FIPE não encontrada.");

        var name = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Nome é obrigatório.");

        var modelCode = input.ModelCode;
        if (!modelCode.HasValue)
        {
            var max = await models.GetMaxModelCodeAsync(brandCode, ct);
            modelCode = (max ?? 0) + 1;
        }

        if (await models.ExistsAsync(brandCode, modelCode.Value, ct))
            throw new InvalidOperationException("Já existe um modelo com esse código para a marca.");

        var entity = new FipeModel
        {
            BrandCode = brandCode,
            ModelCode = modelCode.Value,
            Name = name
        };

        await models.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<FipeModelDto> UpdateAsync(int brandCode, int modelCode, FipeModelUpdateDto input, CancellationToken ct = default)
    {
        var entity = await models.GetByCodesAsync(brandCode, modelCode, ct)
            ?? throw new KeyNotFoundException("Modelo FIPE não encontrado.");

        var name = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Nome é obrigatório.");

        entity.Name = name;
        models.Update(entity);
        await uow.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task DeleteAsync(int brandCode, int modelCode, CancellationToken ct = default)
    {
        var entity = await models.GetByCodesAsync(brandCode, modelCode, ct)
            ?? throw new KeyNotFoundException("Modelo FIPE não encontrado.");
        models.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }

    private static FipeModelDto ToDto(FipeModel x) => new()
    {
        BrandCode = x.BrandCode,
        ModelCode = x.ModelCode,
        Name = x.Name
    };
}

