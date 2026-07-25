using System.ComponentModel.DataAnnotations;

namespace MaisonAeternum.Web.Models.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = default!;
}

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Token { get; set; } = default!;

    [Required(ErrorMessage = "Please choose a new password.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Your password must be at least {2} characters long.")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Please confirm your new password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The confirmation does not match your password.")]
    public string ConfirmPassword { get; set; } = default!;
}
