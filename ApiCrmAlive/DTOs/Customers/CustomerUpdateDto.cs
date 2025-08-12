namespace ApiCrmAlive.DTOs.Customers;

public class CustomerUpdateDto
{
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public AddressDto Address { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string? Occupation { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public string CustomerType { get; set; } = "pessoa_fisica";
}
