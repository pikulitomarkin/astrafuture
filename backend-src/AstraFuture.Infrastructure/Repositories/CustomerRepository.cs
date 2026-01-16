using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using AstraFuture.Infrastructure.Persistence;
using Dapper;

namespace AstraFuture.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Customers usando Dapper
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly SupabaseContext _context;

    public CustomerRepository(SupabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, full_name, email, phone, birth_date, document_number,
                address, lead_source, referred_by, meta_data, accepts_marketing,
                tags, is_active, created_at, updated_at, deleted_at
            FROM customers
            WHERE id = @Id AND deleted_at IS NULL";

        return await _context.Connection.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, full_name, email, phone, birth_date, document_number,
                address, lead_source, referred_by, meta_data, accepts_marketing,
                tags, is_active, created_at, updated_at, deleted_at
            FROM customers
            WHERE tenant_id = @TenantId AND deleted_at IS NULL
            ORDER BY full_name ASC";

        return await _context.Connection.QueryAsync<Customer>(sql, new { TenantId = tenantId });
    }

    public async Task<Customer?> GetByPhoneAsync(Guid tenantId, string phone)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, full_name, email, phone, birth_date, document_number,
                address, lead_source, referred_by, meta_data, accepts_marketing,
                tags, is_active, created_at, updated_at, deleted_at
            FROM customers
            WHERE tenant_id = @TenantId AND phone = @Phone AND deleted_at IS NULL";

        return await _context.Connection.QuerySingleOrDefaultAsync<Customer>(
            sql, 
            new { TenantId = tenantId, Phone = phone });
    }

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, full_name, email, phone, birth_date, document_number,
                address, lead_source, referred_by, meta_data, accepts_marketing,
                tags, is_active, created_at, updated_at, deleted_at
            FROM customers
            WHERE tenant_id = @TenantId AND email = @Email AND deleted_at IS NULL";

        return await _context.Connection.QuerySingleOrDefaultAsync<Customer>(
            sql, 
            new { TenantId = tenantId, Email = email });
    }

    public async Task<Guid> CreateAsync(Customer customer)
    {
        const string sql = @"
            INSERT INTO customers (
                id, tenant_id, full_name, email, phone, birth_date, document_number,
                address, lead_source, referred_by, meta_data, accepts_marketing,
                tags, is_active, created_at, updated_at
            ) VALUES (
                @Id, @TenantId, @FullName, @Email, @Phone, @BirthDate, @DocumentNumber,
                @Address::jsonb, @LeadSource, @ReferredBy, @MetaData::jsonb, @AcceptsMarketing,
                @Tags, @IsActive, @CreatedAt, @UpdatedAt
            )
            RETURNING id";

        var id = await _context.Connection.ExecuteScalarAsync<Guid>(sql, customer);
        return id;
    }

    public async Task UpdateAsync(Customer customer)
    {
        const string sql = @"
            UPDATE customers
            SET 
                full_name = @FullName,
                email = @Email,
                phone = @Phone,
                birth_date = @BirthDate,
                document_number = @DocumentNumber,
                address = @Address::jsonb,
                lead_source = @LeadSource,
                referred_by = @ReferredBy,
                meta_data = @MetaData::jsonb,
                accepts_marketing = @AcceptsMarketing,
                tags = @Tags,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND deleted_at IS NULL";

        await _context.Connection.ExecuteAsync(sql, customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = @"
            UPDATE customers
            SET deleted_at = @DeletedAt, updated_at = @DeletedAt
            WHERE id = @Id AND deleted_at IS NULL";

        await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM customers
            WHERE id = @Id AND deleted_at IS NULL";

        var count = await _context.Connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
