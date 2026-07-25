using FluentValidation;
using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Validators;

public class TrainerProfileFormDtoValidator : AbstractValidator<TrainerProfileFormDto>
{
    public TrainerProfileFormDtoValidator()
    {
        RuleFor(x => x.Biography).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.AtelierAffiliation).NotEmpty().MaximumLength(150);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 70);
    }
}

public class CreateTrainerDtoValidator : AbstractValidator<CreateTrainerDto>
{
    public CreateTrainerDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Profile).SetValidator(new TrainerProfileFormDtoValidator());
    }
}
