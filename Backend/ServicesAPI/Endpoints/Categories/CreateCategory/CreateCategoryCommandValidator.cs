using FluentValidation;

namespace ServicesAPI.Endpoints.Categories.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IValidator<CategoryObject> validator)
    {
        Include(validator);
    }
}