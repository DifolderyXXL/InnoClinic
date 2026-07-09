using AppointmentsAPI.Data;
using Contracts.ProfilesContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AppointmentsAPI.Consumers.Doctors;

public class DoctorDeletedEventConsumer(AppointmentDbContext db) : IConsumer<DoctorDeletedEvent>
{
    public async Task Consume(ConsumeContext<DoctorDeletedEvent> context)
    {
        await db.Doctors
            .Where(x=>x.Id == context.Message.Id)
            .ExecuteDeleteAsync(context.CancellationToken);
    }
}