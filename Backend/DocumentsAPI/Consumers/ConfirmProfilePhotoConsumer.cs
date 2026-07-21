using Contracts.DocumentsContracts;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Infrastructure.Photos;
using MassTransit;

namespace DocumentsAPI.Consumers;

public class ConfirmProfilePhotoConsumer(IUserPhotoStorage photoStorage) : IConsumer<ConfirmProfilePhoto>
{
    public async Task Consume(ConsumeContext<ConfirmProfilePhoto> context)
    {
        if( context.Message.OldPhoto != null)
            await photoStorage.DeletePhotoAsync(context.Message.UserId.ToString(), context.Message.OldPhoto.Value, context.CancellationToken);
        await photoStorage.ConfirmPhotoAsync(context.Message.UserId.ToString(), context.Message.PhotoId, context.CancellationToken);
        await photoStorage.SetPublicity(context.Message.UserId.ToString(), context.Message.PhotoId, context.Message.IsPublicUser, context.CancellationToken);
    }
}