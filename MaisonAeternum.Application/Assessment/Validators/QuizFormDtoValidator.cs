using FluentValidation;
using MaisonAeternum.Application.Assessment.Models;

namespace MaisonAeternum.Application.Assessment.Validators;

public class QuizFormDtoValidator : AbstractValidator<QuizFormDto>
{
    public QuizFormDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Instructions).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PassingScore).InclusiveBetween(1, 100);
        RuleFor(x => x.TimeLimitSeconds).GreaterThan(0).When(x => x.TimeLimitSeconds.HasValue);
        RuleFor(x => x.MaxAttempts).GreaterThan(0).When(x => x.MaxAttempts.HasValue);
        RuleFor(x => x.QuestionsToServe).GreaterThan(0).When(x => x.QuestionsToServe.HasValue);
    }
}

public class QuestionFormDtoValidator : AbstractValidator<QuestionFormDto>
{
    public QuestionFormDtoValidator()
    {
        RuleFor(x => x.QuizId).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Explanation).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Points).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThan(0);

        RuleFor(x => x.Options)
            .Must(options => options.Count(o => !string.IsNullOrWhiteSpace(o.Text)) >= 2)
            .WithMessage("Provide at least two answer options.");

        RuleFor(x => x.Options)
            .Must(options => options.Any(o => !string.IsNullOrWhiteSpace(o.Text) && o.IsCorrect))
            .WithMessage("Mark at least one answer option as correct.");
    }
}
