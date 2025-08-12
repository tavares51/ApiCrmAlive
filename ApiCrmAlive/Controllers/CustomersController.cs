using ApiCrmAlive.DTOs.Customers;
using ApiCrmAlive.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController(ICustomerService service) : ControllerBase
{
    /// <summary>GET /api/customers</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista clientes (sem paginação)")]
    [SwaggerResponse(200, "Lista de clientes", typeof(IEnumerable<CustomerDto>))]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll(CancellationToken ct = default)
        => Ok(await service.GetAllAsync(ct));

    /// <summary>GET /api/customers/:id</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtém um cliente por ID")]
    [SwaggerResponse(200, "Cliente encontrado", typeof(CustomerDto))]
    [SwaggerResponse(404, "Cliente não encontrado")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct = default)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>POST /api/customers</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria um novo cliente")]
    [SwaggerResponse(201, "Cliente criado", typeof(CustomerDto))]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(409, "CPF/E-mail já cadastrado")]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerCreateDto dto, CancellationToken ct = default)
    {
        var updatedBy = Guid.NewGuid(); // troque pelo ID do usuário autenticado
        var created = await service.CreateAsync(dto, updatedBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/customers/:id</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza dados do cliente")]
    [SwaggerResponse(200, "Cliente atualizado", typeof(CustomerDto))]
    [SwaggerResponse(404, "Cliente não encontrado")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, [FromBody] CustomerUpdateDto dto, CancellationToken ct = default)
    {
        var updatedBy = Guid.NewGuid(); // troque pelo ID do usuário autenticado
        return Ok(await service.UpdateAsync(id, dto, updatedBy, ct));
    }

    /// <summary>DELETE /api/customers/:id</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um cliente (hard delete)")]
    [SwaggerResponse(204, "Removido com sucesso")]
    [SwaggerResponse(404, "Cliente não encontrado")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>GET /api/customers/:id/purchase-history</summary>
    [HttpGet("{id:guid}/purchase-history")]
    [SwaggerOperation(Summary = "Obtém histórico de compras do cliente")]
    [SwaggerResponse(200, "Histórico de compras", typeof(IEnumerable<CustomerPurchaseDto>))]
    [SwaggerResponse(404, "Cliente não encontrado")]
    public async Task<ActionResult<IEnumerable<CustomerPurchaseDto>>> GetPurchaseHistory(Guid id, CancellationToken ct = default)
        => Ok(await service.GetPurchaseHistoryAsync(id, ct));

    /// <summary>GET /api/customers/search?q=term</summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Busca clientes por nome, CPF, e-mail ou telefone")]
    [SwaggerResponse(200, "Resultados da busca", typeof(IEnumerable<CustomerDto>))]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string q, CancellationToken ct = default)
        => Ok(await service.SearchAsync(q, ct));
}
