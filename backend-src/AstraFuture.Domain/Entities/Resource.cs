namespace AstraFuture.Domain.Entities;

/// <summary>
/// Representa um recurso que pode ser agendado (profissional, sala, equipamento)
/// </summary>
public class Resource : BaseEntity
{
    public Guid TenantId { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    /// <summary>
    /// Tipo do recurso: professional, room, equipment
    /// </summary>
    public ResourceType Type { get; private set; }
    
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    
    /// <summary>
    /// Cor para exibição no calendário (hex)
    /// </summary>
    public string Color { get; private set; } = "#3B82F6";
    
    /// <summary>
    /// Campos customizáveis por tenant (JSON string)
    /// </summary>
    public string? MetaFields { get; private set; }
    
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public ICollection<Appointment>? Appointments { get; private set; }

    private Resource() { } // For EF Core

    private Resource(
        Guid tenantId,
        string name,
        ResourceType type,
        string? description,
        string? email,
        string? phone,
        string color,
        string? metaFields = null)
    {
        TenantId = tenantId;
        Name = name;
        Type = type;
        Description = description;
        Email = email;
        Phone = phone;
        Color = color;
        MetaFields = metaFields;
    }

    public static Resource Create(
        Guid tenantId,
        string name,
        ResourceType type,
        string? description = null,
        string? email = null,
        string? phone = null,
        string color = "#3B82F6",
        string? metaFields = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");

        return new Resource(tenantId, name, type, description, email, phone, color, metaFields);
    }

    public void UpdateDetails(string name, string? description, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required");
        
        Name = name;
        Description = description;
        Email = email;
        Phone = phone;
        MarkAsUpdated();
    }

    public void ChangeColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color is required");
        
        Color = color;
        MarkAsUpdated();
    }

    // Para manipulação de metaFields, use métodos utilitários para serializar/desserializar JSON

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}

public enum ResourceType
{
    Professional = 1,
    Room = 2,
    Equipment = 3
}
