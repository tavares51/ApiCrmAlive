namespace ApiCrmAlive.DTOs.Companies;

public sealed class CompanyPatchDto
{
    public string? Name { get; set; }
    public string? Cnpj { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public CompanyAddressDto? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

