namespace ScanDrive.Domain.DTOs.User;

/// <summary>
/// Requisição para atualização de usuário
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Novo email do usuário
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Nova senha do usuário
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Novo número de telefone do usuário
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Define se o usuário pode ser bloqueado
    /// </summary>
    public bool? LockoutEnabled { get; set; }
} 