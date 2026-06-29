using FluentValidation;

namespace ServicesAPI.Endpoints.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeSlotSize).GreaterThan(TimeSpan.Zero);
    }
}