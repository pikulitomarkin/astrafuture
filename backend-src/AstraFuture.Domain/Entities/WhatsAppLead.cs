namespace AstraFuture.Domain.Entities;

public class WhatsAppLead : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? CustomerId { get; set; } // Null até converter em Customer
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadSource Source { get; set; } = LeadSource.WhatsApp;
    public string? Notes { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum LeadStatus
{
    New = 0,
    InProgress = 1,
    Converted = 2,
    Lost = 3
}

public enum LeadSource
{
    WhatsApp = 0,
    Manual = 1,
    Import = 2
}
