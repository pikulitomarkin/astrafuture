using AstraFuture.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AstraFuture.Tests.Domain;

public class AppointmentTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _resourceId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateAppointment()
    {
        // Arrange
        var title = "Consulta de Teste";
        var scheduledAt = DateTime.UtcNow.AddDays(1);
        var durationMinutes = 60;

        // Act
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            title,
            scheduledAt,
            durationMinutes);

        // Assert
        appointment.Should().NotBeNull();
        appointment.TenantId.Should().Be(_tenantId);
        appointment.CustomerId.Should().Be(_customerId);
        appointment.ResourceId.Should().Be(_resourceId);
        appointment.Title.Should().Be(title);
        appointment.ScheduledAt.Should().Be(scheduledAt);
        appointment.DurationMinutes.Should().Be(durationMinutes);
        appointment.EndsAt.Should().Be(scheduledAt.AddMinutes(durationMinutes));
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => Appointment.Create(
            Guid.Empty,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*TenantId*");
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "",
            DateTime.UtcNow.AddDays(1),
            60);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Title*");
    }

    [Fact]
    public void Create_WithZeroDuration_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Duration*");
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(-1),
            60);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*past*");
    }

    [Fact]
    public void Reschedule_ShouldUpdateScheduledAtAndEndsAt()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);

        var newScheduledAt = DateTime.UtcNow.AddDays(2);
        var newDuration = 90;

        // Act
        appointment.Reschedule(newScheduledAt, newDuration);

        // Assert
        appointment.ScheduledAt.Should().Be(newScheduledAt);
        appointment.DurationMinutes.Should().Be(newDuration);
        appointment.EndsAt.Should().Be(newScheduledAt.AddMinutes(newDuration));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateTitleAndDescription()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta Original",
            DateTime.UtcNow.AddDays(1),
            60);

        // Act
        appointment.UpdateDetails("Consulta Atualizada", "Nova descrição");

        // Assert
        appointment.Title.Should().Be("Consulta Atualizada");
        appointment.Description.Should().Be("Nova descrição");
    }

    [Fact]
    public void Confirm_FromScheduled_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelledWithReason()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);

        var reason = "Cliente solicitou cancelamento";

        // Act
        appointment.Cancel(reason);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be(reason);
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_ShouldThrowException()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);
        appointment.Complete();

        // Act
        var act = () => appointment.Cancel("Teste");

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*completed*");
    }

    [Fact]
    public void Reschedule_WhenCancelled_ShouldThrowException()
    {
        // Arrange
        var appointment = Appointment.Create(
            _tenantId,
            _customerId,
            _resourceId,
            "Consulta",
            DateTime.UtcNow.AddDays(1),
            60);
        appointment.Cancel("Cancelado");

        // Act
        var act = () => appointment.Reschedule(DateTime.UtcNow.AddDays(3));

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*cancelled*");
    }
}
