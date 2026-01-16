using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AppointmentDto?> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        await _unitOfWork.SetTenantContextAsync(request.TenantId);
        
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id);
        
        if (appointment == null)
            return null;

        return new AppointmentDto
        {
            Id = appointment.Id,
            TenantId = appointment.TenantId,
            CustomerId = appointment.CustomerId,
            ResourceId = appointment.ResourceId,
            Title = appointment.Title,
            Description = appointment.Description,
            ScheduledAt = appointment.ScheduledAt,
            EndsAt = appointment.EndsAt,
            DurationMinutes = appointment.DurationMinutes,
            Status = appointment.Status.ToString(),
            AppointmentType = appointment.AppointmentType,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}
