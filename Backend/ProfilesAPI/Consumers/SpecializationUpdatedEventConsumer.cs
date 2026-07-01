using Contracts;
using MassTransit;
using ProfilesAPI.Data;
using ProfilesAPI.Models;

namespace ProfilesAPI.Consumers;


[Serializable]
public class EntityNotFoundException(string message) : Exception(message);
public class SpecializationUpdatedEventConsumer(ProfilesDbContext dbcontext) : IConsumer<SpecializationUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SpecializationUpdatedEvent> context)
    {
        var specialization = await dbcontext.Specializations.FindAsync(context.Message.Id, context.CancellationToken);

        if (specialization == null)
        {
            throw new EntityNotFoundException($"Specialization {context.Message.Id} is not created yet.");
        }

        specialization.SpecializationName = context.Message.SpecializationName;
        specialization.IsActive = context.Message.IsActive;

        dbcontext.Specializations.Update(specialization);

        await dbcontext.SaveChangesAsync(context.CancellationToken);
    }
}

public class SpecializationCreatedEventConsumer(ProfilesDbContext dbcontext) : IConsumer<SpecializationCreatedEvent>
{
    public async Task Consume(ConsumeContext<SpecializationCreatedEvent> context)
    {
        var specialization = new Specialization { Id = context.Message.Id, IsActive = context.Message.IsActive, SpecializationName = context.Message.SpecializationName };

        await dbcontext.Specializations.AddAsync(specialization, context.CancellationToken);

        await dbcontext.SaveChangesAsync(context.CancellationToken);
    }
}