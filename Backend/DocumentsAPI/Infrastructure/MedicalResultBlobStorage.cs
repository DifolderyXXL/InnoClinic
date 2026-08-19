using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure.Photos;

namespace DocumentsAPI.Infrastructure;

public class MedicalResultBlobStorage(BlobDbContext blobDbContext) : IMedicalResultStorage, IMedicalResultCleaner
{
    private BlobClient GetClient(AppointmentKey key) => blobDbContext.MedicalResultsContainerClient.GetBlobClient($"{key.PatientId}/{key.AppointmentId}.pdf");
    public async Task<(bool Exists, DateTimeOffset? Timestamp)> GetMedicalResultInfoAsync(AppointmentKey appointment, CancellationToken ct)
    {
        var pdfClient = GetClient(appointment);
        
        if (await pdfClient.ExistsAsync(ct))
        {
            BlobProperties properties = await pdfClient.GetPropertiesAsync(cancellationToken: ct);

            if (properties.Metadata.TryGetValue("TimeStamp", out string? timestamp))
            {
                if (DateTimeOffset.TryParse(timestamp, out var datetimeStamp) )
                {
                    return (true, datetimeStamp);
                }
            }

            return (true, null);
        }

        return (false, null);
    }

    public Task<Uri> GenerateReadUriAsync(AppointmentKey appointment)
    {
        var client = GetClient(appointment);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return Task.FromResult(client.GenerateSasUri(sasBuilder));
    }

    public async Task UploadPdfAsync(AppointmentKey appointment, byte[] pdfBytes, DateTimeOffset timestamp, CancellationToken ct)
    {
        var client = GetClient(appointment);
        
        using var memoryStream = new MemoryStream(pdfBytes);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" },
            Metadata = new Dictionary<string, string>
            {
                { "TimeStamp", timestamp.ToString("o") }
            }
        };
                
        await client.UploadAsync(memoryStream, uploadOptions, cancellationToken: ct);
    }

    public async Task DeletePdfIfExistsAsync(AppointmentKey appointment, CancellationToken ct)
    {
        var client = GetClient(appointment);

        await client.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task DeleteMedicalResultsDocumentsByUserId(Guid userId, CancellationToken ct)
    {
        var containerClient = blobDbContext.MedicalResultsContainerClient;
        var prefix = $"{userId}/";

        await BlobContainerHelper.DeleteBlobsByPrefixAsync(containerClient, prefix, ct);
    }
}