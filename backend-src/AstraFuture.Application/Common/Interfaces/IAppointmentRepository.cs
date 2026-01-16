using AstraFuture.Domain.Entities;

namespace AstraFuture.Application.Common.Interfaces;

/// <summary>
/// Repositório de Appointments
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Appointment>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Appointment>> GetByResourceIdAsync(Guid resourceId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid tenantId, DateTime start, DateTime end);
    Task<Guid> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
