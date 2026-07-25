using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Assessment.Validators;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.UnitTests.Validators;

public class QuestionFormDtoValidatorTests
{
    private readonly QuestionFormDtoValidator _validator = new();

    private static QuestionFormDto ValidForm() => new()
    {
        QuizId = 1,
        Text = "What is the primary purpose of the escapement?",
        Type = QuestionType.SingleChoice,
        Explanation = "It regulates the release of energy.",
        Points = 10,
        DisplayOrder = 1,
        Options = new List<AnswerOptionFormDto>
        {
            new() { Text = "To regulate energy release", IsCorrect = true },
            new() { Text = "To wind the mainspring", IsCorrect = false },
            new(), new()
        }
    };

    [Fact]
    public void Validate_Passes_ForValidForm()
    {
        Assert.True(_validator.Validate(ValidForm()).IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenFewerThanTwoOptionsHaveText()
    {
        var form = ValidForm();
        form.Options = new List<AnswerOptionFormDto> { new() { Text = "Only one", IsCorrect = true }, new(), new(), new() };

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(QuestionFormDto.Options));
    }

    [Fact]
    public void Validate_Fails_WhenNoOptionIsMarkedCorrect()
    {
        var form = ValidForm();
        foreach (var option in form.Options) option.IsCorrect = false;

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(QuestionFormDto.Options));
    }
}
