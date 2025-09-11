using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Reports;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar os relatórios e agendamentos do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    /// <summary>
    /// Construtor do controller de relatórios
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de funções do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public ReportsController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
    }

    /// <summary>
    /// Obtém o relatório de uma loja específica
    /// </summary>
    /// <param name="shopId">ID da loja</param>
    /// <param name="sellerId">ID do vendedor (opcional)</param>
    /// <param name="startDate">Data inicial do período (opcional)</param>
    /// <param name="endDate">Data final do período (opcional)</param>
    /// <returns>Relatório da loja</returns>
    /// <response code="200">Retorna o relatório da loja</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [HttpGet("shops/{shopId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ShopOwner}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShopReportDto>> GetShopReport(Guid shopId, [FromQuery] string? sellerId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var shop = await _context.Shops
            .Include(s => s.Sellers)
            .FirstOrDefaultAsync(s => s.Id == shopId && !s.IsDeleted);

        if (shop == null)
            return NotFound("Shop not found");

        // Verifica permissão
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (shop.OwnerId != userId && !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        var query = _context.Vehicles
            .Where(v => v.ShopId == shopId && !v.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(v => v.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(v => v.CreatedAt <= endDate.Value);

        var vehicles = await query.ToListAsync();

        var report = new ShopReportDto
        {
            ShopId = shop.Id.ToString(),
            ShopName = shop.Name,
            TotalVehicles = vehicles.Count,
            SoldVehicles = vehicles.Count(v => v.IsSold),
            ReservedVehicles = await _context.VehicleReservations
                .CountAsync(r => r.ShopId == shopId && r.IsActive && !r.IsCancelled),
            TotalRevenue = vehicles.Where(v => v.IsSold).Sum(v => v.Price),
            TotalProfit = vehicles.Where(v => v.IsSold).Sum(v => v.Price * 0.1m) // Exemplo: 10% de lucro
        };

        // Relatórios por vendedor
        if (!string.IsNullOrEmpty(sellerId))
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller != null && shop.Sellers.Any(s => s.Id == sellerId))
            {
                var sellerVehicles = vehicles.Where(v => v.CreatedById == sellerId || v.LastUpdatedById == sellerId);
                report.SellerReports.Add(new SellerReportDto
                {
                    SellerId = seller.Id,
                    SellerName = seller.UserName ?? seller.Email ?? seller.Id,
                    VehiclesSold = sellerVehicles.Count(v => v.IsSold),
                    Revenue = sellerVehicles.Where(v => v.IsSold).Sum(v => v.Price),
                    Profit = sellerVehicles.Where(v => v.IsSold).Sum(v => v.Price * 0.1m)
                });
            }
        }
        else
        {
            foreach (var seller in shop.Sellers)
            {
                var sellerVehicles = vehicles.Where(v => v.CreatedById == seller.Id || v.LastUpdatedById == seller.Id);
                report.SellerReports.Add(new SellerReportDto
                {
                    SellerId = seller.Id,
                    SellerName = seller.UserName ?? seller.Email ?? seller.Id,
                    VehiclesSold = sellerVehicles.Count(v => v.IsSold),
                    Revenue = sellerVehicles.Where(v => v.IsSold).Sum(v => v.Price),
                    Profit = sellerVehicles.Where(v => v.IsSold).Sum(v => v.Price * 0.1m)
                });
            }
        }

        return Ok(report);
    }

    /// <summary>
    /// Cria uma nova reserva de veículo
    /// </summary>
    /// <param name="request">Dados da reserva</param>
    /// <returns>ID da reserva criada</returns>
    /// <response code="200">Reserva criada com sucesso</response>
    /// <response code="400">Veículo já está vendido ou reservado</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Veículo não encontrado</response>
    [HttpPost("reservations")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReservation(Domain.DTOs.Reports.VehicleReservationDto request)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Shop)
            .FirstOrDefaultAsync(v => v.Id == Guid.Parse(request.VehicleId) && !v.IsDeleted);

        if (vehicle == null)
            return NotFound("Vehicle not found");

        if (vehicle.IsSold)
            return BadRequest("Vehicle is already sold");

        if (await _context.VehicleReservations.AnyAsync(r => 
            r.VehicleId == vehicle.Id && 
            r.IsActive && 
            !r.IsCancelled))
            return BadRequest("Vehicle is already reserved");

        var userId = GetCurrentUserId();
        var reservation = new VehicleReservation
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            ShopId = vehicle.ShopId,
            CustomerId = userId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ReservationDate = request.ReservationDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.VehicleReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Ok(new { Id = reservation.Id });
    }

    /// <summary>
    /// Cria um novo agendamento de test drive
    /// </summary>
    /// <param name="request">Dados do test drive</param>
    /// <returns>ID do test drive criado</returns>
    /// <response code="200">Test drive criado com sucesso</response>
    /// <response code="400">Veículo já está vendido ou horário já está agendado</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Veículo não encontrado</response>
    [HttpPost("test-drives")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTestDrive(Domain.DTOs.Reports.TestDriveDto request)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Shop)
            .FirstOrDefaultAsync(v => v.Id == Guid.Parse(request.VehicleId) && !v.IsDeleted);

        if (vehicle == null)
            return NotFound("Vehicle not found");

        if (vehicle.IsSold)
            return BadRequest("Vehicle is already sold");

        // Verifica se já existe um test drive agendado para o mesmo horário
        if (await _context.TestDrives.AnyAsync(t => 
            t.VehicleId == vehicle.Id && 
            t.IsActive && 
            !t.IsCancelled &&
            !t.IsCompleted &&
            t.PreferredDate == request.PreferredDate))
            return BadRequest("There is already a test drive scheduled for this time");

        var userId = GetCurrentUserId();
        var testDrive = new TestDrive
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            ShopId = vehicle.ShopId,
            CustomerId = userId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            PreferredDate = request.PreferredDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TestDrives.Add(testDrive);
        await _context.SaveChangesAsync();

        return Ok(new { Id = testDrive.Id });
    }

    /// <summary>
    /// Obtém uma reserva específica pelo ID
    /// </summary>
    /// <param name="id">ID da reserva</param>
    /// <returns>Dados da reserva</returns>
    /// <response code="200">Retorna os dados da reserva</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Reserva não encontrada</response>
    [HttpGet("reservations/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleReservation>> GetReservation(Guid id)
    {
        var reservation = await _context.VehicleReservations
            .Include(r => r.Vehicle)
            .Include(r => r.Shop)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Verifica se o usuário tem permissão para ver a reserva
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (reservation.CustomerId != userId && 
            reservation.Shop?.OwnerId != userId && 
            !await _userManager.IsInRoleAsync(user, Roles.Admin))
            return Forbid();

        return Ok(reservation);
    }

    /// <summary>
    /// Obtém um test drive específico pelo ID
    /// </summary>
    /// <param name="id">ID do test drive</param>
    /// <returns>Dados do test drive</returns>
    /// <response code="200">Retorna os dados do test drive</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Test drive não encontrado</response>
    [HttpGet("test-drives/{id}")]
    [Authorize(Policy = "Module.Reports:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDrive>> GetTestDrive(Guid id)
    {
        var testDrive = await _context.TestDrives
            .Include(t => t.Vehicle)
            .Include(t => t.Shop)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (testDrive == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Verifica se o usuário tem permissão para ver o test drive
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Se não for admin, verifica se o test drive pertence ao usuário ou a uma loja do usuário
        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var userShops = await _context.Shops
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            if (testDrive.CustomerId != userId && !userShops.Contains(testDrive.ShopId))
                return Forbid();
        }

        return Ok(testDrive);
    }
} 