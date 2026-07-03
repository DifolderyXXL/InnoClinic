using AppointmentsAPI.Controllers;
using Contracts.AppointmentContracts;
using MassTransit;

namespace AppointmentsAPI.Data;

public class AppointmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    
    public Guid PatientAccountId { get; set; }
    public long DoctorId { get; set; }
    
    public long ReservationId { get; set; }
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }

    public Guid AppointmentId => CorrelationId;
}

public class AppointmentStateMachine : MassTransitStateMachine<AppointmentState>
{
    public Event<AppointmentSubmitted> AppointmentSubmitted { get; private set; } = null!;
    public Event<TimeWindowReserved> TimeWindowReserved { get; private set; } = null!;
    public Event<AppointmentApproved> AppointmentApproved { get; private set; } = null!;
    public Event<ReservationExpired> ReservationExpired { get; private set; } = null!;
    public Event<AppointmentDeclined> AppointmentDeclined { get; private set; } = null!;
    public Event<ReservationConfirmed> ReservationConfirmed { get; private set; } = null!;
    
    
    public AppointmentStateMachine()
    {
        Event(() => AppointmentSubmitted, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => AppointmentApproved, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => AppointmentDeclined, x => x.CorrelateById(e => e.Message.AppointmentId));

        Event(() => TimeWindowReserved, x => x.CorrelateBy(
            (state, context) => state.ReservationId == context.Message.ReservationId 
        ));
        Event(() => ReservationExpired, x => x.CorrelateBy(
            (state, context) => state.ReservationId == context.Message.ReservationId 
        ));
        Event(() => ReservationConfirmed, x => x.CorrelateBy(
            (state, context) => state.ReservationId == context.Message.ReservationId 
        ));
        
        
        InstanceState(x => x.CurrentState);
        
        DuringAny(
            When(AppointmentDeclined)
                .PublishAsync(context => context.Init<CancelReservation>(new CancelReservation(context.Saga.ReservationId)))
                .TransitionTo(Failed) 
        );
        
        Initially(
            When(AppointmentSubmitted)
                .ThenAsync(async context =>
                {
                    context.Saga.PatientAccountId = context.Message.PatientAccountId;
                    context.Saga.DoctorId = context.Message.DoctorId;
                    context.Saga.Date = context.Message.Date;
                    context.Saga.StartSlotIndex = context.Message.StartSlotIndex;
                    context.Saga.SlotCount = context.Message.SlotCount;

                    var service = context.GetPayload<IAppointmentService>();
                    await service.UpdateState(context.Saga.AppointmentId, Models.AppointmentState.PendingReservation, context.CancellationToken);
                })
                .PublishAsync(context => context.Init<ProcessReservation>(new ProcessReservation
                (
                    context.Saga.AppointmentId,
                    context.Saga.Date,
                    context.Saga.StartSlotIndex,
                    context.Saga.SlotCount
                )))
                .TransitionTo(ProcessingReservation)
        );
        
        During(ProcessingReservation,
            When(TimeWindowReserved)
                .ThenAsync(async context =>
                {
                    context.Saga.ReservationId = context.Message.ReservationId;
                    
                    var service = context.GetPayload<IAppointmentService>();
                    await service.UpdateState(context.Saga.AppointmentId, Models.AppointmentState.PendingApproval, context.CancellationToken);
                })
                .TransitionTo(WaitingForApproval),
            When(ReservationExpired)
                .TransitionTo(Failed)
        );
        
        During(WaitingForApproval,
            When(AppointmentApproved)
                .ThenAsync(async context =>
                {
                    var service = context.GetPayload<IAppointmentService>();
                    await service.UpdateState(context.Saga.AppointmentId, Models.AppointmentState.Approved, context.CancellationToken);
                })
                .PublishAsync(
                    context => context.Init<ProcessReservationConfirmation>(
                        new ProcessReservationConfirmation(context.Saga.AppointmentId, context.Saga.ReservationId))
                )
                .TransitionTo(WaitingForReservationConfirmation),
            When(ReservationExpired)
                .TransitionTo(Failed)
        );
        
        During(WaitingForReservationConfirmation,
            When(ReservationConfirmed)
                .ThenAsync(async context =>
                {
                    var service = context.GetPayload<IAppointmentService>();
                    await service.UpdateState(context.Saga.AppointmentId, Models.AppointmentState.Confirmed, context.CancellationToken);
                })
                .TransitionTo(Completed)
                .Finalize(),
            When(ReservationExpired)
                .TransitionTo(Failed)
        );
        
        WhenEnter(Failed, binder => binder
            .ThenAsync(async context =>
            {
                var service = context.GetPayload<IAppointmentService>();
                await service.UpdateState(context.Saga.AppointmentId, Models.AppointmentState.Failed, context.CancellationToken);
            })
            .Finalize() 
        );
        
        SetCompletedWhenFinalized();
    }

    public State ProcessingReservation { get; private set; } = null!;
    public State WaitingForApproval { get; private set; }= null!;
    public State WaitingForReservationConfirmation { get; private set; }= null!;
    public State Completed { get; private set; }= null!;
    public State Failed { get; private set; }= null!;
}