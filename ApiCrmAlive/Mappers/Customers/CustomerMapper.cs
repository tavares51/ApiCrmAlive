using System.Text.Json;
using ApiCrmAlive.DTOs.Customers;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Mappers.Customers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(Customer c)
    {
        return new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Cpf = c.Document,
            Phone = c.Phone,
            Email = c.Email,
            Address = DeserializeAddress(c.Address),
            BirthDate = c.BirthDate,
            Occupation = c.Occupation,
            MonthlyIncome = c.MonthlyIncome ?? 0m,
            CustomerType = c.CustomerType,
            TotalPurchases = c.TotalPurchases,
            TotalSpent = c.TotalSpent,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy
        };
    }

    public static Customer FromCreateDto(CustomerCreateDto dto, Guid updatedBy)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Document = dto.Cpf.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
            Address = dto.Address is null ? null : SerializeAddress(dto.Address),
            BirthDate = dto.BirthDate?.Date,
            Occupation = string.IsNullOrWhiteSpace(dto.Occupation) ? null : dto.Occupation.Trim(),
            MonthlyIncome = dto.MonthlyIncome,
            LastPurchaseDate = null,
            TotalPurchases = 0,
            TotalSpent = 0m,
            CustomerType = string.IsNullOrWhiteSpace(dto.CustomerType) ? "pessoa_fisica" : dto.CustomerType.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy
        };
    }

    public static void UpdateEntity(Customer entity, CustomerUpdateDto dto, Guid updatedBy)
    {
        entity.Name = dto.Name.Trim();
        entity.Phone = dto.Phone.Trim();
        entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        entity.Address = SerializeAddress(dto.Address);
        entity.BirthDate = dto.BirthDate?.Date;
        entity.Occupation = string.IsNullOrWhiteSpace(dto.Occupation) ? null : dto.Occupation.Trim();
        entity.MonthlyIncome = dto.MonthlyIncome;
        entity.CustomerType = string.IsNullOrWhiteSpace(dto.CustomerType) ? entity.CustomerType : dto.CustomerType.Trim().ToLowerInvariant();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;
    }

    private static JsonDocument? SerializeAddress(AddressDto? dto)
    {
        if (dto is null) return null;
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

    private static AddressDto DeserializeAddress(JsonDocument? json)
    {
        if (json is null) return new AddressDto();
        try
        {
            var root = json.RootElement;
            return new AddressDto
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
            return new AddressDto();
        }
    }
}
