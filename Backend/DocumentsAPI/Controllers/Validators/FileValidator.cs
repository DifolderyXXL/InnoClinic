using FluentValidation;

namespace DocumentsAPI.Controllers.Validators;

public class FileValidator : AbstractValidator<IFormFile>
{
    public FileValidator()
    {
        RuleFor(x => x.Length).LessThanOrEqualTo(2 * 1024 * 1024); // 2MB
    }
}