using MediatR;

namespace AstraFuture.Application.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery : IRequest<AppointmentDto?>
{
    public Guid TenantId { get; init; }
    public Guid Id { get; init; }
}

public record AppointmentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ResourceId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime EndsAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Status { get; init; } = "scheduled";
    public string AppointmentType { get; init; } = "consultation";
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
