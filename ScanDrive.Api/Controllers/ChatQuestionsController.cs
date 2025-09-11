using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.DTOs.ChatQuestion;
using ScanDrive.Domain.DTOs.Common;
using ScanDrive.Domain.Entities;
using ScanDrive.Domain.Settings;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Infrastructure.Extensions;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar as perguntas do chat
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatQuestionsController : BaseController
{
    /// <summary>
    /// Construtor do controller de perguntas do chat
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    public ChatQuestionsController(
        AppDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
        : base(userManager, roleManager, context)
    {
    }

    /// <summary>
    /// Lista todas as perguntas do chat com filtros e paginação
    /// </summary>
    /// <param name="filter">Filtros e parâmetros de paginação</param>
    /// <returns>Lista paginada de perguntas</returns>
    /// <response code="200">Retorna a lista de perguntas</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpGet]
    [Authorize(Policy = $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedList<ChatQuestionDto>>> GetAll([FromQuery] ChatQuestionFilter filter)
    {
        var query = _context.ChatQuestions
            .Where(cq => !cq.IsDeleted)
            .AsQueryable();

        // Aplicar filtros específicos
        if (filter.Step.HasValue)
            query = query.Where(cq => cq.Step == filter.Step.Value);

        if (filter.IsEnabled.HasValue)
            query = query.Where(cq => cq.IsEnabled == filter.IsEnabled.Value);

        // Aplicar filtros base e paginação
        query = query.ApplyFilter(filter);

        var dtoQuery = query.Select(cq => new ChatQuestionDto
        {
            Id = cq.Id,
            Question = cq.Question,
            IsEnabled = cq.IsEnabled,
            Step = cq.Step,
            CreatedAt = cq.CreatedAt,
            UpdatedAt = cq.UpdatedAt
        });

        var result = await dtoQuery.ToPagedListAsync(filter);

        return Ok(result);
    }

    /// <summary>
    /// Obtém uma pergunta específica por ID
    /// </summary>
    /// <param name="id">ID da pergunta</param>
    /// <returns>Dados da pergunta</returns>
    /// <response code="200">Retorna os dados da pergunta</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Pergunta não encontrada</response>
    [HttpGet("{id}")]
    [Authorize(Policy = $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.View}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatQuestionDto>> GetById(Guid id)
    {
        var chatQuestion = await _context.ChatQuestions
            .FirstOrDefaultAsync(cq => cq.Id == id && !cq.IsDeleted);

        if (chatQuestion == null)
            return NotFound();

        var dto = new ChatQuestionDto
        {
            Id = chatQuestion.Id,
            Question = chatQuestion.Question,
            IsEnabled = chatQuestion.IsEnabled,
            Step = chatQuestion.Step,
            CreatedAt = chatQuestion.CreatedAt,
            UpdatedAt = chatQuestion.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Cria uma nova pergunta
    /// </summary>
    /// <param name="request">Dados da pergunta</param>
    /// <returns>Dados da pergunta criada</returns>
    /// <response code="201">Pergunta criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    [HttpPost]
    [Authorize(Policy = $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Create}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatQuestionDto>> Create([FromBody] CreateChatQuestionDto request)
    {
        var chatQuestion = new ChatQuestion
        {
            Question = request.Question,
            IsEnabled = request.IsEnabled,
            Step = request.Step
        };

        _context.ChatQuestions.Add(chatQuestion);
        await _context.SaveChangesAsync();

        var dto = new ChatQuestionDto
        {
            Id = chatQuestion.Id,
            Question = chatQuestion.Question,
            IsEnabled = chatQuestion.IsEnabled,
            Step = chatQuestion.Step,
            CreatedAt = chatQuestion.CreatedAt,
            UpdatedAt = chatQuestion.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = chatQuestion.Id }, dto);
    }

    /// <summary>
    /// Atualiza uma pergunta existente
    /// </summary>
    /// <param name="id">ID da pergunta</param>
    /// <param name="request">Dados atualizados</param>
    /// <returns>Dados da pergunta atualizada</returns>
    /// <response code="200">Pergunta atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Pergunta não encontrada</response>
    [HttpPut("{id}")]
    [Authorize(Policy = $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Edit}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatQuestionDto>> Update(Guid id, [FromBody] UpdateChatQuestionDto request)
    {
        var chatQuestion = await _context.ChatQuestions
            .FirstOrDefaultAsync(cq => cq.Id == id && !cq.IsDeleted);

        if (chatQuestion == null)
            return NotFound();

        chatQuestion.Question = request.Question;
        chatQuestion.IsEnabled = request.IsEnabled;
        chatQuestion.Step = request.Step;
        chatQuestion.UpdatedAt = DateTime.UtcNow;

        _context.ChatQuestions.Update(chatQuestion);
        await _context.SaveChangesAsync();

        var dto = new ChatQuestionDto
        {
            Id = chatQuestion.Id,
            Question = chatQuestion.Question,
            IsEnabled = chatQuestion.IsEnabled,
            Step = chatQuestion.Step,
            CreatedAt = chatQuestion.CreatedAt,
            UpdatedAt = chatQuestion.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Remove uma pergunta (soft delete)
    /// </summary>
    /// <param name="id">ID da pergunta</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Pergunta removida com sucesso</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Usuário não tem permissão</response>
    /// <response code="404">Pergunta não encontrada</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = $"{Claims.Modules.ChatQuestions}:{Claims.Permissions.Delete}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var chatQuestion = await _context.ChatQuestions
            .FirstOrDefaultAsync(cq => cq.Id == id && !cq.IsDeleted);

        if (chatQuestion == null)
            return NotFound();

        chatQuestion.IsDeleted = true;
        chatQuestion.UpdatedAt = DateTime.UtcNow;

        _context.ChatQuestions.Update(chatQuestion);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pergunta removida com sucesso" });
    }

    /// <summary>
    /// Obtém 3 perguntas aleatórias para um step específico
    /// </summary>
    /// <param name="step">Step das perguntas (1 ou 2)</param>
    /// <returns>Lista de 3 perguntas aleatórias</returns>
    /// <response code="200">Retorna as perguntas</response>
    [HttpGet("random/{step}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetRandomQuestions(int step)
    {
        var questions = await _context.ChatQuestions
            .Where(cq => !cq.IsDeleted && cq.IsEnabled && cq.Step == step)
            .Select(cq => cq.Question)
            .ToListAsync();

        var random = new Random();
        var randomQuestions = questions
            .OrderBy(x => random.Next())
            .Take(3)
            .ToList();

        return Ok(randomQuestions);
    }
} 