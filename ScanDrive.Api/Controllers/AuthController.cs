using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ScanDrive.Domain.Models.Auth;
using ScanDrive.Domain.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ScanDrive.Api.Controllers;

/// <summary>
/// Controller responsável por gerenciar a autenticação e autorização do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Construtor do controller de autenticação
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity</param>
    /// <param name="signInManager">Gerenciador de login do Identity</param>
    /// <param name="roleManager">Gerenciador de papéis do Identity</param>
    /// <param name="jwtSettings">Configurações do JWT</param>
    /// <param name="configuration">Configurações do aplicativo</param>
    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtSettings> jwtSettings,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <param name="model">Dados do usuário</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Usuário registrado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Adiciona a claim de Nome
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, model.Name));
            
            // Adiciona role padrão de usuário
            await _userManager.AddToRoleAsync(user, Roles.User);

            return Ok(new { Message = "Usuário registrado com sucesso!" });
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Realiza o login do usuário
    /// </summary>
    /// <param name="model">Credenciais do usuário</param>
    /// <returns>Token JWT</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
                return Unauthorized(new { Message = "Usuário não encontrado" });

            var token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        return Unauthorized(new { Message = "Email ou senha inválidos" });
    }

    /// <summary>
    /// Gera um token JWT para o usuário
    /// </summary>
    /// <param name="user">Usuário</param>
    /// <returns>Token JWT</returns>
    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var role = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role.FirstOrDefault(Roles.User))
        };

        // Adiciona claims customizadas do usuário
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Adiciona roles e suas claims
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var roleName in userRoles)
        {
            // Adiciona as claims da role
            if (Claims.DefaultClaims.RoleClaims.ContainsKey(roleName))
            {
                var roleClaims = Claims.DefaultClaims.RoleClaims[roleName]
                    .Select(claim => new Claim("Permission", claim));
                claims.AddRange(roleClaims);
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT key not found")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"]));

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 