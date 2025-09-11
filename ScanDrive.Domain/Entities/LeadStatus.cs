namespace ScanDrive.Domain.Entities;

/// <summary>
/// Status possíveis para um lead
/// </summary>
public enum LeadStatus
{
    /// <summary>
    /// Lead recém criado
    /// </summary>
    New,

    /// <summary>
    /// Lead foi contatado
    /// </summary>
    Contacted,

    /// <summary>
    /// Lead está em processo de negociação
    /// </summary>
    InProgress,

    /// <summary>
    /// Lead foi qualificado e tem potencial de compra
    /// </summary>
    Qualified,

    /// <summary>
    /// Em negociação final
    /// </summary>
    Negotiating,

    /// <summary>
    /// Lead convertido em venda
    /// </summary>
    Won,

    /// <summary>
    /// Lead perdido/desistiu
    /// </summary>
    Lost,

    /// <summary>
    /// Lead não respondeu aos contatos
    /// </summary>
    NoResponse
} 