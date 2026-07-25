using FluentValidation;
using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Validators;

public class ModuleFormDtoValidator : AbstractValidator<ModuleFormDto>
{
    public ModuleFormDtoValidator()
    {
        RuleFor(x => x.FormationId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.DisplayOrder).GreaterThan(0);
        RuleFor(x => x.EstimatedMinutes).GreaterThan(0);
    }
}

public class ContentItemFormDtoValidator : AbstractValidator<ContentItemFormDto>
{
    public ContentItemFormDtoValidator()
    {
        RuleFor(x => x.ModuleId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExternalUrl).NotEmpty().WithMessage("Provide a URL to the video, PDF, or audio file.");
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThan(0);
    }
}
