using AstraFuture.Application.Resources.Commands;
using AstraFuture.Application.Resources.Queries;
using AstraFuture.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraFuture.Api.Controllers;

/// <summary>
/// API de Resources (profissionais, salas, equipamentos)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(IMediator mediator, ILogger<ResourcesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os recursos do tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ResourceType? type = null,
        [FromQuery] bool? activeOnly = null)
    {
        var tenantId = GetTenantId();
        
        var resources = await _mediator.Send(new GetResourcesQuery(tenantId, type, activeOnly));
        
        var dtos = resources.Select(r => new ResourceDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Type = r.Type,
            TypeName = r.Type.ToString(),
            Email = r.Email,
            Phone = r.Phone,
            Color = r.Color,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Busca um recurso por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resource = await _mediator.Send(new GetResourceByIdQuery(id));
        
        if (resource == null)
            return NotFound(new { error = "Recurso não encontrado" });

        var dto = new ResourceDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Type = resource.Type,
            TypeName = resource.Type.ToString(),
            Email = resource.Email,
            Phone = resource.Phone,
            Color = resource.Color,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Cria um novo recurso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            
            var command = new CreateResourceCommand(
                tenantId,
                request.Name,
                request.Type,
                request.Description,
                request.Email,
                request.Phone,
                request.Color ?? "#3B82F6"
            );

            var id = await _mediator.Send(command);
            
            _logger.LogInformation("Resource created: {Id} for tenant {TenantId}", id, tenantId);

            return CreatedAtAction(
                nameof(GetById), 
                new { id }, 
                new { id, message = "Recurso criado com sucesso" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um recurso
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceRequest request)
    {
        try
        {
            var command = new UpdateResourceCommand(
                id,
                request.Name,
                request.Description,
                request.Email,
                request.Phone,
                request.Color
            );

            var success = await _mediator.Send(command);
            
            if (!success)
                return NotFound(new { error = "Recurso não encontrado" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um recurso (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteResourceCommand(id));
        
        if (!success)
            return NotFound(new { error = "Recurso não encontrado" });

        return NoContent();
    }

    private Guid GetTenantId()
    {
        // Primeiro tenta pegar do header X-Tenant-Id
        if (Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) 
            && Guid.TryParse(headerValue, out var headerTenantId))
        {
            return headerTenantId;
        }

        // Depois tenta pegar do JWT claim
        var claim = User.FindFirst("tenant_id") ?? User.FindFirst("user_metadata.tenant_id");
        if (claim != null && Guid.TryParse(claim.Value, out var claimTenantId))
        {
            return claimTenantId;
        }

        // Default tenant para desenvolvimento
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}

// DTOs
public class ResourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Color { get; set; }
}

public class UpdateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Color { get; set; }
}
