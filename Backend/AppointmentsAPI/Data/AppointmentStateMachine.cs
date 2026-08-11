using AppointmentsAPI.Controllers;
using Contracts.AppointmentContracts;
using Contracts.Notifications;
using MassTransit;

namespace AppointmentsAPI.Data;

public class AppointmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    
    public Guid PatientAccountId { get; set; }
    public Guid DoctorAccountId { get; set; }
    
    public string PatientEmail { get; set; } = null!;

    public long ReservationId { get; set; }
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public long ServiceId { get; set; }
    
    public TimeSpan BeginTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Guid AppointmentId => CorrelationId;
    
    public bool IsCreatedByAdmin { get; set; }
}

public class AppointmentSagaStateChanged
{
    public Guid AppointmentId { get; init; } 
    public Models.AppointmentState State { get; init; }
}

public static class AppointmentStateMachineExtension
{
    public static EventActivityBinder<TInstance, TData> PublishStateChanged<TInstance, TData>(
        this EventActivityBinder<TInstance, TData> source,
        Models.AppointmentState state)
        where TInstance : class, SagaStateMachineInstance
        where TData : class
    {
        return source.PublishAsync(context => context.Init<AppointmentSagaStateChanged>(
            new AppointmentSagaStateChanged { AppointmentId = context.Saga.CorrelationId, State = state }));
    }
    
    public static EventActivityBinder<TInstance> PublishStateChanged<TInstance>(
        this EventActivityBinder<TInstance> source,
        Models.AppointmentState state)
        where TInstance : class, SagaStateMachineInstance
    {
        return source.PublishAsync(context => context.Init<AppointmentSagaStateChanged>(
            new AppointmentSagaStateChanged { AppointmentId = context.Saga.CorrelationId, State = state }));
    }
    
    public static EventActivityBinder<TInstance, TData> ApproveAppointment<TInstance, TData>(
        this EventActivityBinder<TInstance, TData> source,
        State waitingForConfirmationState)
        where TInstance : AppointmentState
        where TData : class
    {
        return source
            .PublishAsync(context => context.Init<ProcessReservationConfirmation>(
                new ProcessReservationConfirmation(context.Saga.AppointmentId, context.Saga.ReservationId)))
            .PublishStateChanged(Models.AppointmentState.Approved)
            .TransitionTo(waitingForConfirmationState);
    }
}

public class AppointmentStateMachine : MassTransitStateMachine<AppointmentState>
{
    public Event<AppointmentSubmitted> AppointmentSubmitted { get; private set; } = null!;
    public Event<TimeWindowReserved> TimeWindowReserved { get; private set; } = null!;
    public Event<ReservationFailed> ReservationFailed { get; private set; } = null!;
    public Event<AppointmentApproved> AppointmentApproved { get; private set; } = null!;
    public Event<ReservationExpired> ReservationExpired { get; private set; } = null!;
    public Event<AppointmentDeclined> AppointmentDeclined { get; private set; } = null!;
    public Event<ReservationConfirmed> ReservationConfirmed { get; private set; } = null!;
    
    public AppointmentStateMachine()
    {
        Event(() => AppointmentSubmitted, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => AppointmentApproved, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => AppointmentDeclined, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => ReservationFailed, x => x.CorrelateById(e => e.Message.AppointmentId));
        Event(() => TimeWindowReserved, x => x.CorrelateById(e => e.Message.AppointmentId));

        Event(() => ReservationExpired, x => x.CorrelateBy(
            (state, context) => state.ReservationId == context.Message.ReservationId 
        ));

        Event(() => ReservationConfirmed, x => x.CorrelateBy(
            (state, context) => state.ReservationId == context.Message.ReservationId 
        ));
        
        InstanceState(x => x.CurrentState);
        
        During(WaitingForApproval, WaitingForReservationConfirmation,
            When(AppointmentDeclined)
                .PublishAsync(context => context.Init<CancelReservation>(new CancelReservation(context.Saga.ReservationId)))
                .TransitionTo(Failed) 
        );

        DuringAny(
            When(ReservationFailed)
                .TransitionTo(Failed) 
        );

        During(WaitingForApproval, WaitingForReservationConfirmation,
            When(ReservationExpired)
                .TransitionTo(Failed) 
        );
        
        Initially(
            When(AppointmentSubmitted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.AppointmentId;
                    context.Saga.PatientAccountId = context.Message.PatientAccountId;
                    context.Saga.DoctorAccountId = context.Message.DoctorAccountId;
                    context.Saga.PatientEmail = context.Message.PatientEmail;
                    context.Saga.Date = context.Message.Date;
                    context.Saga.StartSlotIndex = context.Message.StartSlotIndex;
                    context.Saga.ServiceId = context.Message.ServiceId;
                    context.Saga.IsCreatedByAdmin = context.Message.IsCreatedByAdmin;
                })
                .PublishStateChanged(Models.AppointmentState.PendingReservation)
                .PublishAsync(context => context.Init<ProcessReservation>(new ProcessReservation
                (
                    context.Saga.AppointmentId,
                    context.Saga.DoctorAccountId,
                    context.Saga.Date,
                    context.Saga.StartSlotIndex,
                    context.Saga.ServiceId,
                    context.Saga.PatientAccountId
                )))
                .TransitionTo(ProcessingReservation)   
        );
        
        During(ProcessingReservation,
            When(TimeWindowReserved)
                .Then(context =>
                {  
                    context.Saga.ReservationId = context.Message.ReservationId;
                    context.Saga.BeginTime = context.Message.BeginTime;
                    context.Saga.EndTime = context.Message.EndTime;
                })
                .IfElse(
                    context => context.Saga.IsCreatedByAdmin,
                    thenBinder => thenBinder.ApproveAppointment(WaitingForReservationConfirmation),
                    elseBinder => elseBinder
                        .PublishStateChanged(Models.AppointmentState.PendingApproval)
                        .TransitionTo(WaitingForApproval)
                )
        );
        
        During(WaitingForApproval,
            When(AppointmentApproved)
                .ApproveAppointment(WaitingForReservationConfirmation)
        );
        
        During(WaitingForReservationConfirmation,
            When(ReservationConfirmed)
                .PublishStateChanged(Models.AppointmentState.Confirmed)   
                .PublishAsync(context => context.Init<UserAppointmentConfirmedIntegrationEvent>(
                    new UserAppointmentConfirmedIntegrationEvent
                    {
                        AppointmentId = context.Saga.AppointmentId,
                        Date = context.Saga.Date,
                        BeginTime = context.Saga.BeginTime,
                        EndTime = context.Saga.EndTime,
                        Email = context.Saga.PatientEmail
                    }))
                .TransitionTo(Completed)
                .Finalize()
        );
        
        WhenEnter(Failed, binder => binder
            .PublishStateChanged(Models.AppointmentState.Failed)
            .Finalize() 
        );
        
        SetCompletedWhenFinalized();
    }

    public State ProcessingReservation { get; private set; } = null!;
    public State WaitingForApproval { get; private set; } = null!;
    public State WaitingForReservationConfirmation { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
}