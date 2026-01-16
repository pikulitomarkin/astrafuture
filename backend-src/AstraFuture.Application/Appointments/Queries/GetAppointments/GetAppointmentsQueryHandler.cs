using AstraFuture.Application.Appointments.Queries.GetAppointmentById;
using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IEnumerable<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        await _unitOfWork.SetTenantContextAsync(request.TenantId);
        
        IEnumerable<Domain.Entities.Appointment> appointments;

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByDateRangeAsync(
                request.TenantId, 
                request.StartDate.Value, 
                request.EndDate.Value);
        }
        else if (request.CustomerId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByCustomerIdAsync(request.CustomerId.Value);
        }
        else if (request.ResourceId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByResourceIdAsync(request.ResourceId.Value);
        }
        else
        {
            appointments = await _unitOfWork.Appointments.GetAllAsync(request.TenantId);
        }

        return appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            CustomerId = a.CustomerId,
            ResourceId = a.ResourceId,
            Title = a.Title,
            Description = a.Description,
            ScheduledAt = a.ScheduledAt,
            EndsAt = a.EndsAt,
            DurationMinutes = a.DurationMinutes,
            Status = a.Status.ToString(),
            AppointmentType = a.AppointmentType,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        });
    }
}
