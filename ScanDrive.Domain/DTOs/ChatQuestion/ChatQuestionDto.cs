namespace ScanDrive.Domain.DTOs.ChatQuestion;

public class ChatQuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public int Step { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 