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
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? resourceId = null,
        [FromQuery] string? status = null)
    {
        try
        {
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
    public async Task<IActionResult> GetById(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        try
        {
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
            // Parse manual do JSON
            var request = new CreateAppointmentRequest
            {
                TenantId = json.GetProperty("tenantId").GetGuid(),
                CustomerId = json.GetProperty("customerId").GetGuid(),
                ResourceId = json.GetProperty("resourceId").GetGuid(),
                Title = json.GetProperty("title").GetString() ?? "",
                Description = json.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                ScheduledAt = json.GetProperty("scheduledAt").GetDateTime(),
                DurationMinutes = json.GetProperty("durationMinutes").GetInt32(),
                Location = json.TryGetProperty("location", out var loc) ? loc.GetString() ?? "" : "",
                AppointmentType = json.GetProperty("appointmentType").GetString() ?? "consultation",
                Notes = json.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : ""
            };
            
            _logger.LogInformation(
                "Creating appointment for tenant {TenantId}, customer {CustomerId}", 
                request.TenantId, 
                request.CustomerId);

            var command = new CreateAppointmentCommand
            {
                TenantId = request.TenantId,
                CustomerId = request.CustomerId,
                ResourceId = request.ResourceId,
                Title = request.Title,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                ScheduledAt = request.ScheduledAt,
                DurationMinutes = request.DurationMinutes,
                Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location,
                AppointmentType = request.AppointmentType,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes
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
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId,
        [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            _logger.LogInformation("Updating appointment {Id} for tenant {TenantId}", id, tenantId);

            var command = new UpdateAppointmentCommand
            {
                Id = id,
                TenantId = tenantId,
                Title = request.Title,
                Description = request.Description,
                ScheduledAt = request.ScheduledAt,
                DurationMinutes = request.DurationMinutes,
                Status = request.Status,
                AppointmentType = request.AppointmentType
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
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] Guid tenantId)
    {
        try
        {
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
