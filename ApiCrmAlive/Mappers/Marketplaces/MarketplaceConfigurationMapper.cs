using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers;

public static class MarketplaceConfigurationMapper
{
    public static MarketplaceConfigurationDto ToDto(MarketplaceConfiguration entity) => new()
    {
        Id = entity.Id,
        MarketplaceId = entity.MarketplaceId,
        ApiKey = entity.ApiKey,
        AccountId = entity.AccountId,
        StoreId = entity.StoreId,
        ConnectionStatus = entity.ConnectionStatus,
        LastTestDate = entity.LastTestDate,
        LastSyncDate = entity.LastSyncDate,
        AutoSyncEnabled = entity.AutoSyncEnabled,
        ConfigurationData = entity.ConfigurationData,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };

    public static MarketplaceConfiguration FromCreateDto(Guid marketplaceId, MarketplaceConfigurationCreateDto dto, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        MarketplaceId = marketplaceId,
        ApiKey = dto.ApiKey,
        AccountId = dto.AccountId,
        StoreId = dto.StoreId,
        AutoSyncEnabled = dto.AutoSyncEnabled,
        ConfigurationData = dto.ConfigurationData,
        ConnectionStatus = "desconectado",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        UpdatedBy = userId
    };

    public static void UpdateEntity(MarketplaceConfiguration entity, MarketplaceConfigurationUpdateDto dto, Guid userId)
    {
        entity.ApiKey = dto.ApiKey;
        entity.AccountId = dto.AccountId;
        entity.StoreId = dto.StoreId;
        entity.AutoSyncEnabled = dto.AutoSyncEnabled;
        entity.ConfigurationData = dto.ConfigurationData;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
    }
}
