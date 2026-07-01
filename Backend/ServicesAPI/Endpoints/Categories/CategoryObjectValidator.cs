using FluentValidation;

namespace ServicesAPI.Endpoints.Categories;

public class CategoryObjectValidator : AbstractValidator<CategoryObject>
{
    public CategoryObjectValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeSlotSize).GreaterThan((uint)0);
    }
}