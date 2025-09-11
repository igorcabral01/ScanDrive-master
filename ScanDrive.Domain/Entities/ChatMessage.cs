namespace ScanDrive.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatSessionId { get; set; } = Guid.Empty;
    public string Content { get; set; } = null!;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; }

    // Relacionamento
    public virtual ChatSession ChatSession { get; set; } = null!;
} 