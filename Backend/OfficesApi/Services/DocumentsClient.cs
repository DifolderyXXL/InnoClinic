namespace OfficesApi.Services;


public interface IDocumentsClient
{
    Task ConfirmOfficePhotoAsync(string officeId, Guid photoId, Guid? oldPhotoId, CancellationToken ct);
}

public class DocumentsClient : IDocumentsClient
{
    private readonly HttpClient _httpClient;

    public DocumentsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ConfirmOfficePhotoAsync(string officeId, Guid photoId, Guid? oldPhotoId, CancellationToken ct)
    {
        var route = $"offices/{officeId}/avatar/confirm?photoId={photoId}";
        if (oldPhotoId != null) route += $"&oldPhotoId={oldPhotoId.Value}";
            
        var response = await _httpClient.PostAsync(route, null, ct);
        response.EnsureSuccessStatusCode();
    }
}