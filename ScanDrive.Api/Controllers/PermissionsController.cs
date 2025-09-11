using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using System.Security.Claims;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar as permissões do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Module.Administration:Permission.View")]
public class PermissionsController : BaseController
{
    private new readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Construtor do controller de permissões
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public PermissionsController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Lista todos os papéis disponíveis no sistema
    /// </summary>
    /// <returns>Lista de papéis</returns>
    /// <response code="200">Retorna a lista de papéis</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<string>> GetRoles()
    {
        return Ok(Roles.All);
    }

    /// <summary>
    /// Lista todos os módulos do sistema
    /// </summary>
    /// <returns>Lista de módulos</returns>
    /// <response code="200">Retorna a lista de módulos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("modules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<string>> GetModules()
    {
        var modules = typeof(Claims.Modules)
            .GetFields()
            .Select(f => f.GetValue(null)?.ToString())
            .Where(m => m != null);

        return Ok(modules);
    }

    /// <summary>
    /// Lista todas as permissões disponíveis no sistema
    /// </summary>
    /// <returns>Lista de permissões</returns>
    /// <response code="200">Retorna a lista de permissões</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<string>> GetPermissions()
    {
        var permissions = typeof(Claims.Permissions)
            .GetFields()
            .Select(f => f.GetValue(null)?.ToString())
            .Where(p => p != null);

        return Ok(permissions);
    }

    /// <summary>
    /// Lista todas as claims disponíveis no sistema (combinação de módulos e permissões)
    /// </summary>
    /// <returns>Lista de claims</returns>
    /// <response code="200">Retorna a lista de claims</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("available-claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<string>> GetAvailableClaims()
    {
        var modules = typeof(Claims.Modules)
            .GetFields()
            .Select(f => f.GetValue(null)?.ToString());

        var permissions = typeof(Claims.Permissions)
            .GetFields()
            .Select(f => f.GetValue(null)?.ToString());

        var claims = from module in modules
                    from permission in permissions
                    select $"{module}:{permission}";

        return Ok(claims);
    }

    /// <summary>
    /// Lista os papéis de um usuário específico
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de papéis do usuário</returns>
    /// <response code="200">Retorna a lista de papéis do usuário</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("user/{userId}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles);
    }

    /// <summary>
    /// Lista as claims de um usuário específico
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de claims do usuário</returns>
    /// <response code="200">Retorna a lista de claims do usuário</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("user/{userId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetUserClaims(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var claims = await _userManager.GetClaimsAsync(user);
        return Ok(claims.Where(c => c.Type == "Permission").Select(c => c.Value));
    }

    /// <summary>
    /// Atualiza os papéis de um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="roles">Lista de papéis</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="200">Papéis atualizados com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpPost("user/{userId}/roles")]
    [Authorize(Policy = "Module.Administration:Permission.Edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] string[] roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove roles que não estão na nova lista
        var rolesToRemove = currentRoles.Except(roles);
        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
                return BadRequest(removeResult.Errors);
        }

        // Adiciona novas roles
        var rolesToAdd = roles.Except(currentRoles);
        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
                return BadRequest(addResult.Errors);
        }

        // Atualiza as claims baseado nas novas roles
        var currentClaims = await _userManager.GetClaimsAsync(user);
        await _userManager.RemoveClaimsAsync(user, currentClaims.Where(c => c.Type == "Permission"));

        var newClaims = new List<Claim>();
        foreach (var role in roles)
        {
            if (Claims.DefaultClaims.RoleClaims.ContainsKey(role))
            {
                newClaims.AddRange(Claims.DefaultClaims.RoleClaims[role]
                    .Select(claim => new Claim("Permission", claim)));
            }
        }

        await _userManager.AddClaimsAsync(user, newClaims);

        return Ok();
    }

    /// <summary>
    /// Atualiza as claims de um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="claims">Lista de claims</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="200">Claims atualizadas com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpPost("user/{userId}/claims")]
    [Authorize(Policy = "Module.Administration:Permission.Edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserClaims(string userId, [FromBody] string[] claims)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("Usuário não encontrado");

        var currentClaims = await _userManager.GetClaimsAsync(user);
        await _userManager.RemoveClaimsAsync(user, currentClaims.Where(c => c.Type == "Permission"));

        var newClaims = claims.Select(claim => new Claim("Permission", claim));
        var result = await _userManager.AddClaimsAsync(user, newClaims);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }
} 