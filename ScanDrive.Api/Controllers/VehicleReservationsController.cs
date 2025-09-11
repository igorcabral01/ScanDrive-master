using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.VehicleReservation;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Infrastructure.Extensions;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar as reservas de veículos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VehicleReservationsController : BaseController
{
    /// <summary>
    /// Construtor do controller de reservas de veículos
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    public VehicleReservationsController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
        : base(userManager, roleManager, context)
    {
    }

    /// <summary>
    /// Lista todas as reservas de veículos com filtros e paginação
    /// </summary>
    /// <param name="filter">Filtros e parâmetros de paginação</param>
    /// <returns>Lista paginada de reservas</returns>
    /// <response code="200">Retorna a lista de reservas</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedList<VehicleReservationListItemDTO>>> GetReservations([FromQuery] VehicleReservationFilter filter)
    {
        var user = await GetCurrentUser();
        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isShopOwner = await _userManager.IsInRoleAsync(user, Roles.ShopOwner);

        var query = _context.VehicleReservations
            .Include(r => r.Vehicle)
            .Include(r => r.Shop)
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        // Aplicar filtros de permissão
        if (!isAdmin)
        {
            if (isShopOwner)
            {
                var userShops = await _context.Shops
                    .Where(s => s.OwnerId == user.Id)
                    .Select(s => s.Id)
                    .ToListAsync();

                query = query.Where(r => userShops.Contains(r.ShopId));
            }
            else
            {
                query = query.Where(r => r.CustomerId == user.Id);
            }
        }

        // Aplicar filtros específicos
        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            query = query.Where(r => r.CustomerName.Contains(filter.CustomerName));

        if (!string.IsNullOrWhiteSpace(filter.CustomerEmail))
            query = query.Where(r => r.CustomerEmail.Contains(filter.CustomerEmail));

        if (!string.IsNullOrWhiteSpace(filter.CustomerPhone))
            query = query.Where(r => r.CustomerPhone.Contains(filter.CustomerPhone));

        if (filter.ReservationDateStart.HasValue)
            query = query.Where(r => r.ReservationDate >= filter.ReservationDateStart.Value);

        if (filter.ReservationDateEnd.HasValue)
            query = query.Where(r => r.ReservationDate <= filter.ReservationDateEnd.Value);

        if (filter.IsCancelled.HasValue)
            query = query.Where(r => r.IsCancelled == filter.IsCancelled.Value);

        if (filter.VehicleId.HasValue)
            query = query.Where(r => r.VehicleId == filter.VehicleId.Value);

        if (!string.IsNullOrWhiteSpace(filter.VehicleBrand))
            query = query.Where(r => r.Vehicle != null && r.Vehicle.Brand.Contains(filter.VehicleBrand));

        if (!string.IsNullOrWhiteSpace(filter.VehicleModel))
            query = query.Where(r => r.Vehicle != null && r.Vehicle.Model.Contains(filter.VehicleModel));

        // Aplicar filtros base e paginação
        query = query.ApplyFilter(filter);

        var dtoQuery = query.Select(r => new VehicleReservationListItemDTO
        {
            Id = r.Id,
            CustomerName = r.CustomerName,
            CustomerEmail = r.CustomerEmail,
            CustomerPhone = r.CustomerPhone,
            ReservationDate = r.ReservationDate,
            Notes = r.Notes,
            IsActive = r.IsActive,
            IsCancelled = r.IsCancelled,
            CancellationDate = r.CancellationDate,
            CancellationReason = r.CancellationReason,
            VehicleId = r.Vehicle.Id,
            VehicleBrand = r.Vehicle.Brand,
            VehicleModel = r.Vehicle.Model,
            VehicleYear = r.Vehicle.Year,
            VehicleMainPhotoUrl = r.Vehicle.MainPhotoUrl,
            VehicleTransmission = r.Vehicle.Transmission,
            VehicleFuelType = r.Vehicle.FuelType,
            VehicleHasAuction = r.Vehicle.HasAuction,
            VehicleHasAccident = r.Vehicle.HasAccident,
            VehicleIsFirstOwner = r.Vehicle.IsFirstOwner,
            VehicleOwnersCount = r.Vehicle.OwnersCount,
            ShopId = r.ShopId,
            ShopName = r.Shop != null ? r.Shop.Name : "Loja não encontrada"
        });

        var result = await dtoQuery.ToPagedListAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova reserva de veículo
    /// </summary>
    /// <param name="dto">Dados da reserva</param>
    /// <returns>Dados da reserva criada</returns>
    /// <response code="201">Reserva criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Veículo não encontrado</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleReservationListItemDTO>> CreateReservation(CreateVehicleReservationDTO dto)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Shop)
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted);

        if (vehicle == null)
            return NotFound("Vehicle not found");

        if (!vehicle.IsActive)
            return BadRequest("Vehicle is not available for reservation");

        if (vehicle.IsSold)
            return BadRequest("Vehicle has already been sold");

        var user = await GetCurrentUser();

        var reservation = new VehicleReservation
        {
            VehicleId = dto.VehicleId,
            ShopId = vehicle.ShopId,
            CustomerId = user.Id,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            ReservationDate = dto.ReservationDate,
            Notes = dto.Notes,
            IsActive = true
        };

        _context.VehicleReservations.Add(reservation);
        await _context.SaveChangesAsync();

        var result = new VehicleReservationListItemDTO
        {
            Id = reservation.Id,
            CustomerName = reservation.CustomerName,
            CustomerEmail = reservation.CustomerEmail,
            CustomerPhone = reservation.CustomerPhone,
            ReservationDate = reservation.ReservationDate,
            Notes = reservation.Notes,
            IsActive = reservation.IsActive,
            IsCancelled = reservation.IsCancelled,
            CancellationDate = reservation.CancellationDate,
            CancellationReason = reservation.CancellationReason,
            VehicleId = vehicle.Id,
            VehicleBrand = vehicle.Brand,
            VehicleModel = vehicle.Model,
            VehicleYear = vehicle.Year,
            VehicleMainPhotoUrl = vehicle.MainPhotoUrl,
            VehicleTransmission = vehicle.Transmission,
            VehicleFuelType = vehicle.FuelType,
            VehicleHasAuction = vehicle.HasAuction,
            VehicleHasAccident = vehicle.HasAccident,
            VehicleIsFirstOwner = vehicle.IsFirstOwner,
            VehicleOwnersCount = vehicle.OwnersCount,
            ShopId = vehicle.ShopId
        };

        if (vehicle.Shop != null)
        {
            result.ShopId = vehicle.Shop.Id;
            result.ShopName = vehicle.Shop.Name;
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancela uma reserva de veículo
    /// </summary>
    /// <param name="id">ID da reserva</param>
    /// <param name="reason">Motivo do cancelamento</param>
    /// <returns>Dados da reserva cancelada</returns>
    /// <response code="200">Reserva cancelada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Reserva não encontrada</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelReservation(Guid id, [FromBody] string reason)
    {
        var user = await GetCurrentUser();
        var reservation = await _context.VehicleReservations
            .Include(r => r.Shop)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (reservation == null)
            return NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isShopOwner = await _userManager.IsInRoleAsync(user, Roles.ShopOwner);
        var isCustomer = reservation.CustomerId == user.Id;
        var ownsShop = isShopOwner && reservation.Shop?.OwnerId == user.Id;

        if (!isAdmin && !ownsShop && !isCustomer)
            return Forbid();

        if (reservation.IsCancelled)
            return BadRequest("Reservation is already cancelled");

        reservation.IsCancelled = true;
        reservation.IsActive = false;
        reservation.CancellationDate = DateTime.UtcNow;
        reservation.CancellationReason = reason;

        await _context.SaveChangesAsync();

        return Ok();
    }
} 