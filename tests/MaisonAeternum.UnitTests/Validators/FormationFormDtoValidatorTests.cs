using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Catalog.Validators;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.UnitTests.Validators;

public class FormationFormDtoValidatorTests
{
    private readonly FormationFormDtoValidator _validator = new();

    private static FormationFormDto ValidForm() => new()
    {
        Title = "Gear Train Fundamentals",
        CategoryId = 1,
        TrainerId = 1,
        Difficulty = DifficultyLevel.Apprentice,
        EstimatedMinutes = 90,
        ShortDescription = "Short description.",
        FullDescription = "A longer, full description of the formation.",
        PrerequisitesText = "No prior formation required.",
        Objectives = new List<string> { "Understand gear trains", "", "" }
    };

    [Fact]
    public void Validate_Passes_ForValidForm()
    {
        Assert.True(_validator.Validate(ValidForm()).IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenNoCategorySelected()
    {
        var form = ValidForm();
        form.CategoryId = 0;

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FormationFormDto.CategoryId));
    }

    [Fact]
    public void Validate_Fails_WhenAllObjectivesAreBlank()
    {
        var form = ValidForm();
        form.Objectives = new List<string> { "", "  ", "" };

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FormationFormDto.Objectives));
    }

    [Fact]
    public void Validate_Fails_WhenEstimatedMinutesIsZero()
    {
        var form = ValidForm();
        form.EstimatedMinutes = 0;

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
    }
}
