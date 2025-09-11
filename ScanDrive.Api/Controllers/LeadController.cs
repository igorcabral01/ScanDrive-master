using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Lead;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Infrastructure.Extensions;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de leads
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeadController : BaseController
{
    /// <summary>
    /// Construtor do LeadController
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="userManager">Gerenciador de usuários</param>
    /// <param name="roleManager">Gerenciador de papéis</param>
    public LeadController(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
        : base(userManager, roleManager, context)
    {
    }

    /// <summary>
    /// Lista todos os leads com filtros e paginação
    /// </summary>
    /// <param name="filter">Filtros e parâmetros de paginação</param>
    /// <returns>Lista paginada de leads</returns>
    [HttpGet]
    [Authorize(Policy = "Module.Leads:Permission.View")]
    public async Task<ActionResult<PagedList<LeadDto>>> GetLeads([FromQuery] LeadFilter filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);
        
        var query = _context.Leads
            .Include(l => l.Shop)
            .Include(l => l.Vehicle)
            .Include(l => l.CreatedBy)
            .Include(l => l.LastUpdatedBy)
            .Where(l => !l.IsDeleted)
            .AsQueryable();

        // Se não for admin, filtra apenas os leads da loja do usuário
        if (!userRoles.Contains(Roles.Admin))
        {
            var userShops = await _context.Shops
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            query = query.Where(l => userShops.Contains(l.ShopId));
        }

        // Aplicar filtros específicos
        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            query = query.Where(l => l.Name.Contains(filter.CustomerName));

        if (!string.IsNullOrWhiteSpace(filter.CustomerEmail))
            query = query.Where(l => l.Email.Contains(filter.CustomerEmail));

        if (!string.IsNullOrWhiteSpace(filter.CustomerPhone))
            query = query.Where(l => l.Phone.Contains(filter.CustomerPhone));

        if (filter.ContactDateStart.HasValue)
            query = query.Where(l => l.ContactDate >= filter.ContactDateStart.Value);

        if (filter.ContactDateEnd.HasValue)
            query = query.Where(l => l.ContactDate <= filter.ContactDateEnd.Value);

        if (filter.LastContactDateStart.HasValue)
            query = query.Where(l => l.LastContactDate >= filter.LastContactDateStart.Value);

        if (filter.LastContactDateEnd.HasValue)
            query = query.Where(l => l.LastContactDate <= filter.LastContactDateEnd.Value);

        if (filter.HasBeenContacted.HasValue)
            query = query.Where(l => l.HasBeenContacted == filter.HasBeenContacted.Value);

        if (filter.Status.HasValue)
            query = query.Where(l => l.Status == filter.Status.Value);

        if (filter.VehicleId.HasValue)
            query = query.Where(l => l.VehicleId == filter.VehicleId.Value);

        if (!string.IsNullOrWhiteSpace(filter.VehicleBrand))
            query = query.Where(l => l.Vehicle != null && l.Vehicle.Brand.Contains(filter.VehicleBrand));

        if (!string.IsNullOrWhiteSpace(filter.VehicleModel))
            query = query.Where(l => l.Vehicle != null && l.Vehicle.Model.Contains(filter.VehicleModel));

        // Aplicar filtros base e paginação
        query = query.ApplyFilter(filter);

        var dtoQuery = query.Select(l => new LeadDto
        {
            Id = l.Id,
            Name = l.Name,
            Phone = l.Phone,
            Email = l.Email,
            ContactDate = l.ContactDate,
            LastContactDate = l.LastContactDate,
            Notes = l.Notes,
            Status = l.Status,
            HasBeenContacted = l.HasBeenContacted,
            ShopId = l.ShopId,
            ShopName = l.Shop != null ? l.Shop.Name : string.Empty,
            VehicleId = l.VehicleId,
            VehicleName = l.Vehicle != null ? $"{l.Vehicle.Brand} {l.Vehicle.Model} {l.Vehicle.Year}" : null,
            CreatedById = l.CreatedById,
            CreatedByName = l.CreatedBy != null ? l.CreatedBy.UserName : l.CreatedById,
            LastUpdatedById = l.LastUpdatedById,
            LastUpdatedByName = l.LastUpdatedBy != null ? l.LastUpdatedBy.UserName : l.LastUpdatedById,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            IsActive = l.IsActive
        });

        var result = await dtoQuery.ToPagedListAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um lead específico
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <returns>Dados do lead</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "Module.Leads:Permission.View")]
    public async Task<ActionResult<LeadDto>> GetLead(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);

        var query = _context.Leads
            .Include(l => l.Shop)
            .Include(l => l.Vehicle)
            .Include(l => l.CreatedBy)
            .Include(l => l.LastUpdatedBy)
            .Where(l => l.Id == id);

        // Se não for admin, verifica se o lead pertence a uma loja do usuário
        if (!userRoles.Contains(Roles.Admin))
        {
            var userShops = await _context.Shops
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            query = query.Where(l => userShops.Contains(l.ShopId));
        }

        var lead = await query.Select(l => new LeadDto
        {
            Id = l.Id,
            Name = l.Name,
            Phone = l.Phone,
            Email = l.Email,
            ContactDate = l.ContactDate,
            LastContactDate = l.LastContactDate,
            Notes = l.Notes,
            Status = l.Status,
            HasBeenContacted = l.HasBeenContacted,
            ShopId = l.ShopId,
            ShopName = l.Shop != null ? l.Shop.Name : string.Empty,
            VehicleId = l.VehicleId,
            VehicleName = l.Vehicle != null ? $"{l.Vehicle.Brand} {l.Vehicle.Model} {l.Vehicle.Year}" : null,
            CreatedById = l.CreatedById,
            CreatedByName = l.CreatedBy != null ? l.CreatedBy.UserName : l.CreatedById,
            LastUpdatedById = l.LastUpdatedById,
            LastUpdatedByName = l.LastUpdatedBy != null ? l.LastUpdatedBy.UserName : l.LastUpdatedById,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            IsActive = l.IsActive
        }).FirstOrDefaultAsync();

        if (lead == null)
            return NotFound();

        return Ok(lead);
    }

    /// <summary>
    /// Cria um novo lead
    /// </summary>
    /// <param name="createLeadDto">Dados do lead</param>
    /// <returns>Lead criado</returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<LeadDto>> CreateLead(CreateLeadDto createLeadDto)
    {
        var userId = GetCurrentUserId(); // Pode ser null para usuários anônimos

        // Validar se a loja existe
        var shop = await _context.Shops
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == createLeadDto.ShopId);
            
        if (shop == null)
            return BadRequest(new { Message = "Loja não encontrada" });

        // Validar se o veículo existe (se foi informado)
        if (createLeadDto.VehicleId.HasValue)
        {
            var vehicle = await _context.Vehicles.FindAsync(createLeadDto.VehicleId.Value);
            if (vehicle == null)
                createLeadDto.VehicleId = null;
        }

        // Verifica permissões apenas se o usuário estiver autenticado
        if (userId != null && !await IsAdminOrShopMember(createLeadDto.ShopId))
            return Forbid();

        var lead = new Lead
        {
            Name = createLeadDto.Name,
            Phone = createLeadDto.Phone,
            Email = createLeadDto.Email,
            Notes = createLeadDto.Notes,
            ContactDate = DateTime.UtcNow,
            Status = LeadStatus.New,
            ShopId = createLeadDto.ShopId,
            VehicleId = createLeadDto.VehicleId,
            CreatedById = userId ?? shop.OwnerId, // Usa o ID do dono da loja quando não há usuário autenticado
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        // Carregar os dados relacionados para retornar no DTO
        await _context.Entry(lead)
            .Reference(l => l.Shop)
            .LoadAsync();

        if (lead.VehicleId.HasValue)
        {
            await _context.Entry(lead)
                .Reference(l => l.Vehicle)
                .LoadAsync();
        }

        // Carregar CreatedBy apenas se não for anônimo
        if (userId != null)
        {
            await _context.Entry(lead)
                .Reference(l => l.CreatedBy)
                .LoadAsync();
        }

        var leadDto = new LeadDto
        {
            Id = lead.Id,
            Name = lead.Name,
            Phone = lead.Phone,
            Email = lead.Email,
            ContactDate = lead.ContactDate,
            LastContactDate = lead.LastContactDate,
            Notes = lead.Notes,
            Status = lead.Status,
            HasBeenContacted = lead.HasBeenContacted,
            ShopId = lead.ShopId,
            ShopName = lead.Shop != null ? lead.Shop.Name : string.Empty,
            VehicleId = lead.VehicleId,
            VehicleName = lead.Vehicle != null ? $"{lead.Vehicle.Brand} {lead.Vehicle.Model} {lead.Vehicle.Year}" : null,
            CreatedById = lead.CreatedById,
            CreatedByName = userId != null ? (lead.CreatedBy?.UserName ?? lead.CreatedById) : "ScanDrive",
            LastUpdatedById = lead.LastUpdatedById,
            LastUpdatedByName = lead.LastUpdatedBy != null ? lead.LastUpdatedBy.UserName : lead.LastUpdatedById,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt,
            IsActive = lead.IsActive
        };

        return CreatedAtAction(nameof(GetLead), new { id = lead.Id }, leadDto);
    }

    /// <summary>
    /// Atualiza um lead existente
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="updateLeadDto">Dados atualizados do lead</param>
    /// <returns>Nenhum conteúdo</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Module.Leads:Permission.Edit")]
    public async Task<IActionResult> UpdateLead(Guid id, UpdateLeadDto updateLeadDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
            return NotFound();

        // Se não for admin, verifica se o usuário é dono da loja
        if (!await IsAdminOrShopMember(lead.ShopId))
            return Forbid();

        lead.Name = updateLeadDto.Name;
        lead.Phone = updateLeadDto.Phone;
        lead.Email = updateLeadDto.Email;
        lead.Notes = updateLeadDto.Notes;
        lead.Status = updateLeadDto.Status;
        lead.HasBeenContacted = updateLeadDto.HasBeenContacted;
        lead.VehicleId = updateLeadDto.VehicleId;
        lead.IsActive = updateLeadDto.IsActive;
        lead.LastUpdatedById = userId;
        lead.LastContactDate = DateTime.UtcNow;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove um lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <returns>Nenhum conteúdo</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Module.Leads:Permission.Delete")]
    public async Task<IActionResult> DeleteLead(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
            return NotFound();

        // Se não for admin, verifica se o usuário é dono da loja
        if (!await IsAdminOrShopMember(lead.ShopId))
            return Forbid();

        lead.IsDeleted = true;
        lead.UpdatedAt = DateTime.UtcNow;
        lead.LastUpdatedById = userId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Lista todos os status possíveis para um lead
    /// </summary>
    /// <returns>Lista de status</returns>
    [HttpGet("status")]
    [Authorize(Policy = "Module.Leads:Permission.View")]
    public ActionResult<IEnumerable<object>> ListItems()
    {
        var items = Enum.GetValues(typeof(LeadStatus))
            .Cast<LeadStatus>()
            .Select(s => new
            {
                id = (int)s,
                name = s.ToString()
            });

        return Ok(items);
    }
} 