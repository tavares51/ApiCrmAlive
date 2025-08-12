namespace ApiCrmAlive.DTOs.Customers;

public class CustomerPurchaseDto
{
    public Guid Id { get; set; }                 // id do pedido/compra
    public DateTime Date { get; set; }           // data da compra
    public decimal Amount { get; set; }          // valor total
    public string? Description { get; set; }     // opcional: observações/resumo
}
