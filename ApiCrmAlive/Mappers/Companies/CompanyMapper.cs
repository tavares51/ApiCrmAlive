using System.Text.Json;
using ApiCrmAlive.DTOs.Companies;
using ApiCrmAlive.Models;
using ApiCrmAlive.Utils;

namespace ApiCrmAlive.Mappers.Companies;

public static class CompanyMapper
{
    public static CompanyDto ToDto(Company c)
    {
        return new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            Cnpj = c.Cnpj,
            Description = c.Description,
            Logo = c.Logo,
            Address = DeserializeAddress(c.Address),
            Phone = c.Phone,
            Email = c.Email,
            Website = c.Website,
            HasSdr = c.HasSdr,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy
        };
    }

    public static Company FromCreateDto(CompanyCreateDto dto, Guid updatedBy)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Cnpj = CnpjUtils.Normalize(dto.Cnpj),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Logo = string.IsNullOrWhiteSpace(dto.Logo) ? null : dto.Logo.Trim(),
            Address = dto.Address is null ? null : SerializeAddress(dto.Address),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website.Trim(),
            HasSdr = dto.HasSdr,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
    }

    public static void UpdateEntity(Company entity, CompanyPutDto dto, Guid updatedBy)
    {
        entity.Name = dto.Name.Trim();
        entity.Cnpj = CnpjUtils.Normalize(dto.Cnpj);
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.Logo = string.IsNullOrWhiteSpace(dto.Logo) ? null : dto.Logo.Trim();
        entity.Address = dto.Address is null ? null : SerializeAddress(dto.Address);
        entity.Phone = dto.Phone.Trim();
        entity.Email = dto.Email.Trim().ToLowerInvariant();
        entity.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website.Trim();
        entity.HasSdr = dto.HasSdr;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }

    public static void PatchEntity(Company entity, CompanyPatchDto dto, Guid updatedBy)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Cnpj))
            entity.Cnpj = CnpjUtils.Normalize(dto.Cnpj);

        if (dto.Description != null)
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (dto.Logo != null)
            entity.Logo = string.IsNullOrWhiteSpace(dto.Logo) ? null : dto.Logo.Trim();

        if (dto.Address != null)
            entity.Address = SerializeAddress(dto.Address);

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            entity.Phone = dto.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Email))
            entity.Email = dto.Email.Trim().ToLowerInvariant();

        if (dto.Website != null)
            entity.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website.Trim();

        if (dto.HasSdr.HasValue)
            entity.HasSdr = dto.HasSdr.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }

    private static JsonDocument? SerializeAddress(CompanyAddressDto dto)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            street = dto.Street,
            number = dto.Number,
            neighborhood = dto.Neighborhood,
            city = dto.City,
            state = dto.State,
            zip_code = dto.ZipCode
        }));
    }

    private static CompanyAddressDto DeserializeAddress(JsonDocument? json)
    {
        if (json is null) return new CompanyAddressDto();
        try
        {
            var root = json.RootElement;
            return new CompanyAddressDto
            {
                Street = root.GetProperty("street").GetString() ?? string.Empty,
                Number = root.GetProperty("number").GetString() ?? string.Empty,
                Neighborhood = root.GetProperty("neighborhood").GetString() ?? string.Empty,
                City = root.GetProperty("city").GetString() ?? string.Empty,
                State = root.GetProperty("state").GetString() ?? string.Empty,
                ZipCode = root.GetProperty("zip_code").GetString() ?? string.Empty
            };
        }
        catch
        {
            return new CompanyAddressDto();
        }
    }
}
