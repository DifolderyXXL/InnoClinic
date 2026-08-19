using Contracts.ProfilesContracts;
using MassTransit;
using ProfilesAPI.Application;

namespace ProfilesAPI.Consumers;

public class UserDeletionRequestedConsumer(
    IDocumentsApiServiceClient documents,
    IAppointmentsApiServiceClient appointments,
    ILogger<UserDeletionRequestedConsumer> logger) : IConsumer<UserDeletionRequestedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserDeletionRequestedIntegrationEvent> context)
    {
        var userId = context.Message.UserId;

        logger.LogInformation("Processing background cleanup for deleted user: {UserId}", userId);

        var photosTask = documents.DeleteAllUserPhotos(userId, context.CancellationToken);
        var medicalResultsTask = documents.DeleteAllUserMedicalResults(userId, context.CancellationToken);
        var appointmentResultTask = appointments.DeleteAllUserAppointments(userId, context.CancellationToken);

        var results = await Task.WhenAll(photosTask, medicalResultsTask, appointmentResultTask);

        var failedResult = results.FirstOrDefault(r => r.IsError);
        if (failedResult is not null)
        {
            logger.LogError("Failed background cleanup for user {UserId}. Error: {Error}. Type: {ErrorType}", 
                userId, failedResult.Error?.ErrorName, failedResult.Error?.ErrorType);
            
            throw new InvalidOperationException($"Background user data cleanup failed: {failedResult.Error?.ErrorDescription}");
        }

        logger.LogInformation("Successfully completed background cleanup for user {UserId}", userId);
    }
}