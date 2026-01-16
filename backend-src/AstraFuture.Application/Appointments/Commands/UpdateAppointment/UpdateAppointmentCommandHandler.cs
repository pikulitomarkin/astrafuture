using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.SetTenantContextAsync(request.TenantId);
        
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
        
        if (appointment == null)
            return false;

        // Usa os métodos de domínio para atualizar
        appointment.UpdateDetails(request.Title, request.Description);
        appointment.Reschedule(request.ScheduledAt, request.DurationMinutes);

        await _unitOfWork.Appointments.UpdateAsync(appointment);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
