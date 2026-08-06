using Contracts.Notifications;
using MassTransit;

namespace NotificationService.Worker.Consumers;

public class UserRegisteredIntegrationEventConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        throw new NotImplementedException();
    }
}