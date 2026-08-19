using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DocumentsAPI.Infrastructure.Photos;

public static class BlobContainerHelper
{
    public static async Task DeleteBlobsByPrefixAsync(
        BlobContainerClient containerClient, 
        string prefix, 
        CancellationToken ct)
    {
        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, traits: BlobTraits.None, states: BlobStates.None, cancellationToken: ct))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
        }
    }
}