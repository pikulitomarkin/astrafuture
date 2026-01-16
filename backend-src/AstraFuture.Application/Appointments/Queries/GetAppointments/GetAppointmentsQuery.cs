using AstraFuture.Application.Appointments.Queries.GetAppointmentById;
using MediatR;

namespace AstraFuture.Application.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery : IRequest<IEnumerable<AppointmentDto>>
{
    public Guid TenantId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ResourceId { get; init; }
    public string? Status { get; init; }
}
