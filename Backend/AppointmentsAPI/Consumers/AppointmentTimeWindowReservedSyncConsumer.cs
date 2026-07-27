using AppointmentsAPI.Controllers;
using Contracts.AppointmentContracts;
using MassTransit;

namespace AppointmentsAPI.Consumers;

public class AppointmentTimeWindowReservedSyncConsumer(
    IAppointmentService service) : IConsumer<TimeWindowReserved>
{
    public async Task Consume(ConsumeContext<TimeWindowReserved> context)
    {
        var result = await service.UpdateReservation(
            context.Message.AppointmentId, 
            context.Message.ReservationId, 
            context.Message.BeginTime, 
            context.Message.EndTime, 
            context.CancellationToken);

        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"Failed to sync reservation for appointment '{context.Message.AppointmentId}'. Error: {result.Error}");
        }
    }
}