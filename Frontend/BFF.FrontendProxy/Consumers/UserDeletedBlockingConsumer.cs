using BFF.FrontendProxy.Services;
using Contracts.ProfilesContracts;
using MassTransit;

namespace BFF.FrontendProxy.Consumers;

public class UserDeletedBlockingConsumer(
    IRevokedUserRepository revokedUserRepository,
    ILogger<UserDeletedBlockingConsumer> logger) : IConsumer<UserDeletionRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserDeletionRequestedIntegrationEvent> context)
    {
        var userId = context.Message.UserId.ToString();

        logger.LogInformation("Processing user access revocation for UserId: {UserId}", userId);

        try
        {
            await revokedUserRepository.RevokeUser(userId, context.CancellationToken);

            logger.LogInformation("Successfully revoked user access for UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while revoking access for UserId: {UserId}", userId);
            
            throw;
        }
    }
}