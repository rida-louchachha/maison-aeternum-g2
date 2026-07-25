using System.ComponentModel.DataAnnotations;

namespace MaisonAeternum.Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please tell us your first name.")]
    [StringLength(50)]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Please tell us your last name.")]
    [StringLength(50)]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Please choose a password.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Your password must be at least {2} characters long.")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The confirmation does not match your password.")]
    public string ConfirmPassword { get; set; } = default!;

    [MustBeTrue(ErrorMessage = "You must accept the Guild Charter to join.")]
    [Display(Name = "I accept the Guild Charter")]
    public bool AcceptTerms { get; set; }
}

public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is bool b && b;
}
