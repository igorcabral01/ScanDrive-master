using Microsoft.AspNetCore.Identity;

namespace ScanDrive.Domain.Entities;

public class ChatSession : BaseEntity
{
    public string SessionId { get; set; } = null!;
    public string? UserId { get; set; }  // Nullable para permitir usuários anônimos
    public DateTime LastActivity { get; set; }
    public bool IsActive { get; set; }
    public string? ThreadId { get; set; }  // ID do thread no OpenAI Assistant
    
    public Guid? ShopId { get; set; }
    public Shop? Shop { get; set; }
    
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    // Relacionamentos
    public virtual IdentityUser? User { get; set; }
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
} 