using System.Net;
using MicroserviceApiKernel.Results;
using ProfilesAPI.Application;

namespace ProfilesAPI.Infrastructure;

public class DocumentsApiServiceClient(HttpClient client) : IDocumentsApiServiceClient
{
    public async Task<Result> DeleteAllUserPhotos(Guid userId, CancellationToken ct)
    {
        try
        {
            var response = await client.DeleteAsync($"/Photos/users/{userId}", ct);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => Result.Success(),
                _ => Error.Failure("DocumentsApi.DeletePhotosFailed", $"Failed to delete user photos. Status: {response.StatusCode}")
            };
        }
        catch (Exception ex)
        {
            return Error.Failure("DocumentsApi.DeletePhotosException", ex.Message);
        }
    }

    public async Task<Result> DeleteAllUserMedicalResults(Guid userId, CancellationToken ct)
    {
        try
        {
            var response = await client.DeleteAsync($"MedicalResults/users/{userId}", ct);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => Result.Success(),
                _ => Error.Failure("DocumentsApi.DeleteMedicalResultsFailed", $"Failed to delete user medical results. Status: {response.StatusCode}")
            };
        }
        catch (Exception ex)
        {
            return Error.Failure("DocumentsApi.DeleteMedicalResultsException", ex.Message);
        }
    }
}