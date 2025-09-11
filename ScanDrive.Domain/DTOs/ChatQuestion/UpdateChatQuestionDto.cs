using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.ChatQuestion;

public class UpdateChatQuestionDto
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = null!;
    
    public bool IsEnabled { get; set; }
    
    [Required]
    [Range(1, 2)]
    public int Step { get; set; }
} 