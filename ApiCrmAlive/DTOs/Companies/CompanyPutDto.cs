namespace ApiCrmAlive.DTOs.Companies;

public sealed class CompanyPutDto
{
    public string Name { get; set; } = default!;
    public string Cnpj { get; set; } = default!;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public CompanyAddressDto Address { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Website { get; set; }
}

