using FluentValidation;

namespace AstraFuture.Application.Appointments.Commands.CreateAppointment;

/// <summary>
/// Validador para CreateAppointmentCommand
/// </summary>
public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID é obrigatório");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID é obrigatório");

        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("Resource ID é obrigatório");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(255).WithMessage("Título deve ter no máximo 255 caracteres");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Data/hora agendada é obrigatória")
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Data/hora agendada deve ser no futuro");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duração deve ser maior que zero")
            .LessThanOrEqualTo(480).WithMessage("Duração não pode exceder 8 horas");

        RuleFor(x => x.AppointmentType)
            .NotEmpty().WithMessage("Tipo de appointment é obrigatório")
            .Must(BeValidAppointmentType).WithMessage("Tipo de appointment inválido");
    }

    private bool BeValidAppointmentType(string type)
    {
        var validTypes = new[] { "consultation", "session", "followup", "initial", "other" };
        return validTypes.Contains(type);
    }
}
