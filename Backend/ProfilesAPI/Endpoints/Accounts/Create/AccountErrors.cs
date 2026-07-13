using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Endpoints.Accounts.Create;

public static class AccountErrors
{
    public static Error AlreadyExists() => Error.Create(ErrorType.Conflict);
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}