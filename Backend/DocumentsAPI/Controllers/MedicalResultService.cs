using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Options;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.Options;
using RedLockNet;

namespace DocumentsAPI.Controllers;

public class MedicalResultService(BlobDbContext blobDbContext, IPdfMedicalResultGenerator pdfGenerator, IDistributedLockFactory lockFactory,
    IOptions<PdfGenerationLockOptions> options)
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

    private async Task<Uri?> GenerateSasIfValid(BlobClient pdfClient, DateTimeOffset lastUpdate, CancellationToken ct)
    {
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

        return null;
    }
    
    public async Task<Result<Uri>> GetOrCreateMedicalResultPdfAsync(Guid patientId, DateTimeOffset lastUpdate, MedicalResultPdfData data, CancellationToken ct)
    {
        var pdfClient = blobDbContext.MedicalResultsContainerClient.GetBlobClient($"{patientId}/{data.AppointmentId}.pdf");

        var uri = await GenerateSasIfValid(pdfClient, lastUpdate, ct);
        if (uri != null) return uri;
        
        var resource = $"appointment-{data.AppointmentId}";
        var expiry = options.Value.ExpireTime;
        var waitTime = options.Value.WaitTime;
        var retryTime = options.Value.AcquireRetryTime;

        await using (var redLock = await lockFactory.CreateLockAsync(resource, expiry, waitTime, retryTime, ct))
        {
            if (!redLock.IsAcquired)
            {
                return SingleWorkerErrors.AlreadyAcquired();
            }

            uri = await GenerateSasIfValid(pdfClient, lastUpdate, ct);
            if (uri != null) return uri;

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