using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using System.Text.Json;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por sincronizar dados com o OpenAI Assistant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Permite acesso sem autenticação, controle via firewall
public class AssistantSyncController : BaseController
{
    private readonly OpenAISettings _settings;
    private readonly HttpClient _httpClient;
    private readonly Random _random = new Random();
    private const string API_BASE_URL = "https://api.openai.com/v1";

    /// <summary>
    /// Construtor do controller de sincronização
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="settings">Configurações da OpenAI</param>
    public AssistantSyncController(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<OpenAISettings> settings)
        : base(userManager, roleManager, context)
    {
        _settings = settings.Value;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    private string GenerateStockFileName()
    {
        var now = DateTime.Now;
        var random = _random.Next(1000, 9999); // Gera um número aleatório entre 1000 e 9999
        return $"estoque_{now:ddMMyyyy}_{random}.json";
    }

    /// <summary>
    /// Sincroniza a lista de veículos com o assistente
    /// </summary>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Lista sincronizada com sucesso</response>
    /// <response code="500">Erro ao sincronizar lista</response>
    [HttpGet("sync-vehicles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SyncVehicles()
    {
        try
        {
            // Busca a lista de veículos disponíveis
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .Select(v => new
                {
                    v.Id,
                    v.Brand,
                    v.Model,
                    v.Year,
                    v.Price,
                    v.Mileage,
                    v.Color,
                    v.Transmission,
                    v.FuelType,
                    v.HasAuction,
                    v.HasAccident,
                    v.IsFirstOwner,
                    v.OwnersCount,
                    v.Features,
                    v.ShopId
                })
                .ToListAsync();

            var shops = await _context.Shops
                .Where(s => !s.IsDeleted)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            // Formata a lista de veículos como JSON
            var vehicleList = vehicles.Select(v => new
            {
                v.Id,
                v.Brand,
                v.Model,
                v.Year,
                v.Price,
                v.Mileage,
                v.Color,
                v.Transmission,
                v.FuelType,
                HasAuction = v.HasAuction ? "Sim" : "Não",
                HasAccident = v.HasAccident ? "Sim" : "Não",
                IsFirstOwner = v.IsFirstOwner ? "Sim" : "Não",
                ShopName = shops.FirstOrDefault(s => s.Id == v.ShopId)?.Name
            });

            var jsonContent = JsonSerializer.Serialize(vehicleList, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Gera o nome do arquivo com data e número aleatório
            var stockFileName = GenerateStockFileName();

            // 1. Faz upload do novo arquivo
            var uploadResponse = await _httpClient.PostAsync(
                $"{API_BASE_URL}/files",
                new MultipartFormDataContent
                {
                    { content, "file", stockFileName },
                    { new StringContent("assistants"), "purpose" }
                }
            );

            if (!uploadResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload file: {await uploadResponse.Content.ReadAsStringAsync()}");
            }

            var uploadResult = JsonSerializer.Deserialize<JsonElement>(await uploadResponse.Content.ReadAsStringAsync());
            var fileId = uploadResult.GetProperty("id").GetString();

            // 2. Busca o assistente para obter o vector store atual
            var assistantResponse = await _httpClient.GetAsync($"{API_BASE_URL}/assistants/{_settings.AssistantId}");
            if (!assistantResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get assistant: {await assistantResponse.Content.ReadAsStringAsync()}");
            }

            var assistantJson = await assistantResponse.Content.ReadAsStringAsync();
            var assistant = JsonSerializer.Deserialize<JsonElement>(assistantJson);

            string? vectorStoreId = null;

            // Verifica se o assistente já tem um vector store
            if (assistant.TryGetProperty("tool_resources", out var toolResources) &&
                toolResources.TryGetProperty("file_search", out var fileSearch) &&
                fileSearch.TryGetProperty("vector_store_ids", out var vectorStoreIds) &&
                vectorStoreIds.GetArrayLength() > 0)
            {
                vectorStoreId = vectorStoreIds[0].GetString();
            }

            // 3. Se não existe vector store, cria um novo
            if (string.IsNullOrEmpty(vectorStoreId))
            {
                var createVectorStoreRequest = new
                {
                    name = "ScanDrive Vehicle Stock",
                    file_ids = new[] { fileId }
                };

                var createVectorStoreResponse = await _httpClient.PostAsync(
                    $"{API_BASE_URL}/vector_stores",
                    new StringContent(JsonSerializer.Serialize(createVectorStoreRequest), System.Text.Encoding.UTF8, "application/json")
                );

                if (!createVectorStoreResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create vector store: {await createVectorStoreResponse.Content.ReadAsStringAsync()}");
                }

                var vectorStoreResult = JsonSerializer.Deserialize<JsonElement>(await createVectorStoreResponse.Content.ReadAsStringAsync());
                vectorStoreId = vectorStoreResult.GetProperty("id").GetString();

                // Atualiza o assistente para usar o vector store
                var updateAssistantRequest = new
                {
                    tool_resources = new
                    {
                        file_search = new
                        {
                            vector_store_ids = new[] { vectorStoreId }
                        }
                    }
                };

                var updateAssistantResponse = await _httpClient.PostAsync(
                    $"{API_BASE_URL}/assistants/{_settings.AssistantId}",
                    new StringContent(JsonSerializer.Serialize(updateAssistantRequest), System.Text.Encoding.UTF8, "application/json")
                );

                if (!updateAssistantResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to update assistant: {await updateAssistantResponse.Content.ReadAsStringAsync()}");
                }
            }
            else
            {
                // 4. Se já existe vector store, remove arquivos antigos e adiciona o novo
                // Busca arquivos do vector store
                var vectorStoreFilesResponse = await _httpClient.GetAsync($"{API_BASE_URL}/vector_stores/{vectorStoreId}/files");
                if (vectorStoreFilesResponse.IsSuccessStatusCode)
                {
                    var vectorStoreFilesJson = await vectorStoreFilesResponse.Content.ReadAsStringAsync();
                    var vectorStoreFiles = JsonSerializer.Deserialize<JsonElement>(vectorStoreFilesJson);

                    // Remove arquivos antigos que começam com "estoque_"
                    foreach (var file in vectorStoreFiles.GetProperty("data").EnumerateArray())
                    {
                        var oldFileId = file.GetProperty("id").GetString();
                        await _httpClient.DeleteAsync($"{API_BASE_URL}/vector_stores/{vectorStoreId}/files/{oldFileId}");
                        // Também deleta o arquivo do storage
                        await _httpClient.DeleteAsync($"{API_BASE_URL}/files/{oldFileId}");
                    }
                }

                // Adiciona o novo arquivo ao vector store
                var addFileRequest = new
                {
                    file_id = fileId
                };

                var addFileResponse = await _httpClient.PostAsync(
                    $"{API_BASE_URL}/vector_stores/{vectorStoreId}/files",
                    new StringContent(JsonSerializer.Serialize(addFileRequest), System.Text.Encoding.UTF8, "application/json")
                );

                if (!addFileResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to add file to vector store: {await addFileResponse.Content.ReadAsStringAsync()}");
                }
            }

            return Ok(new { message = "Lista de veículos sincronizada com sucesso", fileName = stockFileName, vectorStoreId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao sincronizar lista de veículos", details = ex.Message });
        }
    }
} 