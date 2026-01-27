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

            await using var connection = new NpgsqlConnection(_connectionString);
            var apiKeys = await connection.QueryAsync<ApiKey>(
                "SELECT * FROM api_keys WHERE tenant_id = @TenantId ORDER BY created_at DESC",
                new { TenantId = tenantId });

            // Mascarar as keys na resposta (mostrar apenas últimos 8 caracteres)
            var maskedKeys = apiKeys.Select(k => new
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
            });

            return Ok(maskedKeys);
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
            var tenantGuid = Guid.Parse(tenantId);

            await using var connection = new NpgsqlConnection(_connectionString);
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

            await using var connection = new NpgsqlConnection(_connectionString);
            
            // Verificar se a key pertence ao tenant
            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM api_keys WHERE id = @Id AND tenant_id = @TenantId)",
                new { Id = id, TenantId = tenantId });

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

            await using var connection = new NpgsqlConnection(_connectionString);
            
            var deleted = await connection.ExecuteAsync(
                "DELETE FROM api_keys WHERE id = @Id AND tenant_id = @TenantId",
                new { Id = id, TenantId = tenantId });

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
        // Gerar uma key segura: astrafuture_live_[40 caracteres aleatórios]
        var bytes = new byte[30];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var randomPart = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 40);
        
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
