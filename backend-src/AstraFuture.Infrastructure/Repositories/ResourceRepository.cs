using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using AstraFuture.Infrastructure.Persistence;
using Dapper;

namespace AstraFuture.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Resources usando Dapper
/// </summary>
public class ResourceRepository : IResourceRepository
{
    private readonly SupabaseContext _context;

    // SQL comum para refletir resource_type como INT
    private const string SelectColumns = @"
        id as Id,
        tenant_id as TenantId,
        name as Name,
        description as Description,
        resource_type as Type,
        location as Location,
        '#3B82F6' as Color,
        meta_data as MetaFields,
        is_active as IsActive,
        created_at as CreatedAt,
        updated_at as UpdatedAt";

    public ResourceRepository(SupabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Resource?> GetByIdAsync(Guid id)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM resources
            WHERE id = @Id AND deleted_at IS NULL";

        return await _context.Connection.QuerySingleOrDefaultAsync<Resource>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(Guid tenantId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM resources
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY name ASC";

        return await _context.Connection.QueryAsync<Resource>(sql, new { TenantId = tenantId });
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(Guid tenantId, ResourceType type)
    {
        var typeString = TypeToString(type);
        
        var sql = $@"
            SELECT {SelectColumns}
            FROM resources
            WHERE tenant_id = @TenantId 
              AND resource_type = @Type 
              AND deleted_at IS NULL
            ORDER BY name ASC";

        return await _context.Connection.QueryAsync<Resource>(
            sql, 
            new { TenantId = tenantId, Type = typeString });
    }

    public async Task<IEnumerable<Resource>> GetActiveAsync(Guid tenantId)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM resources
            WHERE tenant_id = @TenantId 
              AND is_active = true 
              AND deleted_at IS NULL
            ORDER BY name ASC";

        return await _context.Connection.QueryAsync<Resource>(sql, new { TenantId = tenantId });
    }

    public async Task<Guid> CreateAsync(Resource resource)
    {
        const string sql = @"
            INSERT INTO resources (
                id, tenant_id, name, description, resource_type,
                location, meta_data, is_active,
                created_at, updated_at
            ) VALUES (
                @Id, @TenantId, @Name, @Description, @Type,
                @Location, @MetaFields::jsonb, @IsActive,
                @CreatedAt, @UpdatedAt
            )
            RETURNING id";

        var parameters = new
        {
            resource.Id,
            resource.TenantId,
            resource.Name,
            resource.Description,
            Type = TypeToString(resource.Type),
            Location = (string?)null,
            MetaFields = System.Text.Json.JsonSerializer.Serialize(resource.MetaFields),
            resource.IsActive,
            resource.CreatedAt,
            resource.UpdatedAt
        };

        return await _context.Connection.ExecuteScalarAsync<Guid>(sql, parameters);
    }

    public async Task UpdateAsync(Resource resource)
    {
        const string sql = @"
            UPDATE resources
            SET 
                name = @Name,
                description = @Description,
                resource_type = @Type,
                meta_data = @MetaFields::jsonb,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";

        var parameters = new
        {
            resource.Id,
            resource.Name,
            resource.Description,
            Type = TypeToString(resource.Type),
            MetaFields = System.Text.Json.JsonSerializer.Serialize(resource.MetaFields),
            resource.IsActive,
            resource.UpdatedAt
        };

        await _context.Connection.ExecuteAsync(sql, parameters);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = @"
            UPDATE resources
            SET deleted_at = @DeletedAt, updated_at = @UpdatedAt
            WHERE id = @Id";

        await _context.Connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            DeletedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM resources 
                WHERE id = @Id AND deleted_at IS NULL
            )";

        return await _context.Connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    private static string TypeToString(ResourceType type) => type switch
    {
        ResourceType.Professional => "professional",
        ResourceType.Room => "room",
        ResourceType.Equipment => "equipment",
        _ => "professional"
    };
}
