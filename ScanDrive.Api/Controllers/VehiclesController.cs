using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.Vehicle;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Infrastructure.Extensions;
using ScanDrive.Domain.Extensions;
using System.Text.Json;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar os veículos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : BaseController
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadsFolder;

    /// <summary>
    /// Construtor do controller de veículos
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="environment">Ambiente de hospedagem da aplicação</param>
    public VehiclesController(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment)
        : base(userManager, roleManager, context)
    {
        _environment = environment;
        
        // Configura o diretório de uploads, criando se necessário
        var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _uploadsFolder = Path.Combine(webRootPath, "uploads", "vehicles");
        
        // Garante que a pasta de uploads existe
        if (!Directory.Exists(_uploadsFolder))
            Directory.CreateDirectory(_uploadsFolder);
    }

    /// <summary>
    /// Lista todos os veículos ativos com filtros e paginação
    /// </summary>
    /// <param name="filter">Filtros e parâmetros de paginação</param>
    /// <returns>Lista paginada de veículos</returns>
    /// <response code="200">Retorna a lista de veículos</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedList<VehicleDetailDto>>> GetAll([FromQuery] VehicleFilter filter)
    {
        var query = _context.Vehicles
            .Include(v => v.Shop)
            .Include(v => v.Photos)
            .Include(v => v.Optionals)
            .Where(v => !v.IsDeleted && v.Shop != null && v.Shop.IsActive)
            .AsQueryable();

        // Aplicar filtros específicos
        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(v => v.Brand.Contains(filter.Brand));

        if (!string.IsNullOrWhiteSpace(filter.Model))
            query = query.Where(v => v.Model.Contains(filter.Model));

        if (filter.Year.HasValue)
            query = query.Where(v => v.Year == filter.Year.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(v => v.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(v => v.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Color))
            query = query.Where(v => v.Color.Contains(filter.Color));

        if (!string.IsNullOrWhiteSpace(filter.Transmission))
            query = query.Where(v => v.Transmission == filter.Transmission);

        if (!string.IsNullOrWhiteSpace(filter.FuelType))
            query = query.Where(v => v.FuelType == filter.FuelType);

        if (filter.HasAuction.HasValue)
            query = query.Where(v => v.HasAuction == filter.HasAuction.Value);

        if (filter.HasAccident.HasValue)
            query = query.Where(v => v.HasAccident == filter.HasAccident.Value);

        if (filter.IsFirstOwner.HasValue)
            query = query.Where(v => v.IsFirstOwner == filter.IsFirstOwner.Value);

        if (filter.IsSold.HasValue)
            query = query.Where(v => v.IsSold == filter.IsSold.Value);

        // Novos filtros
        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(v => v.Category != null && v.Category.Contains(filter.Category));

        if (!string.IsNullOrWhiteSpace(filter.CategoryType))
            query = query.Where(v => v.CategoryType != null && v.CategoryType.Contains(filter.CategoryType));

        if (!string.IsNullOrWhiteSpace(filter.Engine))
            query = query.Where(v => v.Engine != null && v.Engine.Contains(filter.Engine));

        if (!string.IsNullOrWhiteSpace(filter.Version))
            query = query.Where(v => v.Version != null && v.Version.Contains(filter.Version));

        if (filter.MinOfferPrice.HasValue)
            query = query.Where(v => v.OfferPrice >= filter.MinOfferPrice.Value);

        if (filter.MaxOfferPrice.HasValue)
            query = query.Where(v => v.OfferPrice <= filter.MaxOfferPrice.Value);

        if (filter.MinFipePrice.HasValue)
            query = query.Where(v => v.FipePrice >= filter.MinFipePrice.Value);

        if (filter.MaxFipePrice.HasValue)
            query = query.Where(v => v.FipePrice <= filter.MaxFipePrice.Value);

        if (filter.Doors.HasValue)
            query = query.Where(v => v.Doors == filter.Doors.Value);

        if (!string.IsNullOrWhiteSpace(filter.Condition))
            query = query.Where(v => v.Condition == filter.Condition);

        if (filter.IsHighlighted.HasValue)
            query = query.Where(v => v.IsHighlighted == filter.IsHighlighted.Value);

        if (filter.IsOnOffer.HasValue)
            query = query.Where(v => v.IsOnOffer == filter.IsOnOffer.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(v => v.City != null && v.City.Contains(filter.City));

        if (!string.IsNullOrWhiteSpace(filter.State))
            query = query.Where(v => v.State != null && v.State.Contains(filter.State));

        if (!string.IsNullOrWhiteSpace(filter.LicensePlate))
            query = query.Where(v => v.LicensePlate != null && v.LicensePlate.Contains(filter.LicensePlate));

        // Aplicar filtros base e paginação
        query = query.ApplyFilter(filter);

        var dtoQuery = query.Select(v => new VehicleDetailDto
        {
            Id = v.Id.ToString(),
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Mileage = v.Mileage,
            Color = v.Color,
            Price = v.Price,
            Description = v.Description,
            PhotoUrls = v.Photos.OrderBy(p => p.Order).Select(p => p.Url).ToList(),
            MainPhotoUrl = v.MainPhotoUrl,
            ShopId = v.ShopId.ToString(),
            ShopName = v.Shop != null ? v.Shop.Name : "Loja não encontrada",
            Transmission = v.Transmission,
            FuelType = v.FuelType,
            HasAuction = v.HasAuction,
            HasAccident = v.HasAccident,
            IsFirstOwner = v.IsFirstOwner,
            AuctionHistory = v.AuctionHistory,
            AccidentHistory = v.AccidentHistory,
            OwnersCount = v.OwnersCount,
            Features = v.Features,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt ?? v.CreatedAt,
            ExternalVehicleCode = v.ExternalVehicleCode,
            ImportCode = v.ImportCode,
            Category = v.Category,
            CategoryType = v.CategoryType,
            Engine = v.Engine,
            Valves = v.Valves,
            Version = v.Version,
            FullName = v.FullName,
            AlternativeName = v.AlternativeName,
            OfferPrice = v.OfferPrice,
            FipePrice = v.FipePrice,
            SiteObservations = v.SiteObservations,
            LicensePlate = v.LicensePlate,
            Renavam = v.Renavam,
            Doors = v.Doors,
            Condition = v.Condition,
            IsHighlighted = v.IsHighlighted,
            IsOnOffer = v.IsOnOffer,
            CompanyName = v.CompanyName,
            City = v.City,
            State = v.State,
            YouTubeUrl = v.YouTubeUrl,
            Optionals = v.Optionals.Select(o => new VehicleOptionalDto
            {
                Code = o.Code,
                Description = o.Description
            }).ToList()
        });

        var result = await dtoQuery.ToPagedListAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um veículo específico pelo ID
    /// </summary>
    /// <param name="id">ID do veículo</param>
    /// <returns>Dados detalhados do veículo</returns>
    /// <response code="200">Retorna os dados do veículo</response>
    /// <response code="404">Veículo não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDetailDto>> GetById(Guid id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Shop)
            .Include(v => v.Photos)
            .Include(v => v.Optionals)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.Shop != null && v.Shop.IsActive);

        if (vehicle == null || vehicle.Shop == null)
            return NotFound();

        var detail = new VehicleDetailDto
        {
            Id = vehicle.Id.ToString(),
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            Color = vehicle.Color,
            Price = vehicle.Price,
            Description = vehicle.Description,
            PhotoUrls = vehicle.Photos.OrderBy(p => p.Order).Select(p => p.Url).ToList(),
            MainPhotoUrl = vehicle.MainPhotoUrl,
            ShopId = vehicle.ShopId.ToString(),
            ShopName = vehicle.Shop.Name,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            HasAuction = vehicle.HasAuction,
            HasAccident = vehicle.HasAccident,
            IsFirstOwner = vehicle.IsFirstOwner,
            AuctionHistory = vehicle.AuctionHistory,
            AccidentHistory = vehicle.AccidentHistory,
            OwnersCount = vehicle.OwnersCount,
            Features = vehicle.Features,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt ?? vehicle.CreatedAt,
            ExternalVehicleCode = vehicle.ExternalVehicleCode,
            ImportCode = vehicle.ImportCode,
            Category = vehicle.Category,
            CategoryType = vehicle.CategoryType,
            Engine = vehicle.Engine,
            Valves = vehicle.Valves,
            Version = vehicle.Version,
            FullName = vehicle.FullName,
            AlternativeName = vehicle.AlternativeName,
            OfferPrice = vehicle.OfferPrice,
            FipePrice = vehicle.FipePrice,
            SiteObservations = vehicle.SiteObservations,
            LicensePlate = vehicle.LicensePlate,
            Renavam = vehicle.Renavam,
            Doors = vehicle.Doors,
            Condition = vehicle.Condition,
            IsHighlighted = vehicle.IsHighlighted,
            IsOnOffer = vehicle.IsOnOffer,
            CompanyName = vehicle.CompanyName,
            City = vehicle.City,
            State = vehicle.State,
            YouTubeUrl = vehicle.YouTubeUrl,
            Optionals = vehicle.Optionals.Select(o => new VehicleOptionalDto
            {
                Code = o.Code,
                Description = o.Description
            }).ToList()
        };

        return Ok(detail);
    }

    /// <summary>
    /// Faz upload de fotos para um veículo
    /// </summary>
    /// <param name="id">ID do veículo</param>
    /// <param name="files">Arquivos de foto</param>
    /// <returns>Lista de URLs das fotos enviadas</returns>
    /// <response code="200">Fotos enviadas com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Veículo não encontrado</response>
    [Authorize]
    [HttpPost("{id}/photos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhotos(Guid id, IFormFileCollection files)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vehicle == null)
            return NotFound();

        // Verifica se o usuário tem permissão (é admin ou dono/vendedor da loja)
        if (!await IsAdminOrShopMember(vehicle.ShopId))
            return Forbid();

        var uploadedFiles = new List<VehiclePhoto>();
        var isFirstPhotoEver = !vehicle.Photos.Any() && string.IsNullOrEmpty(vehicle.MainPhotoUrl);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var photo = new VehiclePhoto
                {
                    Id = Guid.NewGuid(),
                    FileName = fileName,
                    Url = $"/uploads/vehicles/{fileName}",
                    Order = vehicle.Photos.Count + uploadedFiles.Count,
                    IsMain = isFirstPhotoEver && uploadedFiles.Count == 0,
                    VehicleId = vehicle.Id
                };

                if (photo.IsMain)
                    vehicle.MainPhotoUrl = photo.Url;

                uploadedFiles.Add(photo);
            }
        }

        await _context.VehiclePhotos.AddRangeAsync(uploadedFiles);
        await _context.SaveChangesAsync();

        return Ok(uploadedFiles.Select(p => new { p.Url, p.IsMain, p.Order }).OrderBy(p => p.Order).ToList());
    }

    /// <summary>
    /// Define uma foto como a principal do veículo
    /// </summary>
    /// <param name="id">ID do veículo</param>
    /// <param name="photoId">ID da foto</param>
    /// <returns>Nenhum conteúdo</returns>
    /// <response code="200">Foto definida como principal com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Veículo ou foto não encontrado</response>
    [Authorize]
    [HttpPut("{id}/photos/{photoId}/main")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMainPhoto(Guid id, Guid photoId)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

        if (vehicle == null)
            return NotFound();

        // Verifica se o usuário tem permissão
        if (!await IsAdminOrShopMember(vehicle.ShopId))
            return Forbid();

        var photo = vehicle.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return NotFound();

        // Remove a marcação de principal de todas as fotos
        foreach (var p in vehicle.Photos)
            p.IsMain = false;

        // Define a nova foto principal
        photo.IsMain = true;
        vehicle.MainPhotoUrl = photo.Url;

        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Lista todos os veículos em formato simplificado
    /// </summary>
    /// <returns>Lista de veículos em formato de lista</returns>
    /// <response code="200">Retorna a lista de veículos</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("list-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ListItemDto>>> GetListItems()
    {
        var vehicles = await _context.Vehicles
            .Where(v => !v.IsDeleted && !v.IsSold && v.IsActive)
            .Select(v => new ListItemDto
            {
                Id = v.Id.ToString(),
                Description = $"{v.Brand} {v.Model} ({v.Year})"
            })
            .ToListAsync();

        return Ok(vehicles);
    }

    /// <summary>
    /// Verifica se o usuário atual é admin ou membro da loja
    /// </summary>
    private new async Task<bool> IsAdminOrShopMember(Guid shopId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        // Se é admin, tem permissão
        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return true;

        // Verifica se é dono ou vendedor da loja
        var shop = await _context.Shops
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            return false;

        if (shop.OwnerId == userId)
            return true;

        // Verifica se é vendedor da loja
        return await _userManager.IsInRoleAsync(user, Roles.ShopSeller) &&
               await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.RoleId == Roles.ShopSeller);
    }

    /// <summary>
    /// Verifica se o usuário tem permissão para acessar o veículo
    /// </summary>
    private async Task<bool> HasPermission(Guid shopId)
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
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            return false;

        if (shop.OwnerId == userId)
            return true;

        // Verifica se é vendedor da loja e tem permissões de vendedor
        return permissions.Any(p => p.StartsWith($"{Claims.Modules.Vehicles}:"));
    }

    /// <summary>
    /// Importa veículos a partir de um ou mais arquivos JSON no formato base.json
    /// </summary>
    /// <param name="shopId">ID da loja onde os veículos serão importados</param>
    /// <param name="urlPrefix">Prefixo da URL para as fotos dos veículos</param>
    /// <param name="files">Arquivos JSON com os dados dos veículos</param>
    /// <returns>Resultado da importação com estatísticas</returns>
    /// <response code="200">Importação realizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [Authorize(Roles = $"{Roles.Admin},{Roles.ShopOwner},{Roles.ShopSeller}")]
    [HttpPost("import-files")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleImportResultDto>> ImportVehiclesFromFiles(
        [FromQuery] Guid shopId,
        [FromQuery] string urlPrefix,
        IFormFileCollection files)
    {
        if (string.IsNullOrWhiteSpace(urlPrefix))
            return BadRequest("O prefixo da URL das fotos é obrigatório");

        // Validar se a loja existe e se o usuário tem permissão
        var shop = await _context.Shops
            .FirstOrDefaultAsync(s => s.Id == shopId && !s.IsDeleted);

        if (shop == null)
            return NotFound("Loja não encontrada");

        if (!await IsAdminOrShopMember(shopId))
            return Forbid("Usuário não tem permissão para importar veículos para esta loja");

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = new VehicleImportResultDto();
        var vehiclesToAdd = new List<Vehicle>();
        var optionalsToAdd = new List<VehicleOptional>();
        var photosToAdd = new List<VehiclePhoto>();

        try
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var jsonContent = await reader.ReadToEndAsync();

                    try
                    {
                        // Parse do JSON
                        using var document = JsonDocument.Parse(jsonContent);
                        var root = document.RootElement;

                        // Verificar se existe o array "veiculos"
                        if (!root.TryGetProperty("veiculos", out var vehiclesArray) || vehiclesArray.ValueKind != JsonValueKind.Array)
                        {
                            result.ErrorCount++;
                            result.Errors.Add($"Arquivo {file.FileName}: JSON inválido - propriedade 'veiculos' não encontrada ou não é um array");
                            continue;
                        }

                        // Processar cada veículo
                        foreach (var vehicleJson in vehiclesArray.EnumerateArray())
                        {
                            try
                            {
                                // Verificar se o veículo já existe pelo código externo
                                var externalCode = vehicleJson.TryGetProperty("cod_veiculo", out var codProperty) ? codProperty.GetString() : null;
                                
                                if (!string.IsNullOrEmpty(externalCode))
                                {
                                    var existingVehicle = await _context.Vehicles
                                        .FirstOrDefaultAsync(v => v.ExternalVehicleCode == externalCode && !v.IsDeleted);
                                    
                                    if (existingVehicle != null)
                                    {
                                        result.SkippedCount++;
                                        result.SkippedVehicles.Add($"Veículo com código {externalCode} já existe");
                                        continue;
                                    }
                                }

                                // Criar nova entidade Vehicle
                                var vehicle = new Vehicle { Id = Guid.NewGuid() };
                                vehicle.FromJsonObject(vehicleJson, shopId, userId);

                                // Processar fotos do veículo
                                if (vehicleJson.TryGetProperty("fotos", out var photosArray) && photosArray.ValueKind == JsonValueKind.Array)
                                {
                                    var photoOrder = 0;
                                    foreach (var photoUrl in photosArray.EnumerateArray())
                                    {
                                        if (photoUrl.ValueKind == JsonValueKind.String)
                                        {
                                            var fullUrl = $"{urlPrefix.TrimEnd('/')}/{photoUrl.GetString()?.TrimStart('/')}";
                                            var photo = new VehiclePhoto
                                            {
                                                Id = Guid.NewGuid(),
                                                Url = fullUrl,
                                                FileName = Path.GetFileName(fullUrl),
                                                Order = photoOrder,
                                                IsMain = photoOrder == 0,
                                                VehicleId = vehicle.Id,
                                                CreatedAt = DateTime.UtcNow
                                            };
                                            photosToAdd.Add(photo);
                                            photoOrder++;

                                            // Se for a primeira foto, definir como foto principal
                                            if (photoOrder == 1)
                                            {
                                                vehicle.MainPhotoUrl = fullUrl;
                                            }
                                        }
                                    }
                                }

                                vehiclesToAdd.Add(vehicle);

                                // Processar opcionais do veículo
                                var vehicleOptionals = vehicleJson.GetOptionalsFromJson(vehicle.Id);
                                optionalsToAdd.AddRange(vehicleOptionals);

                                result.ProcessedCount++;
                            }
                            catch (Exception ex)
                            {
                                result.ErrorCount++;
                                result.Errors.Add($"Erro ao processar veículo no arquivo {file.FileName}: {ex.Message}");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Erro ao processar arquivo {file.FileName}: {ex.Message}");
                    }
                }
            }

            // Salvar no banco de dados em uma transação
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                if (vehiclesToAdd.Any())
                {
                    await _context.Vehicles.AddRangeAsync(vehiclesToAdd);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = vehiclesToAdd.Count;
                }

                if (optionalsToAdd.Any())
                {
                    await _context.VehicleOptionals.AddRangeAsync(optionalsToAdd);
                    await _context.SaveChangesAsync();
                }

                if (photosToAdd.Any())
                {
                    await _context.VehiclePhotos.AddRangeAsync(photosToAdd);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                
                result.Success = true;
                result.Message = $"Importação concluída: {result.ImportedCount} veículos importados, {result.SkippedCount} ignorados, {result.ErrorCount} erros";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = "Erro ao salvar no banco de dados";
                result.Errors.Add(ex.Message);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "Erro interno do servidor";
            result.Errors.Add(ex.Message);
        }

        return Ok(result);
    }

    /// <summary>
    /// Importa veículos a partir de um JSON no formato base.json (método legado)
    /// </summary>
    /// <param name="request">Dados da importação incluindo o JSON e ID da loja</param>
    /// <returns>Resultado da importação com estatísticas</returns>
    /// <response code="200">Importação realizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Loja não encontrada</response>
    [Authorize(Roles = $"{Roles.Admin},{Roles.ShopOwner},{Roles.ShopSeller}")]
    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Use o endpoint import-files para importar veículos a partir de arquivos JSON")]
    public async Task<ActionResult<VehicleImportResultDto>> ImportVehicles([FromBody] VehicleImportRequestDto request)
    {
        // Validar se a loja existe e se o usuário tem permissão
        var shop = await _context.Shops
            .FirstOrDefaultAsync(s => s.Id == request.ShopId && !s.IsDeleted);

        if (shop == null)
            return NotFound("Loja não encontrada");

        if (!await IsAdminOrShopMember(request.ShopId))
            return Forbid("Usuário não tem permissão para importar veículos para esta loja");

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = new VehicleImportResultDto();
        var vehiclesToAdd = new List<Vehicle>();
        var optionalsToAdd = new List<VehicleOptional>();

        try
        {
            // Parse do JSON
            using var document = JsonDocument.Parse(request.JsonData);
            var root = document.RootElement;

            // Verificar se existe o array "veiculos"
            if (!root.TryGetProperty("veiculos", out var vehiclesArray) || vehiclesArray.ValueKind != JsonValueKind.Array)
            {
                return BadRequest("JSON inválido: propriedade 'veiculos' não encontrada ou não é um array");
            }

            // Processar cada veículo
            foreach (var vehicleJson in vehiclesArray.EnumerateArray())
            {
                try
                {
                    // Verificar se o veículo já existe pelo código externo
                    var externalCode = vehicleJson.TryGetProperty("cod_veiculo", out var codProperty) ? codProperty.GetString() : null;
                    
                    if (!string.IsNullOrEmpty(externalCode))
                    {
                        var existingVehicle = await _context.Vehicles
                            .FirstOrDefaultAsync(v => v.ExternalVehicleCode == externalCode && !v.IsDeleted);
                        
                        if (existingVehicle != null)
                        {
                            result.SkippedCount++;
                            result.SkippedVehicles.Add($"Veículo com código {externalCode} já existe");
                            continue;
                        }
                    }

                    // Criar nova entidade Vehicle
                    var vehicle = new Vehicle { Id = Guid.NewGuid() };
                    vehicle.FromJsonObject(vehicleJson, request.ShopId, userId);

                    vehiclesToAdd.Add(vehicle);

                    // Processar opcionais do veículo
                    var vehicleOptionals = vehicleJson.GetOptionalsFromJson(vehicle.Id);
                    optionalsToAdd.AddRange(vehicleOptionals);

                    result.ProcessedCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Erro ao processar veículo: {ex.Message}");
                }
            }

            // Salvar no banco de dados em uma transação
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                if (vehiclesToAdd.Any())
                {
                    await _context.Vehicles.AddRangeAsync(vehiclesToAdd);
                    await _context.SaveChangesAsync();
                    result.ImportedCount = vehiclesToAdd.Count;
                }

                if (optionalsToAdd.Any())
                {
                    await _context.VehicleOptionals.AddRangeAsync(optionalsToAdd);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                
                result.Success = true;
                result.Message = $"Importação concluída: {result.ImportedCount} veículos importados, {result.SkippedCount} ignorados, {result.ErrorCount} erros";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Success = false;
                result.Message = "Erro ao salvar no banco de dados";
                result.Errors.Add(ex.Message);
            }
        }
        catch (JsonException ex)
        {
            result.Success = false;
            result.Message = "Erro ao processar JSON";
            result.Errors.Add(ex.Message);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "Erro interno do servidor";
            result.Errors.Add(ex.Message);
        }

        return Ok(result);
    }
} 