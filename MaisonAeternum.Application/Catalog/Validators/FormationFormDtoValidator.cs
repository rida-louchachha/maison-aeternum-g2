using FluentValidation;
using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Validators;

public class FormationFormDtoValidator : AbstractValidator<FormationFormDto>
{
    public FormationFormDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Choose a category.");
        RuleFor(x => x.TrainerId).GreaterThan(0).WithMessage("Choose a trainer.");
        RuleFor(x => x.EstimatedMinutes).GreaterThan(0).WithMessage("Estimated duration must be at least 1 minute.");
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(300);
        RuleFor(x => x.FullDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PrerequisitesText).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Objectives)
            .Must(o => o.Count(x => !string.IsNullOrWhiteSpace(x)) >= 1)
            .WithMessage("Provide at least one learning objective.");
    }
}
