using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Settings;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.DTOs.User;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar os usuários do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize(Policy = "Module.Administration:Permission.View")]
public class UsersController : BaseController
{
    private new readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Construtor do controller de usuários
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Lista todos os usuários do sistema
    /// </summary>
    /// <returns>Lista de usuários</returns>
    /// <response code="200">Retorna a lista de usuários</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.EmailConfirmed,
                u.PhoneNumber,
                u.PhoneNumberConfirmed,
                u.LockoutEnabled,
                u.LockoutEnd
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Obtém um usuário específico pelo ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Dados do usuário</returns>
    /// <response code="200">Retorna os dados do usuário</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.LockoutEnabled,
            user.LockoutEnd,
            Roles = roles,
            Claims = claims.Where(c => c.Type == "Permission").Select(c => c.Value)
        });
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    /// <param name="request">Dados do usuário</param>
    /// <returns>Dados do usuário criado</returns>
    /// <response code="201">Usuário criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpPost]
    //[Authorize(Policy = "Module.Administration:Permission.Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (request.Roles?.Any() == true)
        {
            result = await _userManager.AddToRolesAsync(user, request.Roles);
            if (!result.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(result.Errors);
            }

            // Adiciona as claims padrão das roles
            var claims = new List<System.Security.Claims.Claim>();
            foreach (var role in request.Roles)
            {
                if (Claims.DefaultClaims.RoleClaims.ContainsKey(role))
                {
                    claims.AddRange(Claims.DefaultClaims.RoleClaims[role]
                        .Select(claim => new System.Security.Claims.Claim("Permission", claim)));
                }
            }

            if (claims.Any())
            {
                result = await _userManager.AddClaimsAsync(user, claims);
                if (!result.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return BadRequest(result.Errors);
                }
            }
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <param name="request">Dados do usuário</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Usuário atualizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Module.Administration:Permission.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuário não encontrado");

        user.Email = request.Email ?? user.Email;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.LockoutEnabled = request.LockoutEnabled ?? user.LockoutEnabled;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (!string.IsNullOrEmpty(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
        }

        return NoContent();
    }

    /// <summary>
    /// Remove um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Usuário removido com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Module.Administration:Permission.Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Lista todos os usuários em formato simplificado
    /// </summary>
    /// <returns>Lista de usuários em formato de lista</returns>
    /// <response code="200">Retorna a lista de usuários</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("list-items")]
    [Authorize(Policy = "Module.Administration:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ListItemDto>>> GetListItems()
    {
        var users = await _userManager.Users
            .Select(u => new ListItemDto
            {
                Id = u.Id,
                Description = u.Email ?? u.UserName ?? u.Id
            })
            .ToListAsync();

        return Ok(users);
    }
}

public class CreateUserRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string[] Roles { get; set; }
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
} 