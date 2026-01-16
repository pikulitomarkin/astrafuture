using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Appointments.Commands.DeleteAppointment;

public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAppointmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.SetTenantContextAsync(request.TenantId);
        
        var exists = await _unitOfWork.Appointments.ExistsAsync(request.Id);
        
        if (!exists)
            return false;

        // Soft delete
        await _unitOfWork.Appointments.DeleteAsync(request.Id);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
