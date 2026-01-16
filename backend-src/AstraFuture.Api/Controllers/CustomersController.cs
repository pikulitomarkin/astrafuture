using AstraFuture.Application.Customers.Commands;
using AstraFuture.Application.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraFuture.Api.Controllers;

/// <summary>
/// API de Customers (clientes)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os clientes do tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = GetTenantId();
        
        var customers = await _mediator.Send(new GetCustomersQuery(tenantId));
        
        var dtos = customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            CustomerType = c.CustomerType,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Busca um cliente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
        
        if (customer == null)
            return NotFound(new { error = "Cliente não encontrado" });

        var dto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            CustomerType = customer.CustomerType,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            
            var command = new CreateCustomerCommand(
                tenantId,
                request.Name,
                request.Email,
                request.Phone,
                request.CustomerType ?? "individual"
            );

            var id = await _mediator.Send(command);
            
            _logger.LogInformation("Customer created: {Id} for tenant {TenantId}", id, tenantId);

            return CreatedAtAction(
                nameof(GetById), 
                new { id }, 
                new { id, message = "Cliente criado com sucesso" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um cliente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var command = new UpdateCustomerCommand(
                id,
                request.Name,
                request.Email,
                request.Phone
            );

            var success = await _mediator.Send(command);
            
            if (!success)
                return NotFound(new { error = "Cliente não encontrado" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove um cliente (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteCustomerCommand(id));
        
        if (!success)
            return NotFound(new { error = "Cliente não encontrado" });

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
public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string CustomerType { get; set; } = "individual";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CustomerType { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
