using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Infrastructure.Repositories;

namespace AstraFuture.Infrastructure.Persistence;

/// <summary>
/// Implementação do Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SupabaseContext _context;
    private IAppointmentRepository? _appointments;
    private ICustomerRepository? _customers;
    private bool _disposed;

    public UnitOfWork(SupabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IAppointmentRepository Appointments
    {
        get
        {
            _appointments ??= new AppointmentRepository(_context);
            return _appointments;
        }
    }

    public ICustomerRepository Customers
    {
        get
        {
            _customers ??= new CustomerRepository(_context);
            return _customers;
        }
    }

    public async Task SetTenantContextAsync(Guid tenantId)
    {
        await _context.SetTenantContextAsync(tenantId);
    }

    public void BeginTransaction()
    {
        _context.BeginTransaction();
    }

    public Task CommitAsync()
    {
        _context.Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        _context.Rollback();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context?.Dispose();
            _disposed = true;
        }
    }
}
