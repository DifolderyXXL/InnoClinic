using Contracts.DocumentsContracts;
using DocumentsAPI.Infrastructure;
using MassTransit;

namespace DocumentsAPI.Consumers;

public class ConfirmProfilePhotoConsumer(IProfilePhotoStorage photoStorage) : IConsumer<ConfirmProfilePhoto>
{
    public async Task Consume(ConsumeContext<ConfirmProfilePhoto> context)
    {
        if( context.Message.OldPhoto != null)
            await photoStorage.DeletePhotoAsync(context.Message.UserId, context.Message.OldPhoto.Value, context.CancellationToken);
        await photoStorage.ConfirmPhotoAsync(context.Message.UserId, context.Message.PhotoId, context.CancellationToken);
    }
}