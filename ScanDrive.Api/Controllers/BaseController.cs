using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller base com funcionalidades comuns para todos os controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly UserManager<IdentityUser> _userManager;
    protected readonly RoleManager<IdentityRole> _roleManager;
    protected readonly AppDbContext _context;

    protected BaseController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    /// <summary>
    /// Verifica se o usuário tem a permissão mínima necessária
    /// </summary>
    protected async Task<bool> UserHasPermission(string userId, string minimumRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var claims = await _userManager.GetClaimsAsync(user);
        var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value);

        // Se o usuário tem permissão de admin, tem todas as permissões
        if (permissions.Any(p => p.StartsWith($"{Claims.Modules.Administration}:")))
            return true;

        // Verifica a hierarquia de permissões baseado no minimumRole
        switch (minimumRole)
        {
            case Roles.User:
                return permissions.Any(p => p.StartsWith($"{Claims.Modules.Vehicles}:") ||
                                         p.StartsWith($"{Claims.Modules.Shops}:") ||
                                         p.StartsWith($"{Claims.Modules.TestDrives}:"));

            case Roles.ShopSeller:
                return permissions.Any(p => p.StartsWith($"{Claims.Modules.Vehicles}:") ||
                                         p.StartsWith($"{Claims.Modules.Shops}:") ||
                                         p.StartsWith($"{Claims.Modules.Leads}:") ||
                                         p.StartsWith($"{Claims.Modules.TestDrives}:"));

            case Roles.ShopOwner:
                return permissions.Any(p => p.StartsWith($"{Claims.Modules.Shops}:") &&
                                         p.EndsWith($":{Claims.Permissions.Edit}"));

            default:
                return false;
        }
    }

    /// <summary>
    /// Obtém o ID do usuário atual
    /// </summary>
    /// <returns>ID do usuário ou null se não estiver autenticado</returns>
    protected string? GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? 
               User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }

    /// <summary>
    /// Obtém o usuário atual
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Lançada quando o usuário não está autenticado ou não foi encontrado</exception>
    protected async Task<IdentityUser> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        return user;
    }

    /// <summary>
    /// Verifica se o usuário atual é admin ou membro da loja
    /// </summary>
    protected async Task<bool> IsAdminOrShopMember(Guid shopId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var claims = await _userManager.GetClaimsAsync(user);
        var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value);

        // Se tem permissão de admin, tem acesso
        if (permissions.Any(p => p.StartsWith($"{Claims.Modules.Administration}:")))
            return true;

        // Verifica se é dono ou vendedor da loja
        var shop = await _context.Shops
            .Include(s => s.Owner)
            .Include(s => s.Sellers)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            return false;

        if (shop.OwnerId == userId)
            return true;

        // Verifica se é vendedor da loja e tem permissões de vendedor
        return shop.Sellers.Any(s => s.Id == userId) && 
               permissions.Any(p => p.StartsWith($"{Claims.Modules.Shops}:") ||
                                 p.StartsWith($"{Claims.Modules.Vehicles}:") ||
                                 p.StartsWith($"{Claims.Modules.Leads}:") ||
                                 p.StartsWith($"{Claims.Modules.TestDrives}:"));
    }
} 