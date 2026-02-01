using AstraFuture.Domain.Entities;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace AstraFuture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiKeysController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<ApiKeysController> _logger;

    public ApiKeysController(IConfiguration configuration, ILogger<ApiKeysController> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    // Listar todas as API Keys do tenant
    [HttpGet]
    public async Task<IActionResult> GetApiKeys()
    {
        try
        {
            var tenantId = User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized(new { message = "Tenant ID not found in token" });
            }

            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                _logger.LogWarning("Invalid tenant_id claim: {TenantId}", tenantId);
                return BadRequest(new { message = "Invalid tenant_id in token" });
            }

            try
            {
                // Conectando com usuário que deve ter BYPASSRLS para evitar erro no handshake
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Definir o tenant context APÓS conectar (mais seguro e compatível com poolers)
                await connection.ExecuteAsync($"SET LOCAL app.tenant_id = '{tenantGuid}'");

                _logger.LogInformation("Fetching API keys for tenant {TenantId}", tenantGuid);

                // Como temos BYPASSRLS, precisamos garantir que o tenant existe manualmente se quisermos
                // Mas o RLS para este usuário (se reativado) ou filtros manuais devem cuidar disso
                
                var apiKeys = await connection.QueryAsync<ApiKey>(
                    "SELECT * FROM api_keys WHERE tenant_id = @TenantId ORDER BY created_at DESC",
                    new { TenantId = tenantGuid });

                // Retornar com keys mascaradas (mostrar apenas últimos 8 caracteres)
                return Ok(apiKeys.Select(k => new
                {
                    k.Id,
                    Key = MaskApiKey(k.Key),
                    k.Name,
                    k.Description,
                    k.IsActive,
                    k.LastUsedAt,
                    k.ExpiresAt,
                    k.UsageCount,
                    k.RateLimit,
                    k.CreatedAt
                }));
            }
            catch (Npgsql.PostgresException pgEx)
            {
                _logger.LogError(pgEx, "Postgres error while fetching API keys for tenant {TenantId}", tenantGuid);
                return StatusCode(502, new { message = "Database error. Check connection/credentials and that migrations were applied." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching API keys for tenant {TenantId}", tenantGuid);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API keys");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Criar nova API Key
    [HttpPost]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var tenantId = User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized(new { message = "Tenant ID not found in token" });
            }

            var apiKey = new ApiKey
            {
                Key = GenerateApiKey(),
                Name = request.Name,
                Description = request.Description,
                TenantId = tenantId,
                IsActive = true,
                ExpiresAt = request.ExpiresInDays.HasValue 
                    ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value) 
                    : DateTime.UtcNow.AddYears(10), // Padrão: 10 anos
                RateLimit = request.RateLimit,
                UsageCount = 0
            };

            var apiKeyId = Guid.NewGuid();
            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                _logger.LogWarning("Invalid tenant_id claim on create: {TenantId}", tenantId);
                return BadRequest(new { message = "Invalid tenant_id in token" });
            }

            try
            {
                // Conectando com usuário admin (BYPASSRLS)
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Definir tenant context manualmente
                await connection.ExecuteAsync($"SET LOCAL app.tenant_id = '{tenantGuid}'");

                // Verificar existência de tenant
                var tenantExists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM tenants WHERE id = @TenantId)",
                    new { TenantId = tenantGuid });

                if (!tenantExists)
                {
                    _logger.LogWarning("Attempt to create API key for non-existing tenant: {TenantId}", tenantGuid);
                    return NotFound(new { message = "Tenant not found" });
                }

                await connection.ExecuteAsync(
                    @"INSERT INTO api_keys (id, key, name, description, tenant_id, is_active, expires_at, rate_limit, usage_count, created_at, updated_at)
                      VALUES (@Id, @Key, @Name, @Description, @TenantId, @IsActive, @ExpiresAt, @RateLimit, @UsageCount, @CreatedAt, @UpdatedAt)",
                    new {
                        Id = apiKeyId,
                        apiKey.Key,
                        apiKey.Name,
                        apiKey.Description,
                        TenantId = tenantGuid,
                        apiKey.IsActive,
                        apiKey.ExpiresAt,
                        apiKey.RateLimit,
                        apiKey.UsageCount,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
            }
            catch (Npgsql.PostgresException pgEx)
            {
                _logger.LogError(pgEx, "Postgres error while creating API key for tenant {TenantId}", tenantGuid);
                return StatusCode(502, new { message = "Database error. Check connection/credentials and that migrations were applied." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating API key for tenant {TenantId}", tenantGuid);
                return StatusCode(500, new { message = "Internal server error" });
            }

            // Retornar a key completa apenas na criação (única vez)
            return Ok(new
            {
                Id = apiKeyId.ToString(),
                Key = apiKey.Key, // Mostrar completa apenas agora
                apiKey.Name,
                apiKey.Description,
                apiKey.ExpiresAt,
                Message = "⚠️ ATENÇÃO: Copie esta chave agora! Ela não será mostrada novamente."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Atualizar API Key (ativar/desativar, alterar nome)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApiKey(string id, [FromBody] UpdateApiKeyRequest request)
    {
        try
        {
            var tenantId = User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized(new { message = "Tenant ID not found in token" });
            }

            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                _logger.LogWarning("Invalid tenant_id claim on update: {TenantId}", tenantId);
                return BadRequest(new { message = "Invalid tenant_id in token" });
            }

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Definir tenant context
            await connection.ExecuteAsync($"SET LOCAL app.tenant_id = '{tenantGuid}'");

            // Verificar existência de tenant antes de qualquer operação
            var tenantExists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM tenants WHERE id = @TenantId)",
                new { TenantId = tenantGuid });

            if (!tenantExists)
            {
                _logger.LogWarning("Attempt to update API key for non-existing tenant: {TenantId}", tenantGuid);
                return NotFound(new { message = "Tenant not found" });
            }
            
            // Verificar se a key pertence ao tenant
            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM api_keys WHERE id = @Id AND tenant_id = @TenantId)",
                new { Id = id, TenantId = tenantGuid });

            if (!exists)
            {
                return NotFound(new { message = "API Key not found" });
            }

            await connection.ExecuteAsync(
                @"UPDATE api_keys 
                  SET name = @Name, 
                      description = @Description, 
                      is_active = @IsActive, 
                      updated_at = @Now 
                  WHERE id = @Id",
                new { 
                    Id = id, 
                    request.Name, 
                    request.Description, 
                    request.IsActive, 
                    Now = DateTime.UtcNow 
                });

            return Ok(new { message = "API Key updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API key");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Deletar API Key
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiKey(string id)
    {
        try
        {
            var tenantId = User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantId))
            {
                return Unauthorized(new { message = "Tenant ID not found in token" });
            }

            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                _logger.LogWarning("Invalid tenant_id claim on delete: {TenantId}", tenantId);
                return BadRequest(new { message = "Invalid tenant_id in token" });
            }

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Definir tenant context
            await connection.ExecuteAsync($"SET LOCAL app.tenant_id = '{tenantGuid}'");

            // Verificar existência de tenant
            var tenantExists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM tenants WHERE id = @TenantId)",
                new { TenantId = tenantGuid });

            if (!tenantExists)
            {
                _logger.LogWarning("Attempt to delete API key for non-existing tenant: {TenantId}", tenantGuid);
                return NotFound(new { message = "Tenant not found" });
            }
            
            var deleted = await connection.ExecuteAsync(
                "DELETE FROM api_keys WHERE id = @Id AND tenant_id = @TenantId",
                new { Id = id, TenantId = tenantGuid });

            if (deleted == 0)
            {
                return NotFound(new { message = "API Key not found" });
            }

            return Ok(new { message = "API Key deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API key");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Obter URL do webhook configurada
    [HttpGet("webhook-url")]
    public IActionResult GetWebhookUrl()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        return Ok(new
        {
            WebhookUrl = $"{baseUrl}/api/webhook/whatsapp",
            CreateCustomerUrl = $"{baseUrl}/api/webhook/customers",
            CreateAppointmentUrl = $"{baseUrl}/api/webhook/appointments",
            CheckCustomerUrl = $"{baseUrl}/api/webhook/customers/check",
            Instructions = "Use o header 'X-API-Key' com sua chave em todas as requisições"
        });
    }

    // Métodos auxiliares
    private static string GenerateApiKey()
    {
        // Gerar uma key segura: astrafuture_live_[40 hex chars]
        var bytes = new byte[20]; // 20 bytes => 40 hex characters
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var randomPart = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        
        return $"astrafuture_live_{randomPart}";
    }

    private static string MaskApiKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length <= 12)
            return "****";
        
        return $"****{key.Substring(key.Length - 8)}";
    }
}

public record CreateApiKeyRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ExpiresInDays { get; init; }
    public int? RateLimit { get; init; }
}

public record UpdateApiKeyRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
