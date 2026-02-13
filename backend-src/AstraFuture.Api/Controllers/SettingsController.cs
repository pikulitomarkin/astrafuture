using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraFuture.Api.Controllers;

/// <summary>
/// Controller de Settings - Gerencia configurações do tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ILogger<SettingsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Busca settings do tenant atual
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TenantSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            // Extrair tenant_id do JWT
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogWarning("Invalid or missing tenant_id in JWT");
                return BadRequest(new { error = "Invalid tenant_id in token" });
            }

            _logger.LogInformation("Getting settings for tenant {TenantId}", tenantId);

            // TODO: Implementar query quando tiver IMediator
            // Por enquanto, retornar dados mockados baseados no JWT
            var businessName = User.FindFirst("business_name")?.Value ?? "Minha Empresa";
            
            var settings = new TenantSettingsDto
            {
                Name = businessName,
                LogoUrl = null, // TODO: buscar do banco
                PrimaryColor = "#3B82F6",
                Timezone = "America/Sao_Paulo",
                Locale = "pt-BR"
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant settings");
            return StatusCode(500, new { error = "Erro interno ao buscar configurações" });
        }
    }

    /// <summary>
    /// Atualiza settings do tenant
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateTenantSettingsRequest request)
    {
        try
        {
            // Extrair tenant_id do JWT
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogWarning("Invalid or missing tenant_id in JWT");
                return BadRequest(new { error = "Invalid tenant_id in token" });
            }

            _logger.LogInformation("Updating settings for tenant {TenantId}", tenantId);

            // TODO: Implementar command quando tiver IMediator
            // Por enquanto, apenas logar

            return Ok(new { message = "Settings atualizados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant settings");
            return StatusCode(500, new { error = "Erro interno ao atualizar configurações" });
        }
    }
}

public record TenantSettingsDto
{
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string PrimaryColor { get; init; } = "#3B82F6";
    public string Timezone { get; init; } = "America/Sao_Paulo";
    public string Locale { get; init; } = "pt-BR";
}

public record UpdateTenantSettingsRequest
{
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
}
