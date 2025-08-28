using ApiCrmAlive.DTOs.Marketplaces;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Marketplaces;

public static class MarketplaceMapper
{
    public static Marketplace FromCreateDto(MarketplaceCreateDto dto, Guid userId)
    {
        return new Marketplace
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId
        };
    }

    public static void UpdateEntity(Marketplace entity, MarketplaceUpdateDto dto, Guid userId)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;

        if (dto.Description is not null)
            entity.Description = dto.Description;

        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
    }

    public static MarketplaceDto ToDto(Marketplace entity)
    {
        return new MarketplaceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            Configurations = entity.Configurations?
                .Select(MarketplaceConfigurationMapper.ToDto)
                .ToList() ?? new List<MarketplaceConfigurationDto>()
        };
    }
}
