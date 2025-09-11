using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar o chat com IA do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : BaseController
{
    private readonly OpenAIAPI _openAI;
    private readonly OpenAISettings _settings;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Construtor do controller de chat
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="settings">Configurações da OpenAI</param>
    public ChatController(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<OpenAISettings> settings)
        : base(userManager, roleManager, context)
    {
        _settings = settings.Value;
        _openAI = new OpenAIAPI(_settings.ApiKey);
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    private async Task<string> CreateThreadAsync()
    {
        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/threads",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        );

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"CreateThread Response: {responseContent}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create thread: {responseContent}");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        if (!result.TryGetProperty("id", out var idProperty))
        {
            throw new Exception($"Response JSON não contém 'id': {responseContent}");
        }

        var threadId = idProperty.GetString();
        Console.WriteLine($"ThreadId criado: '{threadId}'");
        
        if (string.IsNullOrEmpty(threadId))
        {
            throw new Exception($"Thread creation retornou id nulo ou vazio: {responseContent}");
        }

        return threadId;
    }

    private async Task<(string message, string humor, List<string>? photos)> SendMessageAndWaitForResponseAsync(string threadId, string message)
    {
        Console.WriteLine($"SendMessage - ThreadId recebido: '{threadId}'");
        Console.WriteLine($"SendMessage - ThreadId é nulo/vazio: {string.IsNullOrEmpty(threadId)}");
        
        if (string.IsNullOrEmpty(threadId))
        {
            throw new Exception("ThreadId é nulo ou vazio no SendMessageAndWaitForResponseAsync");
        }

        // Add the message to the thread
        var createMessageRequest = new
        {
            role = "user",
            content = message
        };

        var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
        Console.WriteLine($"URL sendo chamada: {url}");
        Console.WriteLine($"ThreadId na URL: '{threadId}'");
        Console.WriteLine($"URL completa: {url}");

        var messageResponse = await _httpClient.PostAsync(
            url,
            new StringContent(JsonSerializer.Serialize(createMessageRequest), System.Text.Encoding.UTF8, "application/json")
        );

        if (!messageResponse.IsSuccessStatusCode)
        {
            var errorContent = await messageResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro na requisição: {errorContent}");
            throw new Exception($"Failed to add message: {errorContent}");
        }

        // Create a run with the assistant
        var createRunRequest = new
        {
            assistant_id = _settings.AssistantId
        };

        var runResponse = await _httpClient.PostAsync(
            $"https://api.openai.com/v1/threads/{threadId}/runs",
            new StringContent(JsonSerializer.Serialize(createRunRequest), System.Text.Encoding.UTF8, "application/json")
        );

        if (!runResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create run: {await runResponse.Content.ReadAsStringAsync()}");
        }

        var runResult = JsonSerializer.Deserialize<JsonElement>(await runResponse.Content.ReadAsStringAsync());
        var runId = runResult.GetProperty("id").GetString();

        // Poll for completion
        while (true)
        {
            var statusResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
            if (!statusResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get run status: {await statusResponse.Content.ReadAsStringAsync()}");
            }

            var statusResult = JsonSerializer.Deserialize<JsonElement>(await statusResponse.Content.ReadAsStringAsync());
            var status = statusResult.GetProperty("status").GetString();

            if (status == "completed")
            {
                break;
            }
            else if (status == "failed" || status == "cancelled" || status == "expired")
            {
                throw new Exception($"Run failed with status: {status}");
            }

            await Task.Delay(1000);
        }

        // Get the assistant's response
        var messagesResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages");
        if (!messagesResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get messages: {await messagesResponse.Content.ReadAsStringAsync()}");
        }

        var messagesJson = await messagesResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"OpenAI Response: {messagesJson}"); // Debug log

        try
        {
            var responseJson = JsonSerializer.Deserialize<JsonElement>(messagesJson);
            var firstMessage = responseJson.GetProperty("data").EnumerateArray().First();
            var content = firstMessage.GetProperty("content").EnumerateArray().First();
            var responseContent = content.GetProperty("text").GetProperty("value").GetString();

            Console.WriteLine($"Response Content: {responseContent}"); // Debug log

            // Remove any BOM or invalid characters and try to parse the JSON
            responseContent = responseContent?.TrimStart('\uFEFF', '\u200B');
            Console.WriteLine($"Response content limpo: {responseContent}");
            
            var assistantResponse = JsonSerializer.Deserialize<JsonElement>(responseContent ?? "{}");

            // O schema esperado tem threadId, message e humor
            if (assistantResponse.TryGetProperty("message", out var messageElement) && 
                assistantResponse.TryGetProperty("humor", out var humorElement))
            {
                var responseMessage = messageElement.GetString();
                var responseHumor = humorElement.GetString();

                Console.WriteLine($"Message extraída: {responseMessage}");
                Console.WriteLine($"Humor extraído: {responseHumor}");

                if (!string.IsNullOrEmpty(responseMessage) && !string.IsNullOrEmpty(responseHumor))
                {
                    // Verifica se a mensagem contém um link de testdrive com GUID
                    var photos = await ExtractVehiclePhotosFromMessage(responseMessage);
                    return (responseMessage, responseHumor, photos);
                }
            }

            // Se não encontrar a estrutura esperada, retorna a resposta completa
            Console.WriteLine("Estrutura JSON não encontrada, retornando resposta completa");
            var fallbackPhotos = await ExtractVehiclePhotosFromMessage(responseContent ?? "");
            return (responseContent ?? "Erro ao processar resposta", "neutral", fallbackPhotos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex.Message}"); // Debug log
            // Se falhar ao parsear o JSON, assume que a resposta inteira é a mensagem
            var fallbackPhotos = await ExtractVehiclePhotosFromMessage(messagesJson ?? "");
            return (messagesJson ?? "Erro ao processar resposta", "neutral", fallbackPhotos);
        }
    }

    /// <summary>
    /// Extrai fotos do veículo se a mensagem contém um link de testdrive com GUID
    /// </summary>
    /// <param name="message">Mensagem do chat</param>
    /// <returns>Lista de URLs das fotos do veículo ou null se não encontrar</returns>
    private async Task<List<string>?> ExtractVehiclePhotosFromMessage(string message)
    {
        try
        {
            // Regex para encontrar GUIDs na mensagem
            var guidPattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
            var matches = Regex.Matches(message, guidPattern);

            foreach (Match match in matches)
            {
                if (Guid.TryParse(match.Value, out var vehicleId))
                {
                    // Verifica se existe um veículo com esse ID
                    var vehicle = await _context.Vehicles
                        .Include(v => v.Photos)
                        .FirstOrDefaultAsync(v => v.Id == vehicleId);

                    if (vehicle != null)
                    {
                        // Retorna as URLs das fotos ordenadas
                        var photoUrls = vehicle.Photos
                            .OrderBy(p => p.Order)
                            .Select(p => p.Url)
                            .ToList();

                        Console.WriteLine($"Fotos encontradas para veículo {vehicleId}: {photoUrls.Count} fotos");
                        return photoUrls;
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao extrair fotos do veículo: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Envia uma mensagem para o chatbot
    /// </summary>
    /// <param name="request">Dados da mensagem</param>
    /// <returns>Resposta do chatbot</returns>
    /// <response code="200">Retorna a resposta do chatbot</response>
    /// <response code="500">Erro ao processar mensagem</response>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            string? userId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            string tmpMessage = request.Message;

            if (request.VehicleId.HasValue && request.ShopId.HasValue)
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Shop)
                    .FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.ShopId == request.ShopId);

                if (vehicle == null)
                    return BadRequest(new { error = "Veículo não encontrado ou não pertence à loja informada" });

                tmpMessage = $"Loja: {vehicle.Shop?.Name} | Veículo: {vehicle.Brand} {vehicle.Model}-{vehicle.Year} ID: {vehicle.Id} | " + request.Message;
            }

            var chatSession = await _context.ChatSessions
                .Include(cs => cs.Messages)
                .FirstOrDefaultAsync(cs => cs.SessionId == request.SessionId && cs.UserId == userId && cs.IsActive);

            if (chatSession != null &&
                ((chatSession.ShopId != request.ShopId) || (chatSession.VehicleId != request.VehicleId)))
            {
                chatSession.IsActive = false;
                await _context.SaveChangesAsync();
                chatSession = null;
            }

            if (chatSession == null)
            {
                var threadId = await CreateThreadAsync();
                Console.WriteLine($"ThreadId criado para nova sessão: '{threadId}'");

                chatSession = new ChatSession
                {
                    SessionId = request.SessionId,
                    UserId = userId,
                    IsActive = true,
                    LastActivity = DateTime.UtcNow,
                    ThreadId = threadId,
                    ShopId = request.ShopId,
                    VehicleId = request.VehicleId
                };

                Console.WriteLine($"ChatSession antes de salvar - ThreadId: '{chatSession.ThreadId}'");
                _context.ChatSessions.Add(chatSession);
                await _context.SaveChangesAsync(); // Salva para garantir que chatSession.Id exista
                Console.WriteLine($"ChatSession após salvar - ThreadId: '{chatSession.ThreadId}'");
                
                // Recarrega a sessão do banco para verificar se foi salva corretamente
                var reloadedSession = await _context.ChatSessions.FindAsync(chatSession.Id);
                Console.WriteLine($"ChatSession recarregada do banco - ThreadId: '{reloadedSession?.ThreadId}'");
            }
            else
            {
                Console.WriteLine($"Usando sessão existente - ThreadId: '{chatSession.ThreadId}'");
                
                // Se a sessão existente não tem ThreadId válido, cria um novo
                if (string.IsNullOrEmpty(chatSession.ThreadId))
                {
                    Console.WriteLine("ThreadId da sessão existente está vazio, criando novo thread...");
                    var newThreadId = await CreateThreadAsync();
                    chatSession.ThreadId = newThreadId;
                    Console.WriteLine($"Novo ThreadId criado para sessão existente: '{newThreadId}'");
                }
                
                chatSession.LastActivity = DateTime.UtcNow;
                _context.ChatSessions.Update(chatSession);
                await _context.SaveChangesAsync();
            }

            // Agora, chatSession.Id está garantido e ThreadId existe
            Console.WriteLine($"ChatSession.Id: {chatSession.Id}");
            Console.WriteLine($"ChatSession.ThreadId: '{chatSession.ThreadId}'");
            Console.WriteLine($"ThreadId é nulo/vazio: {string.IsNullOrEmpty(chatSession.ThreadId)}");

            if (string.IsNullOrEmpty(chatSession.ThreadId))
            {
                throw new Exception("ChatSession.ThreadId é nulo ou vazio antes de chamar SendMessageAndWaitForResponseAsync");
            }

            var userMessage = new Domain.Entities.ChatMessage
            {
                Content = tmpMessage,
                IsFromUser = true,
                Timestamp = DateTime.UtcNow,
                ChatSessionId = chatSession.Id
            };

            _context.ChatMessages.Add(userMessage); // Adiciona diretamente ao DbSet

            var (responseMessage, responseHumor, photos) = await SendMessageAndWaitForResponseAsync(chatSession.ThreadId, tmpMessage);

            var botMessage = new Domain.Entities.ChatMessage
            {
                Content = responseMessage,
                IsFromUser = false,
                Timestamp = DateTime.UtcNow,
                ChatSessionId = chatSession.Id
            };

            _context.ChatMessages.Add(botMessage);

            await _context.SaveChangesAsync();

            // Determina o step baseado na presença de fotos (links de testdrive)
            var step = photos != null && photos.Any() ? 2 : 1;

            // Busca 3 opções aleatórias para o step
            var options = await GetRandomOptionsForStep(step);

            return Ok(new { 
                message = responseMessage, 
                humor = responseHumor, 
                photos = photos,
                options = options
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao processar mensagem", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém 3 opções aleatórias para um step específico
    /// </summary>
    /// <param name="step">Step das opções (1 ou 2)</param>
    /// <returns>Lista de 3 opções aleatórias</returns>
    private async Task<List<string>> GetRandomOptionsForStep(int step)
    {
        try
        {
            var questions = await _context.ChatQuestions
                .Where(cq => !cq.IsDeleted && cq.IsEnabled && cq.Step == step)
                .Select(cq => cq.Question)
                .ToListAsync();

            var random = new Random();
            var randomOptions = questions
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();

            return randomOptions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar opções para step {step}: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Reseta uma conversa
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Conversa resetada com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="500">Erro ao resetar conversa</response>
    [HttpPost("reset")]
    [Authorize(Roles = $"{Roles.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetConversation([FromBody] string sessionId)
    {
        try
        {
            var chatSession = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.SessionId == sessionId);

            if (chatSession != null)
            {
                // Delete the thread from OpenAI if it exists
                if (!string.IsNullOrEmpty(chatSession.ThreadId))
                {
                    await _httpClient.DeleteAsync($"https://api.openai.com/v1/threads/{chatSession.ThreadId}");
                }

                chatSession.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Conversa resetada com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro ao resetar conversa", details = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas as sessões de chat do usuário
    /// </summary>
    /// <returns>Lista de sessões</returns>
    /// <response code="200">Retorna a lista de sessões</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("sessions")]
    [Authorize(Policy = "Module.Chat:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> GetSessions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var sessions = await _context.ChatSessions
            .Where(cs => cs.UserId == userId || User.IsInRole(Roles.Admin))  // Admin pode ver todas as sessões
            .OrderByDescending(cs => cs.LastActivity)
            .Select(cs => new
            {
                cs.SessionId,
                cs.LastActivity,
                cs.IsActive,
                MessageCount = cs.Messages.Count,
                UserId = cs.UserId  // Incluir UserId para admins identificarem o dono
            })
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Lista todas as mensagens de uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Lista de mensagens</returns>
    /// <response code="200">Retorna a lista de mensagens</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet("session/{sessionId}/messages")]
    [Authorize(Policy = "Module.Chat:Permission.View")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> GetSessionMessages(string sessionId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Admin pode ver mensagens de qualquer sessão
        var isAdmin = User.IsInRole(Roles.Admin);
        var messages = await _context.ChatMessages
            .Where(m => m.ChatSession.SessionId == sessionId && (m.ChatSession.UserId == userId || isAdmin))
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Content,
                m.IsFromUser,
                m.Timestamp,
                UserId = m.ChatSession.UserId  // Incluir UserId para admins identificarem o dono
            })
            .ToListAsync();

        return Ok(messages);
    }

    /// <summary>
    /// Retorna as 10 palavras mais frequentes nas mensagens dos usuários
    /// </summary>
    /// <returns>Lista das palavras mais frequentes com suas contagens</returns>
    [HttpGet("keywords")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ShopOwner}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> GetTopKeywords()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Lista de stop words em português para filtrar
        var stopWords = new HashSet<string>
        {
            // Artigos
            "a", "o", "e", "é", "de", "do", "da", "em", "para", "com",
            "um", "uma", "os", "as", "que", "não", "na", "no", "por",
            "seu", "sua", "mais", "menos", "muito", "pouco", "este",
            "esta", "isso", "aquilo", "ele", "ela", "eles", "elas",
            
            // Pronomes
            "eu", "tu", "ele", "ela", "nós", "vós", "eles", "elas",
            "me", "te", "se", "nos", "vos", "lhe", "lhes",
            
            // Preposições
            "ante", "após", "até", "com", "contra", "desde", "entre",
            "para", "perante", "por", "sem", "sob", "sobre", "trás",
            
            // Conjunções
            "e", "mas", "porém", "todavia", "contudo", "entretanto",
            "ou", "nem", "que", "porque", "pois", "como", "quando",
            
            // Advérbios
            "muito", "pouco", "mais", "menos", "bem", "mal", "hoje",
            "ontem", "amanhã", "agora", "antes", "depois", "sempre",
            "nunca", "jamais", "também", "tampouco", "sim", "não",
            
            // Verbos comuns
            "ser", "estar", "ter", "haver", "fazer", "dizer", "ir",
            "vir", "ver", "dar", "querer", "poder", "dever", "saber",
            "quer", "quero", "gostaria", "pode", "poderia", "deve",
            "deveria", "vou", "vai", "vem", "vindo", "indo", "feito",
            "dito", "visto", "dado", "querido", "podido", "devido",
            "sabido", "estou", "está", "estava", "estive", "estiver",
            "tenho", "tem", "tinha", "tive", "tiver", "hei", "há",
            "havia", "houve", "houver", "faço", "faz", "fazia", "fiz",
            "fizer", "digo", "diz", "dizia", "disse", "disser",
            
            // Palavras específicas do contexto
            "veículo", "id", "procurando", "procurar", "encontrar", "encontrei",
            "preço", "valor", "custo", "pagamento", "trocaria", "trocaria",
            "consulta", "consultar", "informação", "informações",
            "detalhes", "detalhe", "especificações", "especificação",
            "disponível", "disponibilidade", "estoque", "novo", "usado", 
            "seminovo", "km", "quilometragem", "ano", "marca", "modelo", 
            "versão", "versao", "cor", "cambio", "câmbio", "combustível",
            "combustivel", "gasolina", "álcool", "alcool", "diesel", "flex",
            "gasolina", "álcool", "alcool", "diesel", "flex", "híbrido", 
            "hibrido", "elétrico", "eletrico"
        };

        // Busca todas as mensagens dos usuários
        var query = _context.ChatMessages
            .Include(m => m.ChatSession)
            .Where(m => m.IsFromUser); // Apenas mensagens do usuário

        // Se for ShopOwner, filtra apenas as mensagens das sessões relacionadas às suas lojas
        if (User.IsInRole(Roles.ShopOwner))
        {
            var userShops = await _context.Shops
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            var shopVehicles = await _context.Vehicles
                .Where(v => userShops.Contains(v.ShopId))
                .Select(v => v.Id)
                .ToListAsync();

            // Filtra mensagens que contenham IDs de veículos da loja do usuário
            query = query.Where(m => shopVehicles.Any(vehicleId => 
                m.Content.Contains(vehicleId.ToString())));
        }

        var allMessages = await query
            .Where(m => !string.IsNullOrEmpty(m.Content))
            .Select(m => m.Content)
            .ToListAsync();

        // Processa as palavras
        var wordCount = allMessages
            .SelectMany(message => message.Split(new[] { ' ', '\n', '\r', '.', ',', '!', '?', ';', ':', '-', '(', ')', '|' }, 
                StringSplitOptions.RemoveEmptyEntries))
            .Select(word => word.ToLower().Trim())
            .Where(word => 
                word.Length > 2 && // Ignora palavras muito curtas
                !stopWords.Contains(word) && // Remove stop words
                !int.TryParse(word, out _) && // Remove números
                !word.Contains("http") && // Remove URLs
                !word.Contains("@") && // Remove emails
                !word.Contains("www")) // Remove URLs
            .GroupBy(word => word)
            .Select(group => new
            {
                keyword = group.Key,
                count = group.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(10)
            .ToList();

        return Ok(wordCount);
    }
}

/// <summary>
/// Requisição para envio de mensagem
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// ID da sessão de chat
    /// </summary>
    public required string SessionId { get; set; }

    /// <summary>
    /// Conteúdo da mensagem
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// ID do veículo (opcional, quando iniciado de um anúncio específico)
    /// </summary>
    public Guid? VehicleId { get; set; }

    /// <summary>
    /// ID da loja (opcional, quando iniciado de um anúncio específico)
    /// </summary>
    public Guid? ShopId { get; set; }
} 