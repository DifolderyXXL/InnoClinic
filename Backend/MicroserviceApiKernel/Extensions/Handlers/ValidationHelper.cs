using FluentValidation;
using FluentValidation.Results;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceApiKernel.Extensions;

public static class ValidationHelper
{
    public static Error CreateError(IValidator validator, ValidationResult result)
    {
        return new Error($"[{validator.GetType().FullName}] Validatoin failed", "", ErrorType.Validation, result.ToDictionary());
    }

    public static async Task<Error?> Validate<TCommand>(IServiceProvider serviceProvider, TCommand command, CancellationToken ct)
    {
        var validators = serviceProvider.GetServices<IValidator<TCommand>>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(command, ct);
            if (!result.IsValid)
            {
                return CreateError(validator, result);
            }
        }

        return null;
    }
}