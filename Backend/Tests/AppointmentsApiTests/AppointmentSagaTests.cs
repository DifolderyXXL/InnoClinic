using AppointmentsAPI.Controllers;
using AppointmentsAPI.Data;
using Contracts.AppointmentContracts;
using Contracts.Notifications;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AppointmentsApiTests;

public class AppointmentSagaTests
{
    [Fact]
    public async Task AppointmentStateMachine_Successful()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<AppointmentStateMachine, AppointmentState>().InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var date = DateOnly.MinValue;
        var startSlot = 0;
        var slotCount = 2;
        var reservationId = -2;
        
        await harness.Bus.Publish(new AppointmentSubmitted(appointmentId, patientId, doctorId, "", date, startSlot, slotCount, false));

        Assert.True(await harness.Consumed.Any<AppointmentSubmitted>());

        var sagaHarness = harness.GetSagaStateMachineHarness<AppointmentStateMachine, AppointmentState>();
        
        Assert.True(await sagaHarness.Created.Any(x=>x.AppointmentId == appointmentId));
        
        Assert.True(await harness.Published.Any<ProcessReservation>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.ProcessingReservation));
        
        
        await harness.Bus.Publish(new TimeWindowReserved(appointmentId, reservationId, default, default));
        Assert.True(await harness.Consumed.Any<TimeWindowReserved>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.WaitingForApproval));
        
        await harness.Bus.Publish(new AppointmentApproved(appointmentId));
        Assert.True(await harness.Published.Any<ProcessReservationConfirmation>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.WaitingForReservationConfirmation));
        
        await harness.Bus.Publish(new ReservationConfirmed(appointmentId, reservationId));
        Assert.True(await harness.Consumed.Any<ReservationConfirmed>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.Final));
        
        Assert.True(await harness.Published.Any<UserAppointmentConfirmedIntegrationEvent>());
    }

    [Fact]
    public async Task AppointmentStateMachine_ReservationExpired()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<AppointmentStateMachine, AppointmentState>().InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var date = DateOnly.MinValue;
        var startSlot = 0;
        var slotCount = 2;
        var reservationId = -2;
        
        await harness.Bus.Publish(new AppointmentSubmitted(appointmentId, patientId, doctorId, "",  date, startSlot, slotCount, false));
        var sagaHarness = harness.GetSagaStateMachineHarness<AppointmentStateMachine, AppointmentState>();
        
        await harness.Bus.Publish(new TimeWindowReserved(appointmentId, reservationId, default, default));
        Assert.True(await harness.Consumed.Any<TimeWindowReserved>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.WaitingForApproval));
        
        await harness.Bus.Publish(new ReservationExpired(appointmentId, reservationId));
        Assert.True(await harness.Published.Any<AppointmentSagaStateChanged>(x =>
            x.Context.Message.AppointmentId == appointmentId &&
            x.Context.Message.State == AppointmentsAPI.Models.AppointmentState.Failed
        ));
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.Final));
    }
    
    [Fact]
    public async Task AppointmentStateMachine_AppointmentDeclined()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<AppointmentStateMachine, AppointmentState>().InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var date = DateOnly.MinValue;
        var startSlot = 0;
        var slotCount = 2;
        var reservationId = -2;
        
        await harness.Bus.Publish(new AppointmentSubmitted(appointmentId, patientId, doctorId, "",  date, startSlot, slotCount, false));
        var sagaHarness = harness.GetSagaStateMachineHarness<AppointmentStateMachine, AppointmentState>();
        
        await harness.Bus.Publish(new TimeWindowReserved(appointmentId, reservationId,  default, default));
        Assert.True(await harness.Consumed.Any<TimeWindowReserved>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.WaitingForApproval));
        
        
        await harness.Bus.Publish(new AppointmentDeclined(appointmentId, null));
        Assert.True(await harness.Consumed.Any<AppointmentDeclined>());
        Assert.True(await harness.Published.Any<CancelReservation>());
        Assert.NotNull(sagaHarness.Sagas.ContainsInState(appointmentId, sagaHarness.StateMachine, sagaHarness.StateMachine.Final));
    }
}