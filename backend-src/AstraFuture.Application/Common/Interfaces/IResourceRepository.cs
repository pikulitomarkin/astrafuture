using AstraFuture.Domain.Entities;

namespace AstraFuture.Application.Common.Interfaces;

/// <summary>
/// Repositório de Resources (profissionais, salas, equipamentos)
/// </summary>
public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id);
    Task<IEnumerable<Resource>> GetAllAsync(Guid tenantId);
    Task<IEnumerable<Resource>> GetByTypeAsync(Guid tenantId, ResourceType type);
    Task<IEnumerable<Resource>> GetActiveAsync(Guid tenantId);
    Task<Guid> CreateAsync(Resource resource);
    Task UpdateAsync(Resource resource);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
