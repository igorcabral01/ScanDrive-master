namespace ScanDrive.Domain.DTOs.User;

/// <summary>
/// Requisição para criação de usuário
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Nome de usuário
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Email do usuário
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Número de telefone do usuário
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Papéis (roles) do usuário
    /// </summary>
    public required string[] Roles { get; set; }
} 