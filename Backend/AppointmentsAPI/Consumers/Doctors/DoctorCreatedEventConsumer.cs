using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using Contracts.ProfilesContracts;
using MassTransit;

namespace AppointmentsAPI.Consumers.Doctors;

public class DoctorCreatedEventConsumer(AppointmentDbContext db) : IConsumer<DoctorCreatedEvent>
{
    public async Task Consume(ConsumeContext<DoctorCreatedEvent> context)
    {
        await db.Doctors.AddAsync(new Doctor
        {
            AccountId = context.Message.AccountId,
            Id = context.Message.Id,
            CareerStartYear = context.Message.CareerStartYear,
            DateOfBirth = context.Message.DateOfBirth,
            OfficeId = context.Message.OfficeId
        }, context.CancellationToken);
        
        await db.SaveChangesAsync(context.CancellationToken);
    }
}