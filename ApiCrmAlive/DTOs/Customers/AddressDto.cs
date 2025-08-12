namespace ApiCrmAlive.DTOs.Customers;

public class AddressDto
{
    public string Street { get; set; } = default!;
    public string Number { get; set; } = default!;
    public string Neighborhood { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
}
