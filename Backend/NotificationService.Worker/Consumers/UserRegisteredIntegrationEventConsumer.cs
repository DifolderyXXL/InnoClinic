using Contracts.Notifications;
using Infrastructure.Mailing.SMTP;
using MassTransit;

namespace NotificationService.Worker.Consumers;

public class UserRegisteredIntegrationEventConsumer(ISmtpClient mailClient) : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        await mailClient.Send(
            context.Message.Email, 
            "INNO-CLINIC Registration", 
            $"Account created, click to set up password: <a href=\"{context.Message.CreateAccountLink}\">Set up password</a>");
    }
}