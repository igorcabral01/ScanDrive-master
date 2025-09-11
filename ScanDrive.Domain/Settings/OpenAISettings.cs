namespace ScanDrive.Domain.Settings;

public class OpenAISettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
    public string AssistantId { get; set; } = "asst_1iXMA2VbOKjnxNFOM9L5Uv7D";
} 