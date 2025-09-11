using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.Models.Auth;

/// <summary>
/// Modelo para login de usuário
/// </summary>
public class LoginModel
{
    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
} 