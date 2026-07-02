using System.Diagnostics;
using AppointmentsAPI.ModelBinders;
using MassTransit;
using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    [HttpPost]
    [Route("/book")]
    public IActionResult BookAppointment([ModelBinder<UserClaimsInfoModelBinder>] UserClaimParserResult? user)
    {


        return Ok();
    }
}

public class BookAppointmentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    
    public long AppointmentId { get; set; }
    
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    
    public long ReservationId { get; set; }
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }
}

public record AppointmentSubmitted(long AppointmentId, long PatientId, long DoctorId, DateOnly Date, int StartSlotIndex, int SlotCount);
public record TimeWindowReserved(long AppointmentId, int ReservationId);
public record AppointmentApproved(long AppointmentId);
public record AppointmentDeclined(long AppointmentId, string? Reason);
public record ReservationExpired(long AppointmentId, int ReservationId);
public record ReservationConfirmed(long AppointmentId, int ReservationId);


public record ProcessReservation(long AppointmentId, DateOnly Date, int StartSlotIndex, int SlotCount);
public record ProcessApproval(long AppointmentId);

public record ProcessReservationConfirmation(long AppointmentId, long ReservationId);

public record CancelReservation(long ReservationId);

public class BookAppointmentStateMachine : MassTransitStateMachine<BookAppointmentState>
{
    public Event<AppointmentSubmitted> AppointmentSubmitted { get; private set; } = null!;
    public Event<TimeWindowReserved> TimeWindowReserved { get; private set; } = null!;
    public Event<AppointmentApproved> AppointmentApproved { get; private set; } = null!;
    public Event<ReservationExpired> ReservationExpired { get; private set; } = null!;
    public Event<AppointmentDeclined> AppointmentDeclined { get; private set; } = null!;
    public Event<ReservationConfirmed> ReservationConfirmed { get; private set; } = null!;
    
    
    public BookAppointmentStateMachine()
    {
        Event(() => AppointmentSubmitted, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        Event(() => TimeWindowReserved, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        Event(() => AppointmentApproved, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        Event(() => ReservationExpired, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        Event(() => AppointmentDeclined, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        Event(() => ReservationConfirmed, x => x.CorrelateById(e => e.AppointmentId, e => e.Message.AppointmentId));
        
        InstanceState(x => x.CurrentState);
        
        Initially(
            When(AppointmentSubmitted)
                .Then(context =>
                {
                    context.Saga.PatientId = context.Message.PatientId;
                    context.Saga.DoctorId = context.Message.DoctorId;
                    context.Saga.Date = context.Message.Date;
                    context.Saga.StartSlotIndex = context.Message.StartSlotIndex;
                    context.Saga.SlotCount = context.Message.SlotCount;
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
                .Then(context =>
                {
                    context.Saga.ReservationId = context.Message.ReservationId;
                })
                .PublishAsync(
                    context => context.Init<ProcessApproval>(new ProcessApproval(context.Saga.AppointmentId))
                )
                .TransitionTo(WaitingForApproval),
            When(ReservationExpired)
                .TransitionTo(Failed)
                .Finalize()
        );
        
        During(WaitingForApproval,
            When(AppointmentApproved)
                .PublishAsync(
                    context => context.Init<ProcessReservationConfirmation>(
                        new ProcessReservationConfirmation(context.Saga.AppointmentId, context.Saga.ReservationId))
                    )
                .TransitionTo(WaitingForReservationConfirmation)
                .Finalize(),
            When(AppointmentDeclined)
                .PublishAsync(context => context.Init<CancelReservation>(new CancelReservation(context.Saga.ReservationId)))
                .TransitionTo(Failed)
                .Finalize(),
            When(ReservationExpired)
                .TransitionTo(Failed)
                .Finalize()
        );
        
        During(WaitingForReservationConfirmation,
            When(ReservationConfirmed)
                .TransitionTo(Completed)
                .Finalize(),
            When(ReservationExpired)
                .TransitionTo(Failed)
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

