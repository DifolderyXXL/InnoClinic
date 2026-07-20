namespace DocumentsAPI.Application;

public interface IMedicalResultStorage
{
    Task<(bool Exists, DateTimeOffset? Timestamp)> GetMedicalResultInfoAsync(AppointmentKey appointment, CancellationToken ct);
    Task<Uri> GenerateReadUriAsync(AppointmentKey appointment);
    Task UploadPdfAsync(AppointmentKey appointment, byte[] pdfBytes, DateTimeOffset timestamp, CancellationToken ct);
    Task DeletePdfIfExistsAsync(AppointmentKey appointment, CancellationToken ct);
}