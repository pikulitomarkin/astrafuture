namespace AstraFuture.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid TenantId { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    
    public string CustomerType { get; private set; } = "individual";
    public string? TaxId { get; private set; }
    public string? CompanyName { get; private set; }
    
    public Dictionary<string, object> MetaFields { get; private set; } = new();
    
    public bool IsActive { get; private set; } = true;

    private Customer() { } // For EF Core

    private Customer(Guid tenantId, string name, string email, string? phone, string customerType)
    {
        TenantId = tenantId;
        Name = name;
        Email = email;
        Phone = phone;
        CustomerType = customerType;
    }

    public static Customer Create(Guid tenantId, string name, string email, string? phone = null, string customerType = "individual")
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");

        return new Customer(tenantId, name, email, phone, customerType);
    }

    public void UpdateContactInfo(string name, string email, string? phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
        MarkAsUpdated();
    }

    public void SetMetaField(string key, object value)
    {
        MetaFields[key] = value;
        MarkAsUpdated();
    }

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
