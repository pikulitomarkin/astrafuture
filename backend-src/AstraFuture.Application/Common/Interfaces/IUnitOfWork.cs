namespace AstraFuture.Application.Common.Interfaces;

/// <summary>
/// Unit of Work - Coordena múltiplos repositórios e transações
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    ICustomerRepository Customers { get; }
    
    Task SetTenantContextAsync(Guid tenantId);
    void BeginTransaction();
    Task CommitAsync();
    Task RollbackAsync();
}
