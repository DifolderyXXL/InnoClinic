using FluentValidation;

namespace ProfilesAPI.Endpoints.Accounts.Create;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .EmailAddress();
    }

    public static IRuleBuilderOptions<T, string> ValidPersonName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(50);
    }
}