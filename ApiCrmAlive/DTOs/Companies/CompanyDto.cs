namespace ApiCrmAlive.DTOs.Companies;

public sealed class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Cnpj { get; set; } = default!;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public CompanyAddressDto Address { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Website { get; set; }
    public bool HasSdr { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
