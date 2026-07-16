using MicroserviceApiKernel.Results;

namespace DocumentsAPI.Application;

public static class SingleWorkerErrors
{
    public static Error AlreadyAcquired() => Error.Create(ErrorType.Conflict);
}