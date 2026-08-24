using AppointmentsAPI.Controllers;
using AppointmentsAPI.Data;
using Contracts.AppointmentContracts;
using Contracts.Notifications;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppointmentsApiTests;

public class AppointmentSagaTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;
    private readonly ITestHarness _harness;
    private readonly ISagaStateMachineTestHarness<AppointmentStateMachine, AppointmentState> _sagaHarness;

    private readonly Guid _appointmentId = Guid.NewGuid();
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _doctorId = Guid.NewGuid();
    private readonly DateOnly _date = DateOnly.MinValue;
    private readonly int _startSlot = 0;
    private readonly int _slotCount = 2;
    private readonly int _reservationId = -2;

    public AppointmentSagaTests()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<AppointmentStateMachine, AppointmentState>().InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        _sagaHarness = _harness.GetSagaStateMachineHarness<AppointmentStateMachine, AppointmentState>();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }


    private async Task GivenAppointmentSubmitted()
    {
        await _harness.Bus.Publish(new AppointmentSubmitted(_appointmentId, _patientId, _doctorId, "", _date, _startSlot, _slotCount, false));
        Assert.True(await _harness.Published.Any<ProcessReservation>(x => x.Context.Message.AppointmentId == _appointmentId));
    }

    private async Task GivenTimeWindowReserved()
    {
        await _harness.Bus.Publish(new TimeWindowReserved(_appointmentId, _reservationId, default, default));
        Assert.True(await _harness.Consumed.Any<TimeWindowReserved>(x => x.Context.Message.AppointmentId == _appointmentId));
    }


    [Fact]
    public async Task AppointmentStateMachine_Successful()
    {
        // Arrange
        await GivenAppointmentSubmitted();
        await GivenTimeWindowReserved();

        // Act
        await _harness.Bus.Publish(new AppointmentApproved(_appointmentId));
        
        // Assert
        Assert.True(await _harness.Published.Any<ProcessReservationConfirmation>(x => x.Context.Message.AppointmentId == _appointmentId));
        Assert.NotNull(_sagaHarness.Sagas.ContainsInState(_appointmentId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.WaitingForReservationConfirmation));
        
        // Act
        await _harness.Bus.Publish(new ReservationConfirmed(_appointmentId, _reservationId));
        
        // Assert
        Assert.True(await _harness.Published.Any<AppointmentConfirmedIntegrationEvent>(x => x.Context.Message.AppointmentId == _appointmentId));
        Assert.NotNull(_sagaHarness.Sagas.ContainsInState(_appointmentId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final));
    }

    [Fact]
    public async Task AppointmentStateMachine_ReservationExpired()
    {
        // Arrange
        await GivenAppointmentSubmitted();
        await GivenTimeWindowReserved();

        // Act
        await _harness.Bus.Publish(new ReservationExpired(_appointmentId, _reservationId));

        // Assert
        Assert.True(await _harness.Published.Any<AppointmentSagaStateChanged>(x =>
            x.Context.Message.AppointmentId == _appointmentId &&
            x.Context.Message.State == AppointmentsAPI.Models.AppointmentState.Failed
        ));
        Assert.NotNull(_sagaHarness.Sagas.ContainsInState(_appointmentId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final));
    }
    
    [Fact]
    public async Task AppointmentStateMachine_AppointmentDeclined()
    {
        // Arrange
        await GivenAppointmentSubmitted();
        await GivenTimeWindowReserved();

        // Act
        await _harness.Bus.Publish(new AppointmentDeclined(_appointmentId, null));
        
        // Assert
        Assert.True(await _harness.Published.Any<CancelReservation>(x => x.Context.Message.ReservationId == _reservationId));
        Assert.NotNull(_sagaHarness.Sagas.ContainsInState(_appointmentId, _sagaHarness.StateMachine, _sagaHarness.StateMachine.Final));
    }
}