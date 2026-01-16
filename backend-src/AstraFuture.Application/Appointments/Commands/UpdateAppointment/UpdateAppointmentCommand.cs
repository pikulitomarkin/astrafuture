using MediatR;

namespace AstraFuture.Application.Appointments.Commands.UpdateAppointment;

public record UpdateAppointmentCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Status { get; init; }
    public string AppointmentType { get; init; } = "consultation";
}
