using FluentValidation;
using MaisonAeternum.Web.Models.Account;

namespace MaisonAeternum.Web.Validators;

/// <summary>
/// Data Annotations on RegisterViewModel handle per-field shape (required, length, format).
/// This validator handles the one cross-field business rule Annotations can't express cleanly:
/// a password must not trivially contain the apprentice's own name.
/// </summary>
public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator()
    {
        RuleFor(x => x.Password)
            .Must((model, password) => !ContainsName(password, model.FirstName, model.LastName))
            .WithMessage("Your password should not contain your own name.");
    }

    private static bool ContainsName(string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        return (!string.IsNullOrWhiteSpace(firstName) && password.Contains(firstName, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(lastName) && password.Contains(lastName, StringComparison.OrdinalIgnoreCase));
    }
}
