using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Application;

public interface IDocumentsApiServiceClient
{
    Task<Result> DeleteAllUserPhotos(Guid userId, CancellationToken ct);
    Task<Result> DeleteAllUserMedicalResults(Guid userId, CancellationToken ct);
}