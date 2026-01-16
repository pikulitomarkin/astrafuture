using MediatR;

namespace AstraFuture.Application.Appointments.Commands.DeleteAppointment;

public record DeleteAppointmentCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
}
