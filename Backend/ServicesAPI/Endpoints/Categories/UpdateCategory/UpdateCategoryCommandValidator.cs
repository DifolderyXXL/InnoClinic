using FluentValidation;

namespace ServicesAPI.Endpoints.Categories.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(IValidator<CategoryObject> validator)
    {
        Include(validator);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
