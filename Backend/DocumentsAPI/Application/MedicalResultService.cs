using DocumentsAPI.Controllers;
using DocumentsAPI.Options;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.Options;

namespace DocumentsAPI.Application;

public class MedicalResultService(
    IPdfMedicalResultGenerator pdfGenerator,
    IMedicalResultStorage storage,
    IDistributedLockService lockService,
    IOptions<PdfGenerationLockOptions> options)
{
    public async Task<Result<Uri>> GetOrCreateMedicalResultPdfAsync(Guid patientId, DateTimeOffset lastUpdate,
        MedicalResultPdfData data, CancellationToken ct)
    {
        var key = new AppointmentKey(patientId, data.AppointmentId);
        if (await IsCacheValidAsync(key, lastUpdate, ct))
        {
            return await storage.GenerateReadUriAsync(key);
        }
        
        var resource = $"appointment-{key.AppointmentId}";
        
        await using var redLock = await lockService.TryAcquireLockAsync(
            resource, options.Value.ExpireTime, options.Value.WaitTime, options.Value.AcquireRetryTime, ct);
        
        if (redLock == null)
        {
            return SingleWorkerErrors.AlreadyAcquired();
        }

        if (await IsCacheValidAsync(key, lastUpdate, ct))
        {
            return await storage.GenerateReadUriAsync(key);
        }

        try
        {
            var pdfBytes = pdfGenerator.Generate(data);

            await storage.UploadPdfAsync(key, pdfBytes, lastUpdate, ct);
        }
        catch (Exception)
        {
            await storage.DeletePdfIfExistsAsync(key, ct);
            throw;
        }
        
        return await storage.GenerateReadUriAsync(key);
    }
    
    private async Task<bool> IsCacheValidAsync(AppointmentKey appointment, DateTimeOffset lastUpdate, CancellationToken ct)
    {
        var (exists, timestamp) = await storage.GetMedicalResultInfoAsync(appointment, ct);
        return exists && timestamp == lastUpdate;
    }
}