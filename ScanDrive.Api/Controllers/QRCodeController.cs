using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScanDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QRCodeController : BaseController
{
    private readonly ILogger<QRCodeController> _logger;
    private readonly IConfiguration _configuration;

    public QRCodeController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ILogger<QRCodeController> logger,
        IConfiguration configuration)
        : base(userManager, roleManager, context)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtém todos os QR codes de uma loja
    /// </summary>
    [HttpGet("shop/{shopId}")]
    [Authorize(Policy = "Module.Shops:Permission.View")]
    public async Task<IActionResult> GetShopQRCodes(Guid shopId)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var shop = await _context.Shops.FindAsync(shopId);
        if (shop == null)
            return NotFound();

        // Verifica se o usuário é dono da loja ou admin
        if (!User.IsInRole(Roles.Admin) && shop.OwnerId != user.Id)
            return Forbid();

        var qrCodes = await _context.QRCodes
            .Where(q => q.ShopId == shopId && !q.IsDeleted)
            .Select(q => new
            {
                q.Id,
                q.RedirectType,
                q.RedirectId,
                q.CreatedAt,
                q.UpdatedAt
            })
            .ToListAsync();

        return Ok(qrCodes);
    }

    /// <summary>
    /// Cria um novo QR code para uma loja
    /// </summary>
    [HttpPost("shop/{shopId}")]
    [Authorize(Policy = "Module.Shops:Permission.Edit")]
    public async Task<IActionResult> CreateQRCode(Guid shopId, [FromBody] CreateQRCodeRequest request)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var shop = await _context.Shops.FindAsync(shopId);
        if (shop == null)
            return NotFound();

        // Verifica se o usuário é dono da loja ou admin
        if (!User.IsInRole(Roles.Admin) && shop.OwnerId != user.Id)
            return Forbid();

        // Verifica o limite de QR codes
        var currentQRCodes = await _context.QRCodes
            .CountAsync(q => q.ShopId == shopId && !q.IsDeleted);

        if (currentQRCodes >= shop.QRCodeLimit)
            return BadRequest($"Shop has reached the maximum limit of {shop.QRCodeLimit} QR codes");

        var qrCode = new QRCode
        {
            ShopId = shopId,
            RedirectType = request.RedirectType,
            RedirectId = request.RedirectId
        };

        _context.QRCodes.Add(qrCode);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQRCodeImage), new { id = qrCode.Id }, new
        {
            qrCode.Id,
            qrCode.RedirectType,
            qrCode.RedirectId,
            qrCode.CreatedAt,
            qrCode.UpdatedAt
        });
    }

    /// <summary>
    /// Atualiza o redirecionamento de um QR code
    /// </summary>
    [HttpPut("{id}/redirect")]
    [Authorize(Policy = "Module.Shops:Permission.Edit")]
    public async Task<IActionResult> UpdateQRCodeRedirect(Guid id, [FromBody] UpdateQRCodeRequest request)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var qrCode = await _context.QRCodes
            .Include(q => q.Shop)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (qrCode == null)
            return NotFound();

        // Verifica se o usuário é dono da loja ou admin
        if (!User.IsInRole(Roles.Admin) && qrCode.Shop?.OwnerId != user.Id)
            return Forbid();

        qrCode.RedirectType = request.RedirectType;
        qrCode.RedirectId = request.RedirectId;
        qrCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Exclui um QR code
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Module.Shops:Permission.Edit")]
    public async Task<IActionResult> DeleteQRCode(Guid id)
    {
        var user = await GetCurrentUser();
        if (user == null)
            return Unauthorized();

        var qrCode = await _context.QRCodes
            .Include(q => q.Shop)
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (qrCode == null)
            return NotFound();

        // Verifica se o usuário é dono da loja ou admin
        if (!User.IsInRole(Roles.Admin) && qrCode.Shop?.OwnerId != user.Id)
            return Forbid();

        qrCode.IsDeleted = true;
        qrCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtém a imagem do QR code
    /// </summary>
    [HttpGet("{id}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQRCodeImage(Guid id)
    {
        var qrCode = await _context.QRCodes
            .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

        if (qrCode == null)
            return NotFound();

        string redirectUrl;
        switch (qrCode.RedirectType)
        {
            case QRCodeRedirectType.Vehicle:
                var vehicle = await _context.Vehicles.FindAsync(qrCode.RedirectId);
                if (vehicle == null)
                    return NotFound();
                redirectUrl = $"/vehicles/{vehicle.Id}";
                break;
            case QRCodeRedirectType.Shop:
                var shop = await _context.Shops.FindAsync(qrCode.RedirectId);
                if (shop == null)
                    return NotFound();
                redirectUrl = $"/shops/{shop.Id}";
                break;
            default:
                return BadRequest("Invalid redirect type");
        }

        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(redirectUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCodeImage = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCodeImage.GetGraphic(20);

        return File(qrCodeBytes, "image/png");
    }
}

public class CreateQRCodeRequest
{
    public QRCodeRedirectType RedirectType { get; set; }
    public Guid RedirectId { get; set; }
}

public class UpdateQRCodeRequest
{
    public QRCodeRedirectType RedirectType { get; set; }
    public Guid RedirectId { get; set; }
} 