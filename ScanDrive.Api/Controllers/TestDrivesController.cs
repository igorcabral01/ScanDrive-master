using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.TestDrive;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Infrastructure.Extensions;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar os test drives
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestDrivesController : BaseController
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Construtor do controller de test drives
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="configuration">Configurações da aplicação</param>
    public TestDrivesController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        IConfiguration configuration)
        : base(userManager, roleManager, context)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Lista todos os test drives com filtros e paginação
    /// </summary>
    /// <param name="filter">Filtros e parâmetros de paginação</param>
    /// <returns>Lista paginada de test drives</returns>
    /// <response code="200">Retorna a lista de test drives</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [Authorize(Policy = "Module.TestDrives:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedList<TestDriveListItemDTO>>> GetTestDrives([FromQuery] TestDriveFilter filter)
    {
        var user = await GetCurrentUser();
        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isShopOwner = await _userManager.IsInRoleAsync(user, Roles.ShopOwner);

        var query = _context.TestDrives
            .Include(t => t.Vehicle)
            .Include(t => t.Shop)
            .Where(t => !t.IsDeleted)
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

                query = query.Where(t => userShops.Contains(t.ShopId));
            }
            else
            {
                query = query.Where(t => t.CustomerId == user.Id);
            }
        }

        // Aplicar filtros específicos
        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            query = query.Where(t => t.CustomerName.Contains(filter.CustomerName));

        if (!string.IsNullOrWhiteSpace(filter.CustomerEmail))
            query = query.Where(t => t.CustomerEmail.Contains(filter.CustomerEmail));

        if (!string.IsNullOrWhiteSpace(filter.CustomerPhone))
            query = query.Where(t => t.CustomerPhone.Contains(filter.CustomerPhone));

        if (filter.PreferredDateStart.HasValue)
            query = query.Where(t => t.PreferredDate >= filter.PreferredDateStart.Value);

        if (filter.PreferredDateEnd.HasValue)
            query = query.Where(t => t.PreferredDate <= filter.PreferredDateEnd.Value);

        if (filter.IsCancelled.HasValue)
            query = query.Where(t => t.IsCancelled == filter.IsCancelled.Value);

        if (filter.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);

        if (filter.VehicleId.HasValue)
            query = query.Where(t => t.VehicleId == filter.VehicleId.Value);

        if (!string.IsNullOrWhiteSpace(filter.VehicleBrand))
            query = query.Where(t => t.Vehicle != null && t.Vehicle.Brand.Contains(filter.VehicleBrand));

        if (!string.IsNullOrWhiteSpace(filter.VehicleModel))
            query = query.Where(t => t.Vehicle != null && t.Vehicle.Model.Contains(filter.VehicleModel));

        // Aplicar filtros base e paginação
        query = query.ApplyFilter(filter);

        var dtoQuery = query.Select(t => new TestDriveListItemDTO
        {
            Id = t.Id,
            CustomerName = t.CustomerName,
            CustomerEmail = t.CustomerEmail,
            CustomerPhone = t.CustomerPhone,
            PreferredDate = t.PreferredDate,
            Notes = t.Notes,
            IsActive = t.IsActive,
            IsCancelled = t.IsCancelled,
            IsCompleted = t.IsCompleted,
            CompletionDate = t.CompletionDate,
            CompletionNotes = t.CompletionNotes,
            CancellationDate = t.CancellationDate,
            CancellationReason = t.CancellationReason,
            VehicleId = t.Vehicle.Id,
            VehicleBrand = t.Vehicle.Brand,
            VehicleModel = t.Vehicle.Model,
            VehicleYear = t.Vehicle.Year,
            VehicleMainPhotoUrl = t.Vehicle.MainPhotoUrl,
            VehicleTransmission = t.Vehicle.Transmission,
            VehicleFuelType = t.Vehicle.FuelType,
            VehicleHasAuction = t.Vehicle.HasAuction,
            VehicleHasAccident = t.Vehicle.HasAccident,
            VehicleIsFirstOwner = t.Vehicle.IsFirstOwner,
            VehicleOwnersCount = t.Vehicle.OwnersCount,
            ShopId = t.ShopId,
            ShopName = t.Shop != null ? t.Shop.Name : "Loja não encontrada"
        });

        var result = await dtoQuery.ToPagedListAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo test drive
    /// </summary>
    /// <param name="dto">Dados do test drive</param>
    /// <returns>Dados do test drive criado</returns>
    /// <response code="201">Test drive criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Veículo não encontrado</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDriveListItemDTO>> CreateTestDrive(CreateTestDriveDTO dto)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Shop)
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && !v.IsDeleted);

        if (vehicle == null)
            return NotFound("Vehicle not found");

        if (!vehicle.IsActive)
            return BadRequest("Vehicle is not available for test drive");

        if (vehicle.IsSold)
            return BadRequest("Vehicle has already been sold");

        // Tenta obter o usuário atual se estiver autenticado
        var user = await GetCurrentUserOrNull();
        string? customerId = user?.Id;

        // Se não houver usuário autenticado, cria um lead
        if (user == null)
        {
            var lead = new Lead
            {
                Name = dto.CustomerName,
                Email = dto.CustomerEmail,
                Phone = dto.CustomerPhone,
                ContactDate = DateTime.UtcNow,
                Status = LeadStatus.New,
                Notes = $"Lead gerado automaticamente a partir de agendamento de test drive para o veículo {vehicle.Brand} {vehicle.Model} {vehicle.Year}",
                ShopId = vehicle.ShopId,
                VehicleId = vehicle.Id,
                CreatedById = vehicle.Shop?.OwnerId ?? throw new InvalidOperationException("Shop owner not found"),
                IsActive = true,
                HasBeenContacted = false
            };

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
        }

        var testDrive = new TestDrive
        {
            VehicleId = dto.VehicleId,
            ShopId = vehicle.ShopId,
            CustomerId = customerId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            PreferredDate = dto.PreferredDate,
            Notes = dto.Notes,
            IsActive = true
        };

        _context.TestDrives.Add(testDrive);
        await _context.SaveChangesAsync();

        var result = new TestDriveListItemDTO
        {
            Id = testDrive.Id,
            CustomerName = testDrive.CustomerName,
            CustomerEmail = testDrive.CustomerEmail,
            CustomerPhone = testDrive.CustomerPhone,
            PreferredDate = testDrive.PreferredDate,
            Notes = testDrive.Notes,
            IsActive = testDrive.IsActive,
            IsCancelled = testDrive.IsCancelled,
            IsCompleted = testDrive.IsCompleted,
            CompletionDate = testDrive.CompletionDate,
            CompletionNotes = testDrive.CompletionNotes,
            CancellationDate = testDrive.CancellationDate,
            CancellationReason = testDrive.CancellationReason,
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
            result.ShopName = vehicle.Shop.Name;
        }

        return CreatedAtAction(nameof(GetTestDrives), new { id = testDrive.Id }, result);
    }

    /// <summary>
    /// Cancela um test drive
    /// </summary>
    /// <param name="id">ID do test drive</param>
    /// <param name="reason">Razão da cancelação</param>
    /// <returns>Dados do test drive cancelado</returns>
    /// <response code="200">Test drive cancelado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Test drive não encontrado</response>
    [HttpPost("{id}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelTestDrive(Guid id, [FromBody] string reason)
    {
        var user = await GetCurrentUser();
        var testDrive = await _context.TestDrives
            .Include(t => t.Shop)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (testDrive == null)
            return NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isShopOwner = await _userManager.IsInRoleAsync(user, Roles.ShopOwner);
        var isCustomer = testDrive.CustomerId == user.Id;
        var ownsShop = isShopOwner && testDrive.Shop?.OwnerId == user.Id;

        if (!isAdmin && !ownsShop && !isCustomer)
            return Forbid();

        if (testDrive.IsCancelled)
            return BadRequest("Test drive is already cancelled");

        if (testDrive.IsCompleted)
            return BadRequest("Cannot cancel a completed test drive");

        testDrive.IsCancelled = true;
        testDrive.IsActive = false;
        testDrive.CancellationDate = DateTime.UtcNow;
        testDrive.CancellationReason = reason;

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Completa um test drive
    /// </summary>
    /// <param name="id">ID do test drive</param>
    /// <param name="notes">Notas da conclusão</param>
    /// <returns>Dados do test drive completado</returns>
    /// <response code="200">Test drive completado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="404">Test drive não encontrado</response>
    [HttpPost("{id}/complete")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CompleteTestDrive(Guid id, [FromBody] string notes)
    {
        var user = await GetCurrentUser();
        var testDrive = await _context.TestDrives
            .Include(t => t.Shop)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (testDrive == null)
            return NotFound();

        var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Admin);
        var isShopOwner = await _userManager.IsInRoleAsync(user, Roles.ShopOwner);
        var ownsShop = isShopOwner && testDrive.Shop?.OwnerId == user.Id;

        if (!isAdmin && !ownsShop)
            return Forbid();

        if (testDrive.IsCancelled)
            return BadRequest("Cannot complete a cancelled test drive");

        if (testDrive.IsCompleted)
            return BadRequest("Test drive is already completed");

        testDrive.IsCompleted = true;
        testDrive.IsActive = false;
        testDrive.CompletionDate = DateTime.UtcNow;
        testDrive.CompletionNotes = notes;

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Obtém o usuário atual se estiver autenticado, ou retorna null se não estiver
    /// </summary>
    private async Task<IdentityUser?> GetCurrentUserOrNull()
    {
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return await GetCurrentUser();
            }
        }
        catch
        {
            // Ignora erros ao tentar obter o usuário
        }
        return null;
    }
} 