using ApiCrmAlive.DTOs.Users;
using ApiCrmAlive.Services.JWT;
using ApiCrmAlive.Services.Users;
using ApiCrmAlive.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCrmAlive.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController(IUserService service) : ControllerBase
{

    /// <summary>GET /api/users</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista usuários (sem paginação)")]
    [SwaggerResponse(200, "Lista de usuários", typeof(IEnumerable<UserDto>))]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var users = await service.GetAllAsync(role, isActive, search, ct);
        return Ok(users);
    }


    /// <summary>GET /api/users/:id</summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtém um usuário por ID")]
    [SwaggerResponse(200, "Usuário encontrado", typeof(UserDto))]
    [SwaggerResponse(404, "Usuário não encontrado")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    /// <summary>POST /api/users</summary>
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Cria um novo usuário")]
    [SwaggerResponse(201, "Usuário criado", typeof(UserDto))]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(409, "E-mail já cadastrado")]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid(); // trocar pelo usuário autenticado
        var created = await service.CreateAsync(dto, updatedBy, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT /api/users/:id</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Atualiza dados do usuário")]
    [SwaggerResponse(200, "Usuário atualizado", typeof(UserDto))]
    [SwaggerResponse(404, "Usuário não encontrado")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UserUpdateDto dto, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid();
        return Ok(await service.UpdateAsync(id, dto, updatedBy, ct));
    }

    /// <summary>DELETE /api/users/:id</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um usuário (hard delete)")]
    [SwaggerResponse(204, "Removido com sucesso")]
    [SwaggerResponse(404, "Usuário não encontrado")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>PATCH /api/users/:id/status</summary>
    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(Summary = "Ativa/Desativa um usuário")]
    [SwaggerResponse(204, "Atualizado")]
    [SwaggerResponse(404, "Usuário não encontrado")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive, CancellationToken ct)
    {
        var updatedBy = Guid.NewGuid();
        if (isActive) await service.ActivateAsync(id, updatedBy, ct);
        else await service.DeactivateAsync(id, updatedBy, ct);
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Autentica um cliente", Description = "Realiza login e retorna um token JWT")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto, [FromServices] JwtTokenService jwtService)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await service.GetEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Email inválido");

        var valid = AuthHelper.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt);
        if (!valid)
            return Unauthorized("Senha inválida");

        var token = jwtService.GenerateToken(user);
        return Ok(new { user.Id, token });
    }
}
