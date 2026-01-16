using MediatR;

namespace AstraFuture.Application.Appointments.Commands.CreateAppointment;

/// <summary>
/// Comando para criar um novo appointment
/// </summary>
public record CreateAppointmentCommand : IRequest<Guid>
{
    public Guid TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ResourceId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string? Location { get; init; }
    public string AppointmentType { get; init; } = "consultation";
    public string? Notes { get; init; }
}
