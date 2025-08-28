using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Mappers.Marketplaces;
using ApiCrmAlive.Repositories.Marketplaces;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Marketplaces;

public class MarketplaceService(IMarketplaceRepository repo, IUnitOfWork uow) : IMarketplaceService
{
    private readonly IMarketplaceRepository _repo = repo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<MarketplaceDto> CreateAsync(MarketplaceCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        var entity = MarketplaceMapper.FromCreateDto(dto, userId);
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return MarketplaceMapper.ToDto(entity);
    }

    public async Task<MarketplaceDto?> UpdateAsync(Guid id, MarketplaceUpdateDto dto, Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Marketplace não encontrado.");

        MarketplaceMapper.UpdateEntity(entity, dto, userId);

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return MarketplaceMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Marketplace não encontrado.");
        _repo.Remove(entity);

        var affected = await _uow.SaveChangesAsync(ct);
        if (affected == 0)
            throw new InvalidOperationException("Não foi possível excluir o marketplace.");
    }

    public async Task<MarketplaceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.Query()
            .Include(m => m.Configurations)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return entity is null ? throw new KeyNotFoundException("Marketplace não encontrado.") : MarketplaceMapper.ToDto(entity);
    }

    public async Task<IReadOnlyList<MarketplaceDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.Query()
            .Include(m => m.Configurations)
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return [.. list.Select(MarketplaceMapper.ToDto)];
    }

    public async Task<MarketplaceVehicleSyncResponseDto> SyncVehicleAsync(Guid id, Guid vehicleId, CancellationToken ct)
    {
        await Task.Delay(300, ct);
        throw new NotImplementedException();
    }

    public async Task<MarketplaceVehicleSyncResponseDto> SyncAllVehiclesAsync(CancellationToken ct = default)
    {
        await Task.Delay(300, ct);
        throw new NotImplementedException();
    }

    public async Task<MarketplaceSyncStatusDto> GetSyncStatusAsync(Guid id, CancellationToken ct = default)
    {
        var marketplace = await _repo.GetByIdAsync(id, ct)
         ?? throw new KeyNotFoundException("Marketplace não encontrado.");

        return new MarketplaceSyncStatusDto
        {
            MarketplaceId = marketplace.Id,
            MarketplaceName = marketplace.Name,
            LastSyncAt = DateTime.UtcNow.AddMinutes(-5),
            IsSyncInProgress = false,
            LastSyncMessage = "Última sincronização concluída com sucesso",
            TotalVehiclesSynced = 15,
            TotalVehiclesFailed = 1
        };
    }

    public Task<PaginatedResponseDto<MarketplaceLogDto>> GetLogsAsync(Guid id, int page = 1, int limit = 20, CancellationToken ct = default)
    {
        //var (logs, total) = await _repo.GetLogsAsync(id, page, limit, ct);
        //return new PaginatedResponseDto<MarketplaceLogDto>(logs.Select(MarketplaceLogMapper.ToDto), total, page, limit);
        throw new NotImplementedException();
    }
}
