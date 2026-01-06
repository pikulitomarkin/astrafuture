# Backend - AstraFuture (.NET 8)

## Estrutura Clean Architecture

```
backend/
├── AstraFuture.Api/                 # 🌐 Presentation Layer
│   ├── Controllers/            # Endpoints REST
│   ├── Middleware/             # Auth, Logging, Error Handling
│   ├── Filters/                # Exception filters
│   ├── Program.cs              # Entry point
│   └── appsettings.json
│
├── AstraFuture.Application/         # 📋 Use Cases Layer
│   ├── UseCases/
│   │   ├── Appointments/       # Commands & Queries
│   │   ├── Customers/
│   │   └── Tenants/
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Interfaces/             # Repository & Service abstractions
│   ├── Validators/             # FluentValidation
│   └── Mappings/               # AutoMapper profiles
│
├── AstraFuture.Domain/              # 🏛️ Domain Layer (Core)
│   ├── Entities/               # Domain entities
│   │   ├── Tenant.cs
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   └── Customer.cs
│   ├── ValueObjects/           # Value Objects
│   ├── Events/                 # Domain Events
│   ├── Exceptions/             # Domain-specific exceptions
│   └── Interfaces/             # Domain contracts
│
├── AstraFuture.Infrastructure/      # 🔧 Infrastructure Layer
│   ├── Data/
│   │   ├── Repositories/       # Repository implementations
│   │   ├── SupabaseContext.cs  # DB Context
│   │   └── Migrations/         # EF Core migrations
│   ├── Services/
│   │   ├── CacheService.cs     # Redis caching
│   │   ├── NotificationService.cs
│   │   └── WhatsAppService.cs
│   └── ExternalApis/           # Third-party integrations
│
└── AstraFuture.Shared/              # 🔀 Cross-cutting
    ├── Constants/
    ├── Extensions/
    ├── Helpers/
    └── Models/                 # Shared models
```

---

## Exemplo de Implementação

### 1. Domain Entity

**AstraFuture.Domain/Entities/Appointment.cs**
```csharp
namespace AstraFuture.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid? AssignedTo { get; private set; }
    
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateTime EndsAt => ScheduledAt.AddMinutes(DurationMinutes);
    
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public AppointmentStatus Status { get; private set; }
    
    public int? PriceCents { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual Tenant Tenant { get; set; } = null!;
    
    // Factory method
    public static Appointment Create(
        Guid tenantId,
        Guid customerId,
        Guid resourceId,
        DateTime scheduledAt,
        int durationMinutes,
        string title)
    {
        // Validations
        if (durationMinutes < 15)
            throw new DomainException("Duration must be at least 15 minutes");
        
        if (scheduledAt < DateTime.UtcNow)
            throw new DomainException("Cannot schedule in the past");
        
        return new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            ResourceId = resourceId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Title = title,
            Status = AppointmentStatus.Scheduled,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    // Business methods
    public void Reschedule(DateTime newScheduledAt)
    {
        if (Status == AppointmentStatus.Completed)
            throw new DomainException("Cannot reschedule completed appointment");
        
        ScheduledAt = newScheduledAt;
        AddDomainEvent(new AppointmentRescheduledEvent(Id, newScheduledAt));
    }
    
    public void Complete(string? meetingNotes = null)
    {
        if (Status != AppointmentStatus.InProgress)
            throw new DomainException("Can only complete in-progress appointments");
        
        Status = AppointmentStatus.Completed;
        Description = meetingNotes ?? Description;
        AddDomainEvent(new AppointmentCompletedEvent(Id));
    }
    
    public void Cancel(string reason)
    {
        if (Status == AppointmentStatus.Completed)
            throw new DomainException("Cannot cancel completed appointment");
        
        Status = AppointmentStatus.Cancelled;
        AddDomainEvent(new AppointmentCancelledEvent(Id, reason));
    }
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}
```

---

### 2. Use Case (Application Layer)

**AstraFuture.Application/UseCases/Appointments/CreateAppointmentCommand.cs**
```csharp
namespace AstraFuture.Application.UseCases.Appointments;

public record CreateAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ResourceId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? PriceCents { get; init; }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly ITenantContext _tenantContext;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;
    
    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IResourceRepository resourceRepository,
        ITenantContext tenantContext,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _resourceRepository = resourceRepository;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Check for conflicts
            var hasConflict = await _appointmentRepository.CheckConflictAsync(
                request.ResourceId,
                request.ScheduledAt,
                request.DurationMinutes,
                cancellationToken);
            
            if (hasConflict)
            {
                var suggestedSlots = await _resourceRepository.GetAvailableSlotsAsync(
                    request.ResourceId,
                    request.ScheduledAt.Date,
                    request.DurationMinutes,
                    cancellationToken);
                
                return Result<AppointmentDto>.Failure(
                    "APPOINTMENT_CONFLICT",
                    "Horário indisponível",
                    new { SuggestedSlots = suggestedSlots });
            }
            
            // 2. Create appointment (domain logic)
            var appointment = Appointment.Create(
                _tenantContext.TenantId,
                request.CustomerId,
                request.ResourceId,
                request.ScheduledAt,
                request.DurationMinutes,
                request.Title);
            
            if (!string.IsNullOrEmpty(request.Description))
                appointment.SetDescription(request.Description);
            
            if (request.PriceCents.HasValue)
                appointment.SetPrice(request.PriceCents.Value);
            
            // 3. Persist
            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // 4. Send notification (background)
            _ = _notificationService.SendAppointmentConfirmationAsync(
                appointment.Id,
                cancellationToken);
            
            _logger.LogInformation(
                "Appointment created: {AppointmentId} for Tenant: {TenantId}",
                appointment.Id,
                appointment.TenantId);
            
            // 5. Map to DTO
            var dto = AppointmentDto.FromEntity(appointment);
            
            return Result<AppointmentDto>.Success(dto);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed");
            return Result<AppointmentDto>.Failure("VALIDATION_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create appointment");
            return Result<AppointmentDto>.Failure("INTERNAL_ERROR", "Erro ao criar agendamento");
        }
    }
}
```

---

### 3. API Controller

**AstraFuture.Api/Controllers/AppointmentsController.cs**
```csharp
namespace AstraFuture.Api.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Criar novo agendamento
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "APPOINTMENT_CONFLICT")
                return Conflict(ApiResponse.Error(result.ErrorCode, result.ErrorMessage, result.ErrorDetails));
            
            return BadRequest(ApiResponse.Error(result.ErrorCode, result.ErrorMessage));
        }
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            ApiResponse.Success(result.Data));
    }
    
    /// <summary>
    /// Listar appointments com filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AppointmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListAppointmentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse.Success(result.Data));
    }
    
    /// <summary>
    /// Obter appointment por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetAppointmentByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.IsSuccess)
            return NotFound(ApiResponse.Error("NOT_FOUND", "Agendamento não encontrado"));
        
        return Ok(ApiResponse.Success(result.Data));
    }
    
    /// <summary>
    /// Reagendar appointment
    /// </summary>
    [HttpPost("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reschedule(
        Guid id,
        [FromBody] RescheduleAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { AppointmentId = id };
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.Error(result.ErrorCode, result.ErrorMessage));
        
        return Ok(ApiResponse.Success(result.Data));
    }
    
    /// <summary>
    /// Marcar como concluído
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { AppointmentId = id };
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.Error(result.ErrorCode, result.ErrorMessage));
        
        return Ok(ApiResponse.Success(result.Data));
    }
    
    /// <summary>
    /// Cancelar appointment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { AppointmentId = id };
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(ApiResponse.Error(result.ErrorCode, result.ErrorMessage));
        
        return NoContent();
    }
}
```

---

### 4. Repository Implementation

**AstraFuture.Infrastructure/Data/Repositories/AppointmentRepository.cs**
```csharp
namespace AstraFuture.Infrastructure.Data.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly SupabaseClient _supabase;
    private readonly ITenantContext _tenantContext;
    
    public AppointmentRepository(SupabaseClient supabase, ITenantContext tenantContext)
    {
        _supabase = supabase;
        _tenantContext = tenantContext;
    }
    
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // RLS automaticamente filtra por tenant_id
        var response = await _supabase
            .From<Appointment>()
            .Where(a => a.Id == id)
            .Single();
        
        return response;
    }
    
    public async Task<PagedResult<Appointment>> ListAsync(
        AppointmentFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _supabase.From<Appointment>().AsQueryable();
        
        // Filtros
        if (filter.From.HasValue)
            query = query.Where(a => a.ScheduledAt >= filter.From.Value);
        
        if (filter.To.HasValue)
            query = query.Where(a => a.ScheduledAt <= filter.To.Value);
        
        if (filter.Status != null)
            query = query.Where(a => filter.Status.Contains(a.Status));
        
        if (filter.CustomerId.HasValue)
            query = query.Where(a => a.CustomerId == filter.CustomerId.Value);
        
        if (filter.ResourceId.HasValue)
            query = query.Where(a => a.ResourceId == filter.ResourceId.Value);
        
        // Total count
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Paginação
        var items = await query
            .OrderByDescending(a => a.ScheduledAt)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Appointment>(items, totalCount, filter.Page, filter.Limit);
    }
    
    public async Task<bool> CheckConflictAsync(
        Guid resourceId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var endsAt = scheduledAt.AddMinutes(durationMinutes);
        
        var query = _supabase
            .From<Appointment>()
            .Where(a => a.ResourceId == resourceId)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow);
        
        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        
        // Check overlap
        var conflicts = await query
            .Where(a => 
                (scheduledAt >= a.ScheduledAt && scheduledAt < a.EndsAt) ||
                (endsAt > a.ScheduledAt && endsAt <= a.EndsAt) ||
                (scheduledAt <= a.ScheduledAt && endsAt >= a.EndsAt))
            .CountAsync(cancellationToken);
        
        return conflicts > 0;
    }
    
    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _supabase.From<Appointment>().Insert(appointment);
    }
    
    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _supabase.From<Appointment>().Update(appointment);
    }
    
    public async Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        // Soft delete
        appointment.Delete();
        await UpdateAsync(appointment, cancellationToken);
    }
}
```

---

### 5. Middleware (Tenant Context)

**AstraFuture.Api/Middleware/TenantContextMiddleware.cs**
```csharp
namespace AstraFuture.Api.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantContextMiddleware> _logger;
    
    public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, SupabaseClient supabase)
    {
        // Skip for public endpoints
        if (context.Request.Path.StartsWithSegments("/api/v1/auth") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }
        
        // Extract tenant_id from JWT claims
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            _logger.LogWarning("Missing or invalid tenant_id claim");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "INVALID_TENANT",
                    message = "Tenant ID inválido ou ausente"
                }
            });
            return;
        }
        
        // Set tenant context (usado por repositories)
        tenantContext.TenantId = tenantId;
        
        // Set RLS context no Supabase
        await supabase.Rpc("set_config", new
        {
            setting = "app.tenant_id",
            value = tenantId.ToString(),
            is_local = true
        });
        
        _logger.LogInformation("Tenant context set: {TenantId}", tenantId);
        
        await _next(context);
    }
}
```

---

## Próximos Passos

1. **Implementar testes unitários** para `CreateAppointmentCommandHandler`
2. **Implementar testes de integração** para `AppointmentsController`
3. **Adicionar Swagger/OpenAPI** documentation
4. **Configurar Serilog** para structured logging
5. **Implementar Health Checks** (`/health`)

---

**Ver também:**
- [API Documentation](../api/README.md)
- [Database Schema](../database/schema.sql)
