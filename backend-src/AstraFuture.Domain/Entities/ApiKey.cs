namespace AstraFuture.Domain.Entities;

public class ApiKey : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Limites de uso
    public int? RateLimit { get; set; } // Requests por minuto
    public int UsageCount { get; set; } = 0;
    
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public bool CanUse() => IsActive && !IsExpired();
}
