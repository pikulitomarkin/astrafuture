namespace AstraFuture.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ResourceId { get; private set; }
    
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public DateTime ScheduledAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public int DurationMinutes { get; private set; }
    
    public AppointmentStatus Status { get; private set; }
    public string AppointmentType { get; private set; } = "consultation";
    
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Navigation properties
    public Customer? Customer { get; private set; }

    private Appointment() { } // For EF Core

    private Appointment(
        Guid tenantId,
        Guid customerId,
        Guid resourceId,
        string title,
        DateTime scheduledAt,
        int durationMinutes,
        string appointmentType)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        ResourceId = resourceId;
        Title = title;
        ScheduledAt = scheduledAt;
        DurationMinutes = durationMinutes;
        EndsAt = scheduledAt.AddMinutes(durationMinutes);
        AppointmentType = appointmentType;
        Status = AppointmentStatus.Scheduled;
    }

    // Factory method
    public static Appointment Create(
        Guid tenantId,
        Guid customerId,
        Guid resourceId,
        string title,
        DateTime scheduledAt,
        int durationMinutes,
        string appointmentType = "consultation")
    {
        // Business validations
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required");
        if (customerId == Guid.Empty) throw new ArgumentException("CustomerId is required");
        if (resourceId == Guid.Empty) throw new ArgumentException("ResourceId is required");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required");
        if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive");
        if (scheduledAt < DateTime.UtcNow.AddMinutes(-5)) throw new ArgumentException("Cannot schedule in the past");

        return new Appointment(tenantId, customerId, resourceId, title, scheduledAt, durationMinutes, appointmentType);
    }

    // Business methods
    public void Reschedule(DateTime newScheduledAt, int? newDuration = null)
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule completed appointment");
        
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule cancelled appointment");

        ScheduledAt = newScheduledAt;
        
        if (newDuration.HasValue)
            DurationMinutes = newDuration.Value;
        
        EndsAt = ScheduledAt.AddMinutes(DurationMinutes);
        MarkAsUpdated();
    }

    public void UpdateDetails(string title, string? description = null)
    {
        Title = title;
        Description = description;
        MarkAsUpdated();
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        MarkAsUpdated();
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Appointment is already completed");
        
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete cancelled appointment");

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Cancel(string reason)
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed appointment");
        
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        MarkAsUpdated();
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Can only confirm scheduled appointments");

        Status = AppointmentStatus.Confirmed;
        MarkAsUpdated();
    }

    public void NoShow()
    {
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Can only mark as no-show if scheduled or confirmed");

        Status = AppointmentStatus.NoShow;
        MarkAsUpdated();
    }
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}
