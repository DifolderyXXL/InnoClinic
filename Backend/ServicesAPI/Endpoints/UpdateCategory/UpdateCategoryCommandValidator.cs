using FluentValidation;

namespace ServicesAPI.Endpoints.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeSlotSize).GreaterThan(TimeSpan.Zero);
    }
}
