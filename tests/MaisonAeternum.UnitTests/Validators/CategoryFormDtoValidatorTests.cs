using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Catalog.Validators;

namespace MaisonAeternum.UnitTests.Validators;

public class CategoryFormDtoValidatorTests
{
    private readonly CategoryFormDtoValidator _validator = new();

    private static CategoryFormDto ValidForm() => new()
    {
        Name = "Movements & Mechanisms",
        Description = "A description long enough to pass validation.",
        IconClass = "bi-gear-wide-connected",
        ColorHex = "#C9A24B",
        DisplayOrder = 1
    };

    [Fact]
    public void Validate_Passes_ForValidForm()
    {
        var result = _validator.Validate(ValidForm());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("C9A24B")]      // missing '#'
    [InlineData("#C9A24")]      // too short
    [InlineData("#GGGGGG")]     // not hex
    [InlineData("")]
    public void Validate_Fails_ForInvalidColorHex(string colorHex)
    {
        var form = ValidForm();
        form.ColorHex = colorHex;

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CategoryFormDto.ColorHex));
    }

    [Fact]
    public void Validate_Fails_WhenNameIsEmpty()
    {
        var form = ValidForm();
        form.Name = "";

        var result = _validator.Validate(form);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CategoryFormDto.Name));
    }
}
