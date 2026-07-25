using FluentValidation;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Domain.Enums;
using MaisonAeternum.Infrastructure.Identity;
using MaisonAeternum.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TrainersController : Controller
{
    private readonly ITrainerProfileService _trainerService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateTrainerDto> _createValidator;
    private readonly IValidator<TrainerProfileFormDto> _profileValidator;

    public TrainersController(
        ITrainerProfileService trainerService,
        UserManager<ApplicationUser> userManager,
        IValidator<CreateTrainerDto> createValidator,
        IValidator<TrainerProfileFormDto> profileValidator)
    {
        _trainerService = trainerService;
        _userManager = userManager;
        _createValidator = createValidator;
        _profileValidator = profileValidator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var trainers = await _trainerService.GetAllAsync(cancellationToken);
        return View(trainers);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateTrainerDto
    {
        Profile = new TrainerProfileFormDto
        {
            SocialLinks = new List<TrainerSocialLinkFormDto>
            {
                new() { Platform = SocialPlatform.Instagram },
                new() { Platform = SocialPlatform.LinkedIn }
            }
        }
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTrainerDto form, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        if (await _userManager.FindByEmailAsync(form.Email) is not null)
        {
            ModelState.AddModelError(nameof(form.Email), "An account with this email already exists.");
            return View(form);
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var user = new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            EmailConfirmed = true,
            FirstName = form.FirstName,
            LastName = form.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(form);
        }

        await _userManager.AddToRoleAsync(user, "Trainer");
        await _trainerService.CreateAsync(user.Id, form.Profile, cancellationToken);

        TempData["ToastMessage"] = $"Trainer \"{form.FirstName} {form.LastName}\" created. Temporary password: {temporaryPassword}";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _trainerService.GetForEditAsync(id, cancellationToken);
        return form is null ? NotFound() : View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TrainerProfileFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _profileValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _trainerService.UpdateAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Trainer profile updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _trainerService.DeleteAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Trainer removed.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Index));
    }

    private static string GenerateTemporaryPassword()
    {
        const string symbols = "!@#$%*";
        var randomBytes = RandomNumberGenerator.GetBytes(9);
        var body = Convert.ToBase64String(randomBytes).Replace("+", "A").Replace("/", "B").Replace("=", "");
        var symbol = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];
        return $"{body}{symbol}9";
    }
}
