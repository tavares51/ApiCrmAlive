namespace ApiCrmAlive.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Cpf { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public AddressDto Address { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string? Occupation { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public string CustomerType { get; set; } = "pessoa_fisica";
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
