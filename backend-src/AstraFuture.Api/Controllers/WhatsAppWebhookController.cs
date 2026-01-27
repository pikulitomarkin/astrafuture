using AstraFuture.Api.Contracts;
using AstraFuture.Domain.Entities;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace AstraFuture.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(IConfiguration configuration, ILogger<WhatsAppWebhookController> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _logger = logger;
    }

    // Webhook público que recebe mensagens do WhatsApp
    [HttpPost("whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWhatsAppMessage(
        [FromBody] WhatsAppWebhookRequest request,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        try
        {
            _logger.LogInformation("WhatsApp webhook received from {Instance}", request.Instance);

            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(new { message = "API Key is required" });
            }

            // Validar API Key
            var key = await ValidateApiKey(apiKey);
            if (key == null)
            {
                return Unauthorized(new { message = "Invalid API Key" });
            }

            // Extrair telefone
            var phoneNumber = ExtractPhoneNumber(request.Data.Key.RemoteJid);
            var messageText = request.Data.Message.Conversation 
                ?? request.Data.Message.ExtendedTextMessage 
                ?? string.Empty;

            _logger.LogInformation("Message from {Phone}: {Message}", phoneNumber, messageText);

            // Registrar lead se não existir
            await RegisterLeadIfNew(phoneNumber, request.Data.PushName, key.TenantId);

            // Atualizar uso da API Key
            await UpdateApiKeyUsage(apiKey);

            return Ok(new { 
                success = true, 
                phoneNumber,
                message = "Webhook received successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp webhook");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Endpoint público para criar customer via WhatsApp
    [HttpPost("customers")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCustomerFromWhatsApp(
        [FromBody] WhatsAppCreateCustomerRequest request,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(new { message = "API Key is required" });
            }

            var key = await ValidateApiKey(apiKey);
            if (key == null)
            {
                return Unauthorized(new { message = "Invalid API Key" });
            }

            // Criar customer usando SQL direto
            var customerId = Guid.NewGuid();
            var tenantId = key.TenantId;
            
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(
                @"INSERT INTO customers (id, name, email, phone, tenant_id, created_at, updated_at, is_active, customer_type)
                  VALUES (@Id, @Name, @Email, @Phone, @TenantId, @CreatedAt, @UpdatedAt, @IsActive, @CustomerType)",
                new {
                    Id = customerId,
                    Name = request.Name,
                    Email = request.Email ?? $"{request.PhoneNumber}@whatsapp.temp",
                    Phone = request.PhoneNumber,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CustomerType = "individual"
                });

            // Converter lead em customer
            await ConvertLeadToCustomer(request.PhoneNumber, customerId.ToString(), key.TenantId);

            await UpdateApiKeyUsage(apiKey);

            return Ok(new { 
                success = true, 
                customerId = customerId.ToString(),
                message = "Customer created successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer from WhatsApp");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Endpoint público para criar agendamento via WhatsApp
    [HttpPost("appointments")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAppointmentFromWhatsApp(
        [FromBody] WhatsAppCreateAppointmentRequest request,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(new { message = "API Key is required" });
            }

            var key = await ValidateApiKey(apiKey);
            if (key == null)
            {
                return Unauthorized(new { message = "Invalid API Key" });
            }

            // Buscar customer pelo telefone
            await using var connection = new NpgsqlConnection(_connectionString);
            var customer = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, name, email, phone, tenant_id FROM customers WHERE phone = @Phone AND tenant_id = @TenantId",
                new { Phone = request.CustomerPhone, key.TenantId });

            if (customer == null)
            {
                return NotFound(new { message = "Customer not found with this phone number" });
            }

            var customerId = Guid.Parse(customer.id.ToString());
            var resourceId = string.IsNullOrEmpty(request.ResourceId) ? Guid.Empty : Guid.Parse(request.ResourceId);

            // Criar appointment usando SQL direto
            var appointmentId = Guid.NewGuid();
            var tenantGuid = Guid.Parse(key.TenantId);
            
            await connection.ExecuteAsync(
                @"INSERT INTO appointments (id, customer_id, resource_id, scheduled_at, ends_at, duration_minutes, status, notes, tenant_id, created_at, updated_at, title)
                  VALUES (@Id, @CustomerId, @ResourceId, @ScheduledAt, @EndsAt, @DurationMinutes, @Status, @Notes, @TenantId, @CreatedAt, @UpdatedAt, @Title)",
                new { 
                    Id = appointmentId,
                    CustomerId = customerId,
                    ResourceId = resourceId == Guid.Empty ? (Guid?)null : resourceId,
                    ScheduledAt = request.StartTime,
                    EndsAt = request.EndTime,
                    DurationMinutes = (int)(request.EndTime - request.StartTime).TotalMinutes,
                    Status = AppointmentStatus.Scheduled,
                    Notes = request.Notes,
                    TenantId = tenantGuid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Title = "Agendamento via WhatsApp"
                });

            await UpdateApiKeyUsage(apiKey);

            return Ok(new { 
                success = true, 
                appointmentId = appointmentId.ToString(),
                startTime = request.StartTime,
                endTime = request.EndTime,
                message = "Appointment created successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment from WhatsApp");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Verificar se cliente existe pelo telefone
    [HttpGet("customers/check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckCustomerExists(
        [FromQuery] string phone,
        [FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(new { message = "API Key is required" });
            }

            var key = await ValidateApiKey(apiKey);
            if (key == null)
            {
                return Unauthorized(new { message = "Invalid API Key" });
            }

            await using var connection = new NpgsqlConnection(_connectionString);
            var customerData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT id, name, email, phone FROM customers WHERE phone = @Phone AND tenant_id = @TenantId",
                new { Phone = phone, key.TenantId });

            await UpdateApiKeyUsage(apiKey);

            return Ok(new { 
                exists = customerData != null,
                customer = customerData != null ? new {
                    Id = customerData.id.ToString(),
                    Name = customerData.name,
                    Email = customerData.email,
                    Phone = customerData.phone
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    // Métodos auxiliares privados
    private async Task<ApiKey?> ValidateApiKey(string key)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var apiKey = await connection.QueryFirstOrDefaultAsync<ApiKey>(
            @"SELECT * FROM api_keys 
              WHERE key = @Key 
              AND is_active = true 
              AND expires_at > @Now",
            new { Key = key, Now = DateTime.UtcNow });

        return apiKey;
    }

    private async Task UpdateApiKeyUsage(string key)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE api_keys 
              SET usage_count = usage_count + 1, 
                  last_used_at = @Now 
              WHERE key = @Key",
            new { Key = key, Now = DateTime.UtcNow });
    }

    private async Task RegisterLeadIfNew(string phoneNumber, string name, string tenantId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        var exists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM whatsapp_leads WHERE phone_number = @Phone AND tenant_id = @TenantId)",
            new { Phone = phoneNumber, TenantId = tenantId });

        if (!exists)
        {
            await connection.ExecuteAsync(
                @"INSERT INTO whatsapp_leads (id, phone_number, name, tenant_id, status, source, created_at, updated_at)
                  VALUES (@Id, @PhoneNumber, @Name, @TenantId, @Status, @Source, @CreatedAt, @UpdatedAt)",
                new {
                    Id = Guid.NewGuid(),
                    PhoneNumber = phoneNumber,
                    Name = name,
                    TenantId = Guid.Parse(tenantId),
                    Status = (int)LeadStatus.New,
                    Source = (int)LeadSource.WhatsApp,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        }
    }

    private async Task ConvertLeadToCustomer(string phoneNumber, string customerId, string tenantId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE whatsapp_leads 
              SET customer_id = @CustomerId, 
                  status = @Status, 
                  converted_at = @Now 
              WHERE phone_number = @Phone 
              AND tenant_id = @TenantId",
            new { CustomerId = customerId, Status = (int)LeadStatus.Converted, Now = DateTime.UtcNow, Phone = phoneNumber, TenantId = tenantId });
    }

    private static string ExtractPhoneNumber(string remoteJid)
    {
        // remoteJid vem no formato: 5511987654321@s.whatsapp.net
        return remoteJid.Split('@')[0];
    }
}
