using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using AstraFuture.Infrastructure.Persistence;
using Dapper;

namespace AstraFuture.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Appointments usando Dapper
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly SupabaseContext _context;

    public AppointmentRepository(SupabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id, tenant_id as TenantId, customer_id as CustomerId, resource_id as ResourceId, 
                title, description, scheduled_at as ScheduledAt, ends_at as EndsAt, 
                duration_minutes as DurationMinutes, status, appointment_type as AppointmentType, 
                cancellation_reason as CancellationReason, created_at as CreatedAt, 
                updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM appointments
            WHERE id = @Id AND deleted_at IS NULL";

        return await _context.Connection.QuerySingleOrDefaultAsync<Appointment>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync(Guid tenantId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id as TenantId, customer_id as CustomerId, resource_id as ResourceId, 
                title, description, scheduled_at as ScheduledAt, ends_at as EndsAt, 
                duration_minutes as DurationMinutes, status, appointment_type as AppointmentType, 
                cancellation_reason as CancellationReason, created_at as CreatedAt, 
                updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM appointments
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY scheduled_at DESC";

        return await _context.Connection.QueryAsync<Appointment>(sql, new { TenantId = tenantId });
    }

    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id as TenantId, customer_id as CustomerId, resource_id as ResourceId, 
                title, description, scheduled_at as ScheduledAt, ends_at as EndsAt, 
                duration_minutes as DurationMinutes, status, appointment_type as AppointmentType, 
                cancellation_reason as CancellationReason, created_at as CreatedAt, 
                updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM appointments
            WHERE customer_id = @CustomerId AND deleted_at IS NULL
            ORDER BY scheduled_at DESC";

        return await _context.Connection.QueryAsync<Appointment>(sql, new { CustomerId = customerId });
    }

    public async Task<IEnumerable<Appointment>> GetByResourceIdAsync(Guid resourceId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id as TenantId, customer_id as CustomerId, resource_id as ResourceId, 
                title, description, scheduled_at as ScheduledAt, ends_at as EndsAt, 
                duration_minutes as DurationMinutes, status, appointment_type as AppointmentType, 
                cancellation_reason as CancellationReason, created_at as CreatedAt, 
                updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM appointments
            WHERE resource_id = @ResourceId AND deleted_at IS NULL
            ORDER BY scheduled_at DESC";

        return await _context.Connection.QueryAsync<Appointment>(sql, new { ResourceId = resourceId });
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid tenantId, DateTime start, DateTime end)
    {
        const string sql = @"
            SELECT 
                id, tenant_id as TenantId, customer_id as CustomerId, resource_id as ResourceId, 
                title, description, scheduled_at as ScheduledAt, ends_at as EndsAt, 
                duration_minutes as DurationMinutes, status, appointment_type as AppointmentType, 
                cancellation_reason as CancellationReason, created_at as CreatedAt, 
                updated_at as UpdatedAt, deleted_at as DeletedAt
            FROM appointments
            WHERE tenant_id = @TenantId 
              AND deleted_at IS NULL
              AND scheduled_at >= @Start 
              AND scheduled_at <= @End
            ORDER BY scheduled_at ASC";

        return await _context.Connection.QueryAsync<Appointment>(
            sql, 
            new { TenantId = tenantId, Start = start, End = end });
    }

    public async Task<Guid> CreateAsync(Appointment appointment)
    {
        const string sql = @"
            INSERT INTO appointments (
                id, tenant_id, customer_id, resource_id, title, description,
                scheduled_at, ends_at, duration_minutes, status,
                appointment_type, created_at, updated_at
            ) VALUES (
                @Id, @TenantId, @CustomerId, @ResourceId, @Title, @Description,
                @ScheduledAt, @EndsAt, @DurationMinutes, @Status,
                @AppointmentType, @CreatedAt, @UpdatedAt
            )
            RETURNING id";

        var id = await _context.Connection.ExecuteScalarAsync<Guid>(sql, appointment);
        return id;
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        const string sql = @"
            UPDATE appointments
            SET 
                customer_id = @CustomerId,
                resource_id = @ResourceId,
                title = @Title,
                description = @Description,
                scheduled_at = @ScheduledAt,
                ends_at = @EndsAt,
                duration_minutes = @DurationMinutes,
                status = @Status,
                appointment_type = @AppointmentType,
                cancellation_reason = @CancellationReason,
                updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";

        await _context.Connection.ExecuteAsync(sql, appointment);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = @"
            UPDATE appointments
            SET deleted_at = @DeletedAt, updated_at = @DeletedAt
            WHERE id = @Id AND deleted_at IS NULL";

        await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM appointments
            WHERE id = @Id AND deleted_at IS NULL";

        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
