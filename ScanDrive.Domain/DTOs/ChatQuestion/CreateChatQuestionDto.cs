using System.ComponentModel.DataAnnotations;

namespace ScanDrive.Domain.DTOs.ChatQuestion;

public class CreateChatQuestionDto
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = null!;
    
    public bool IsEnabled { get; set; } = true;
    
    [Required]
    [Range(1, 2)]
    public int Step { get; set; } = 1;
} 