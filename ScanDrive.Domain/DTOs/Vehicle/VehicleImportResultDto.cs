namespace ScanDrive.Domain.DTOs.Vehicle;

public class VehicleImportResultDto
{
    /// <summary>
    /// Indica se a importação foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensagem resumo da operação
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Total de veículos processados do JSON
    /// </summary>
    public int ProcessedCount { get; set; }
    
    /// <summary>
    /// Número de veículos importados com sucesso
    /// </summary>
    public int ImportedCount { get; set; }
    
    /// <summary>
    /// Número de veículos ignorados (já existiam)
    /// </summary>
    public int SkippedCount { get; set; }
    
    /// <summary>
    /// Número de veículos com erro
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// Lista de erros encontrados durante a importação
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Lista de veículos que foram ignorados
    /// </summary>
    public List<string> SkippedVehicles { get; set; } = new();
    
    /// <summary>
    /// Data e hora da importação
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
} 