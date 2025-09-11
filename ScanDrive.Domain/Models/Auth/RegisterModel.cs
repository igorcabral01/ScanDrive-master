using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.Models.Auth;

/// <summary>
/// Modelo para registro de usuário
/// </summary>
public class RegisterModel
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário (mínimo 6 caracteres)
    /// </summary>
    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmação da senha
    /// </summary>
    [Compare("Password", ErrorMessage = "As senhas não conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;
} 