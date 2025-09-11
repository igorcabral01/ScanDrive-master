using ScanDrive.Domain.DTOs.Common;

namespace ScanDrive.Domain.DTOs.ChatQuestion;

public class ChatQuestionFilter : BaseFilter
{
    public int? Step { get; set; }
    public bool? IsEnabled { get; set; }
} 