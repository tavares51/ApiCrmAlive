using ApiCrmAlive.Context;
using ApiCrmAlive.Models;

namespace ApiCrmAlive.Repositories.Sales;

public class SaleRepository(AppDbContext context) : Repository<Sale>(context), ISaleRepository
{
   
}
