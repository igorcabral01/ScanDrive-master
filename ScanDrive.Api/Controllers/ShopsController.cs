using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar as lojas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopsController : BaseController
{
    /// <summary>
    /// Construtor do controller de lojas
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public ShopsController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
    }

    /// <summary>
    /// Lista todas as lojas ativas
    /// </summary>
    /// <returns>Lista de lojas</returns>
    /// <response code="200">Retorna a lista de lojas</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<Shop>>> GetShops()
    {
        return await _context.Shops
            .Where(s => s.IsActive)
            .Include(s => s.Owner)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém uma loja específica pelo ID
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <returns>Dados da loja</returns>
    /// <response code="200">Retorna os dados da loja</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Loja não encontrada</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "Module.Shops:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Shop>> GetShop(Guid id)
    {
        var shop = await _context.Shops
            .Include(s => s.Owner)
            .Include(s => s.Sellers)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (shop == null)
        {
            return NotFound();
        }

        return shop;
    }

    /// <summary>
    /// Cria uma nova loja
    /// </summary>
    /// <param name="shop">Dados da loja</param>
    /// <returns>Dados da loja criada</returns>
    /// <response code="201">Loja criada com sucesso</response>
    /// <response code="400">Dados inválidos ou usuário já possui uma loja</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost]
    [Authorize(Policy = "Module.Shops:Permission.Create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Shop>> CreateShop(Shop shop)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário já tem uma loja (exceto para Admin)
        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var existingShop = await _context.Shops
                .AnyAsync(s => s.OwnerId == userId && s.IsActive);
            
            if (existingShop)
                return BadRequest("Shop owner can only have one active shop");
        }

        shop.Id = Guid.NewGuid();
        shop.OwnerId = userId;
        shop.CreatedAt = DateTime.UtcNow;
        shop.IsActive = true;

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetShop), new { id = shop.Id }, shop);
    }

    /// <summary>
    /// Atualiza uma loja existente
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <param name="shop">Novos dados da loja</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Loja atualizada com sucesso</response>
    /// <response code="400">ID inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "Module.Shops:Permission.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShop(Guid id, Shop shop)
    {
        if (id != shop.Id)
            return BadRequest();

        var existingShop = await _context.Shops.FindAsync(id);
        if (existingShop == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário é o dono da loja ou admin
        if (existingShop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        existingShop.Name = shop.Name;
        existingShop.Description = shop.Description;
        existingShop.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ShopExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Remove (soft delete) uma loja
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Loja removida com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Module.Shops:Permission.Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShop(Guid id)
    {
        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário é o dono da loja ou admin
        if (shop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        shop.IsDeleted = true;
        shop.IsActive = false;
        shop.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Adiciona um vendedor à loja
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <param name="sellerId">ID do usuário vendedor</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Vendedor adicionado com sucesso</response>
    /// <response code="400">Vendedor já está na loja</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja ou vendedor não encontrado</response>
    [HttpPost("{id}/sellers")]
    [Authorize(Policy = "Module.Shops:Permission.Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSeller(Guid id, [FromBody] string sellerId)
    {
        var shop = await _context.Shops
            .Include(s => s.Sellers)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (shop == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário é o dono da loja ou admin
        if (shop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        var seller = await _userManager.FindByIdAsync(sellerId);
        if (seller == null)
            return NotFound("Seller not found");

        // Verifica se o usuário já é um seller
        if (shop.Sellers.Any(s => s.Id == sellerId))
            return BadRequest("User is already a seller in this shop");

        // Adiciona o papel de ShopSeller ao usuário
        await _userManager.AddToRoleAsync(seller, Roles.ShopSeller);

        shop.Sellers.Add(seller);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove um vendedor da loja
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <param name="sellerId">ID do usuário vendedor</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Vendedor removido com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja ou vendedor não encontrado</response>
    [HttpDelete("{id}/sellers/{sellerId}")]
    [Authorize(Policy = "Module.Shops:Permission.Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSeller(Guid id, string sellerId)
    {
        var shop = await _context.Shops
            .Include(s => s.Sellers)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (shop == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário é o dono da loja ou admin
        if (shop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        var seller = shop.Sellers.FirstOrDefault(s => s.Id == sellerId);
        if (seller == null)
            return NotFound("Seller not found in this shop");

        shop.Sellers.Remove(seller);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Lista todas as lojas em formato simplificado
    /// </summary>
    /// <returns>Lista de lojas em formato de lista</returns>
    /// <response code="200">Retorna a lista de lojas</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("list-items")]
    [Authorize(Policy = "Module.Shops:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ListItemDto>>> GetListItems()
    {
        var shops = await _context.Shops
            .Where(s => !s.IsDeleted && s.IsActive)
            .Select(s => new ListItemDto
            {
                Id = s.Id.ToString(),
                Description = s.Name
            })
            .ToListAsync();

        return Ok(shops);
    }

    /// <summary>
    /// Atualiza o limite de QR codes de uma loja
    /// </summary>
    /// <param name="id">ID da loja</param>
    /// <param name="request">Novo limite de QR codes</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="204">Limite atualizado com sucesso</response>
    /// <response code="400">Limite inválido (máximo 100)</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [HttpPut("{id}/qr-code-limit")]
    [Authorize(Policy = "Module.Shops:Permission.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQRCodeLimit(Guid id, [FromBody] UpdateQRCodeLimitRequest request)
    {
        if (request.QRCodeLimit < 1 || request.QRCodeLimit > 100)
            return BadRequest("QR code limit must be between 1 and 100");

        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Verifica se o usuário é o dono da loja ou admin
        if (shop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        shop.QRCodeLimit = request.QRCodeLimit;
        shop.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ShopExists(Guid id)
    {
        return _context.Shops.Any(e => e.Id == id);
    }
}

public class UpdateQRCodeLimitRequest
{
    public int QRCodeLimit { get; set; }
}