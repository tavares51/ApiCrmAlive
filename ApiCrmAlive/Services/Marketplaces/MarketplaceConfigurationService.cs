using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Mappers;
using ApiCrmAlive.Mappers.Marketplaces;
using ApiCrmAlive.Repositories.Marketplaces;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Services.Marketplaces;

public class MarketplaceConfigurationService(
    IMarketplaceRepository marketplaceRepo,
    IMarketplaceConfigurationRepository configRepo,
    IMarketplaceLogRepository logRepo,
    IUnitOfWork uow
) : IMarketplaceConfigurationService
{
    private readonly IMarketplaceRepository _marketplaceRepo = marketplaceRepo;
    private readonly IMarketplaceConfigurationRepository _configRepo = configRepo;
    private readonly IMarketplaceLogRepository _logRepo = logRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<MarketplaceConfigurationDto?> GetByMarketplaceIdAsync(Guid marketplaceId, CancellationToken ct = default)
    {
        var config = await _configRepo.GetByMarketplaceIdAsync(marketplaceId, ct);
        return config is null ? null : MarketplaceConfigurationMapper.ToDto(config);
    }

    public async Task<TestConnectionResultDto> TestConnectionAsync(Guid marketplaceId, CancellationToken ct = default)
    {
        var config = await _configRepo.GetByMarketplaceIdAsync(marketplaceId, ct)
            ?? throw new KeyNotFoundException("Configuração não encontrada.");

        // 🔹 Simulação de chamada externa (mock)
        var result = new TestConnectionResultDto
        {
            Status = "sucesso",
            Message = "Conexão estabelecida com sucesso",
            Details = new TestConnectionDetailsDto
            {
                ResponseTimeMs = 250,
                ApiVersion = "v2.1",
                AccountVerified = true
            }
        };

        config.ConnectionStatus = "conectado";
        config.LastTestDate = DateTime.UtcNow;
        _configRepo.Update(config);
        await _uow.SaveChangesAsync(ct);

        return result;
    }

    public async Task<VehicleSyncResultDto> SyncVehicleAsync(Guid marketplaceId, Guid vehicleId, CancellationToken ct = default)
    {
        var marketplace = await _marketplaceRepo.GetByIdAsync(marketplaceId, ct)
            ?? throw new KeyNotFoundException("Marketplace não encontrado.");

        // 🔹 Simulação de sync externo
        var result = new VehicleSyncResultDto
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            MarketplaceId = marketplaceId,
            ExternalId = "ML123456789",
            SyncStatus = "enviado",
            LastSyncDate = DateTime.UtcNow
        };

        return result;
    }

    public async Task SyncAllVehiclesAsync(Guid marketplaceId, CancellationToken ct = default)
    {
        // 🔹 Mock de sincronização em massa
        await Task.Delay(500, ct);
    }

    public async Task<IReadOnlyList<MarketplaceLogDto>> GetLogsAsync(Guid marketplaceId, int page = 1, int limit = 20, CancellationToken ct = default)
    {
        var query = _logRepo.Query()
            .Where(l => l.MarketplaceId == marketplaceId)
            .OrderByDescending(l => l.CreatedAt);

        var logs = await query.Skip((page - 1) * limit)
                              .Take(limit)
                              .ToListAsync(ct);

        return [.. logs.Select(MarketplaceLogMapper.ToDto)];
    }

    public async Task<MarketplaceConfigurationDto> CreateAsync(Guid marketplaceId, MarketplaceConfigurationCreateDto dto, Guid userId, CancellationToken ct = default)
    {
        _ = await _marketplaceRepo.GetByIdAsync(marketplaceId, ct) ?? throw new KeyNotFoundException("Marketplace não encontrado.");

        var config = await _configRepo.GetByMarketplaceIdAsync(marketplaceId, ct);
        
        if (config == null)
        {
            config = MarketplaceConfigurationMapper.FromCreateDto(marketplaceId, dto, userId);
            await _configRepo.AddAsync(config, ct);
        }

        await _uow.SaveChangesAsync(ct);
        
        return MarketplaceConfigurationMapper.ToDto(config);
    }

    public async Task<MarketplaceConfigurationDto> UpdateAsync(Guid marketplaceId, MarketplaceConfigurationUpdateDto dto, Guid userId, CancellationToken ct = default)
    {
        _ = await _marketplaceRepo.GetByIdAsync(marketplaceId, ct) ?? throw new KeyNotFoundException("Marketplace não encontrado.");

        var config = await _configRepo.GetByMarketplaceIdAsync(marketplaceId, ct) ?? throw new KeyNotFoundException("Configuração não encontrada.");

        MarketplaceConfigurationMapper.UpdateEntity(entity: config, dto: dto, userId: userId);
        _configRepo.Update(config);

        await _uow.SaveChangesAsync(ct);

        return MarketplaceConfigurationMapper.ToDto(config);
    }
}
