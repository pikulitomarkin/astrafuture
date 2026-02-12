using AstraFuture.Api.Contracts;
using AstraFuture.Application.Appointments.Commands.CreateAppointment;
using AstraFuture.Application.Appointments.Commands.UpdateAppointment;
using AstraFuture.Application.Appointments.Commands.DeleteAppointment;
using AstraFuture.Application.Appointments.Queries.GetAppointmentById;
using AstraFuture.Application.Appointments.Queries.GetAppointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstraFuture.Api.Controllers;

/// <summary>
/// Controller de Appointments - CRUD completo com autenticação
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IMediator mediator, ILogger<AppointmentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista todos os appointments do tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? resourceId = null,
        [FromQuery] string? status = null)
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

            _logger.LogInformation("Getting appointments for tenant {TenantId}", tenantId);
            
            var query = new GetAppointmentsQuery
            {
                TenantId = tenantId,
                StartDate = startDate,
                EndDate = endDate,
                CustomerId = customerId,
                ResourceId = resourceId,
                Status = status
            };

            var appointments = await _mediator.Send(query);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointments");
            return StatusCode(500, new { error = "Erro interno ao buscar appointments" });
        }
    }

    /// <summary>
    /// Busca appointment por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
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

            _logger.LogInformation("Getting appointment {Id} for tenant {TenantId}", id, tenantId);

            var query = new GetAppointmentByIdQuery
            {
                Id = id,
                TenantId = tenantId
            };

            var appointment = await _mediator.Send(query);

            if (appointment == null)
            {
                return NotFound(new { error = "Appointment não encontrado" });
            }

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {Id}", id);
            return StatusCode(500, new { error = "Erro interno ao buscar appointment" });
        }
    }

    /// <summary>
    /// Cria um novo appointment
    /// </summary>
    /// <param name="request">Dados do appointment</param>
    /// <returns>ID do appointment criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] System.Text.Json.JsonElement json)
    {
        try
        {
            // Parse manual do JSON - suportando múltiplos formatos
            Guid tenantId;
            Guid customerId;
            Guid? resourceId = null;
            string title = "";
            string description = "";
            DateTime scheduledAt;
            int durationMinutes = 60; // default
            string location = "";
            string appointmentType = "consultation";
            string notes = "";

            // Parse TenantId (obrigatório)
            if (json.TryGetProperty("tenantId", out var tenantIdProp))
            {
                tenantId = tenantIdProp.GetGuid();
            }
            else
            {
                // Extrair do JWT se não fornecido
                var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
                if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out tenantId))
                {
                    _logger.LogWarning("Missing tenant_id in request and JWT");
                    return BadRequest(new { error = "tenant_id is required" });
                }
            }

            // Parse CustomerId (obrigatório)
            if (!json.TryGetProperty("customerId", out var customerIdProp))
            {
                return BadRequest(new { error = "customerId is required" });
            }
            customerId = customerIdProp.GetGuid();

            // Parse ResourceId (opcional)
            if (json.TryGetProperty("resourceId", out var resourceIdProp) && resourceIdProp.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                resourceId = resourceIdProp.GetGuid();
            }

            // Parse Title (opcional)
            if (json.TryGetProperty("title", out var titleProp))
            {
                title = titleProp.GetString() ?? "";
            }

            // Parse Description (opcional)
            if (json.TryGetProperty("description", out var descProp))
            {
                description = descProp.GetString() ?? "";
            }

            // Parse ScheduledAt (obrigatório) - aceita scheduledAt ou startTime
            if (json.TryGetProperty("scheduledAt", out var scheduledAtProp))
            {
                scheduledAt = scheduledAtProp.GetDateTime();
            }
            else if (json.TryGetProperty("startTime", out var startTimeProp))
            {
                scheduledAt = startTimeProp.GetDateTime();
            }
            else
            {
                return BadRequest(new { error = "scheduledAt or startTime is required" });
            }

            // Parse DurationMinutes - aceita durationMinutes direto ou calcula de endsAt/endTime
            if (json.TryGetProperty("durationMinutes", out var durationProp))
            {
                durationMinutes = durationProp.GetInt32();
            }
            else if (json.TryGetProperty("endsAt", out var endsAtProp))
            {
                var endsAt = endsAtProp.GetDateTime();
                durationMinutes = (int)(endsAt - scheduledAt).TotalMinutes;
            }
            else if (json.TryGetProperty("endTime", out var endTimeProp))
            {
                var endTime = endTimeProp.GetDateTime();
                durationMinutes = (int)(endTime - scheduledAt).TotalMinutes;
            }

            // Parse Location (opcional)
            if (json.TryGetProperty("location", out var locProp))
            {
                location = locProp.GetString() ?? "";
            }

            // Parse AppointmentType (opcional)
            if (json.TryGetProperty("appointmentType", out var typeProp))
            {
                appointmentType = typeProp.GetString() ?? "consultation";
            }

            // Parse Notes (opcional)
            if (json.TryGetProperty("notes", out var notesProp))
            {
                notes = notesProp.GetString() ?? "";
            }

            _logger.LogInformation(
                "Creating appointment for tenant {TenantId}, customer {CustomerId}, scheduled at {ScheduledAt}, duration {Duration}min", 
                tenantId, 
                customerId,
                scheduledAt,
                durationMinutes);

            var command = new CreateAppointmentCommand
            {
                TenantId = tenantId,
                CustomerId = customerId,
                ResourceId = resourceId ?? Guid.Empty,
                Title = string.IsNullOrWhiteSpace(title) ? "Agendamento" : title,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                ScheduledAt = scheduledAt,
                DurationMinutes = durationMinutes,
                Location = string.IsNullOrWhiteSpace(location) ? null : location,
                AppointmentType = appointmentType,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
            };

            var appointmentId = await _mediator.Send(command);

            _logger.LogInformation("Appointment {AppointmentId} created successfully", appointmentId);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = appointmentId }, 
                new CreateAppointmentResponse { Id = appointmentId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating appointment");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, new { error = "Erro interno ao criar appointment" });
        }
    }

    /// <summary>
    /// Atualiza um appointment existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] System.Text.Json.JsonElement json)
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

            _logger.LogInformation("Updating appointment {Id} for tenant {TenantId}", id, tenantId);

            // Parse campos opcionais
            string? title = null;
            string? description = null;
            DateTime? scheduledAt = null;
            int? durationMinutes = null;
            string? status = null;
            string? appointmentType = null;

            if (json.TryGetProperty("title", out var titleProp))
            {
                title = titleProp.GetString();
            }

            if (json.TryGetProperty("description", out var descProp))
            {
                description = descProp.GetString();
            }

            // Parse ScheduledAt - aceita scheduledAt ou startTime
            if (json.TryGetProperty("scheduledAt", out var scheduledAtProp))
            {
                scheduledAt = scheduledAtProp.GetDateTime();
            }
            else if (json.TryGetProperty("startTime", out var startTimeProp))
            {
                scheduledAt = startTimeProp.GetDateTime();
            }

            // Parse DurationMinutes - aceita durationMinutes direto ou calcula de endsAt/endTime
            if (json.TryGetProperty("durationMinutes", out var durationProp))
            {
                durationMinutes = durationProp.GetInt32();
            }
            else if (scheduledAt.HasValue)
            {
                if (json.TryGetProperty("endsAt", out var endsAtProp))
                {
                    var endsAt = endsAtProp.GetDateTime();
                    durationMinutes = (int)(endsAt - scheduledAt.Value).TotalMinutes;
                }
                else if (json.TryGetProperty("endTime", out var endTimeProp))
                {
                    var endTime = endTimeProp.GetDateTime();
                    durationMinutes = (int)(endTime - scheduledAt.Value).TotalMinutes;
                }
            }

            if (json.TryGetProperty("status", out var statusProp))
            {
                status = statusProp.GetString();
            }

            if (json.TryGetProperty("appointmentType", out var typeProp))
            {
                appointmentType = typeProp.GetString();
            }

            var command = new UpdateAppointmentCommand
            {
                Id = id,
                TenantId = tenantId,
                Title = title ?? "Agendamento",
                Description = description,
                ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                DurationMinutes = durationMinutes ?? 60,
                Status = status,
                AppointmentType = appointmentType ?? "consultation"
            };

            var updated = await _mediator.Send(command);

            if (!updated)
            {
                return NotFound(new { error = "Appointment não encontrado" });
            }

            _logger.LogInformation("Appointment {Id} updated successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", id);
            return StatusCode(500, new { error = "Erro interno ao atualizar appointment" });
        }
    }

    /// <summary>
    /// Exclui (soft delete) um appointment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
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

            _logger.LogInformation("Deleting appointment {Id} for tenant {TenantId}", id, tenantId);

            var command = new DeleteAppointmentCommand
            {
                Id = id,
                TenantId = tenantId
            };

            var deleted = await _mediator.Send(command);

            if (!deleted)
            {
                return NotFound(new { error = "Appointment não encontrado" });
            }

            _logger.LogInformation("Appointment {Id} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {Id}", id);
            return StatusCode(500, new { error = "Erro interno ao excluir appointment" });
        }
    }
}

public record CreateAppointmentResponse
{
    public Guid Id { get; init; }
}
