using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar os papéis (roles) do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Module.Administration:Permission.View")]
public class RolesController : BaseController
{
    private new readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Construtor do controller de papéis
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public RolesController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Lista todos os papéis em formato simplificado
    /// </summary>
    /// <returns>Lista de papéis em formato de lista</returns>
    /// <response code="200">Retorna a lista de papéis</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("list-items")]
    [Authorize(Policy = "Module.Administration:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ListItemDto>>> GetListItems()
    {
        var roles = await _roleManager.Roles
            .Select(r => new ListItemDto
            {
                Id = r.Id,
                Description = r.Name ?? r.Id
            })
            .ToListAsync();

        return Ok(roles);
    }
} 