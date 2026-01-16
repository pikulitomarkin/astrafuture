namespace AstraFuture.Api.Contracts;

public record UpdateAppointmentRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Status { get; init; }
    public string AppointmentType { get; init; } = "consultation";
}
