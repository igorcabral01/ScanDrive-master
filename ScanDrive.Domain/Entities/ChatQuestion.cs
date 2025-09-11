using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.Entities;

public class ChatQuestion : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = null!;
    
    public bool IsEnabled { get; set; } = true;
    
    [Required]
    public int Step { get; set; } = 1; // 1 = chat iniciado, 2 = chat em andamento
} 