using AstraFuture.Domain.Entities;

namespace AstraFuture.Application.Common.Interfaces;

/// <summary>
/// Repositório de Customers
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId);
    Task<Customer?> GetByPhoneAsync(Guid tenantId, string phone);
    Task<Customer?> GetByEmailAsync(Guid tenantId, string email);
    Task<Guid> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
