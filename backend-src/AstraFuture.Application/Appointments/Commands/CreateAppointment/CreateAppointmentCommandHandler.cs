using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Appointments.Commands.CreateAppointment;

/// <summary>
/// Handler para criar um novo appointment
/// </summary>
public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Guid> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Define contexto do tenant para RLS
        await _unitOfWork.SetTenantContextAsync(request.TenantId);

        // Valida se customer existe
        var customerExists = await _unitOfWork.Customers.ExistsAsync(request.CustomerId);
        if (!customerExists)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} não encontrado");
        }

        // Calcula ends_at
        var endsAt = request.ScheduledAt.AddMinutes(request.DurationMinutes);

        // Cria entidade via factory method
        var appointment = Appointment.Create(
            tenantId: request.TenantId,
            customerId: request.CustomerId,
            resourceId: request.ResourceId,
            title: request.Title,
            scheduledAt: request.ScheduledAt,
            durationMinutes: request.DurationMinutes,
            appointmentType: request.AppointmentType
        );

        // Atualiza campos opcionais
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            typeof(Appointment)
                .GetProperty(nameof(Appointment.Description))!
                .SetValue(appointment, request.Description);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            typeof(Appointment)
                .GetProperty(nameof(Appointment.Notes))!
                .SetValue(appointment, request.Notes);
        }

        // Persiste no database
        var id = await _unitOfWork.Appointments.CreateAsync(appointment);

        // Commit da transação
        await _unitOfWork.CommitAsync();

        return id;
    }
}
