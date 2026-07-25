using FluentValidation;
using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Validators;

public class CategoryFormDtoValidator : AbstractValidator<CategoryFormDto>
{
    public CategoryFormDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.IconClass).NotEmpty().WithMessage("Choose a Bootstrap Icon class, e.g. bi-gear-wide-connected.");
        RuleFor(x => x.ColorHex).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Enter a hex color like #C9A24B.");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
