using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure;
using MicroserviceApiKernel.Results;
using RedLockNet;

namespace DocumentsAPI.Controllers;

public class MedicalResultService(BlobDbContext blobDbContext, IPdfMedicalResultGenerator pdfGenerator, IDistributedLockFactory lockFactory)
{
    private Uri GenerateSas(BlobClient client)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return client.GenerateSasUri(sasBuilder);
    }
    
    public async Task<Result<Uri>> GetOrCreateMedicalResultPdfAsync(Guid patientId, DateTimeOffset lastUpdate, MedicalResultPdfData data, CancellationToken ct)
    {
        var pdfClient = blobDbContext.MedicalResultsContainerClient.GetBlobClient($"{patientId}/{data.AppointmentId}.pdf");

        if (await pdfClient.ExistsAsync(ct))
        {
            BlobProperties properties = await pdfClient.GetPropertiesAsync(cancellationToken: ct);

            if (properties.Metadata.TryGetValue("TimeStamp", out string? timestamp))
            {
                if (DateTimeOffset.TryParse(timestamp, out var datetimeStamp) && datetimeStamp == lastUpdate)
                {
                    return GenerateSas(pdfClient);
                }
            }
        }
        
        var resource = $"appointment-{data.AppointmentId}";
        var expiry = TimeSpan.FromMinutes(5);
        var waitTime = TimeSpan.FromSeconds(30);
        var retryTime = TimeSpan.FromMilliseconds(500);

        await using (var redLock = await lockFactory.CreateLockAsync(resource, expiry, waitTime, retryTime, ct))
        {
            if (!redLock.IsAcquired)
            {
                return SingleWorkerErrors.AlreadyAcquired();
            }

            if (await pdfClient.ExistsAsync(ct))
            {
                BlobProperties properties = await pdfClient.GetPropertiesAsync(cancellationToken: ct);

                if (properties.Metadata.TryGetValue("TimeStamp", out string? timestamp))
                {
                    if (DateTimeOffset.TryParse(timestamp, out var datetimeStamp) && datetimeStamp == lastUpdate)
                    {
                        return GenerateSas(pdfClient);
                    }
                }
            }

            try
            {
                var pdfBytes = pdfGenerator.Generate(data);
                using var memoryStream = new MemoryStream(pdfBytes);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "TimeStamp", lastUpdate.ToString("o") }
                    }
                };
                
                await pdfClient.UploadAsync(memoryStream, uploadOptions, cancellationToken: ct);
            }
            catch (Exception)
            {
                await pdfClient.DeleteIfExistsAsync(cancellationToken: ct);
                throw;
            }
        }

        return GenerateSas(pdfClient);
    }
}