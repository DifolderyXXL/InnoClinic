using AppointmentsAPI.Data;
using Contracts.ProfilesContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AppointmentsAPI.Consumers.Doctors;

public class DoctorUpdatedEventConsumer(AppointmentDbContext db) : IConsumer<DoctorUpdatedEvent>
{
    public async Task Consume(ConsumeContext<DoctorUpdatedEvent> context)
    {
        await db.Doctors.Where(x=>x.Id == context.Message.Id)
            .ExecuteUpdateAsync(setters=>
            {
                setters.SetProperty(x=>x.AccountId, context.Message.AccountId);
                setters.SetProperty(x=>x.CareerStartYear, context.Message.CareerStartYear);
                setters.SetProperty(x=>x.DateOfBirth, context.Message.DateOfBirth);
                setters.SetProperty(x=>x.OfficeId, context.Message.OfficeId);
            }, context.CancellationToken);
    }
}