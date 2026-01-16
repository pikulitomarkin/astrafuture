namespace AstraFuture.Api.Contracts;

/// <summary>
/// Request para criar appointment
/// </summary>
public class CreateAppointmentRequest
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ResourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Location { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = "consultation";
    public string Notes { get; set; } = string.Empty;
}
