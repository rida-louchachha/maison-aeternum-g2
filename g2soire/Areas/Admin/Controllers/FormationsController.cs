using FluentValidation;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FormationsController : Controller
{
    private readonly IFormationService _formationService;
    private readonly IValidator<FormationFormDto> _validator;

    public FormationsController(IFormationService formationService, IValidator<FormationFormDto> validator)
    {
        _formationService = formationService;
        _validator = validator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var formations = await _formationService.GetAllForAdminAsync(cancellationToken);
        return View(formations);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateOptionsAsync(cancellationToken);
        return View(new FormationFormDto { HasCertificate = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FormationFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(cancellationToken);
            return View(form);
        }

        var id = await _formationService.CreateAsync(form, cancellationToken);

        TempData["ToastMessage"] = $"Formation \"{form.Title}\" created as a draft.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _formationService.GetForEditAsync(id, cancellationToken);
        if (form is null) return NotFound();

        await PopulateOptionsAsync(cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FormationFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(cancellationToken);
            return View(form);
        }

        await _formationService.UpdateAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Formation updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        await _formationService.PublishAsync(id, cancellationToken);
        TempData["ToastMessage"] = "Formation published.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        await _formationService.ArchiveAsync(id, cancellationToken);
        TempData["ToastMessage"] = "Formation archived.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _formationService.DeleteAsync(id, cancellationToken);
        TempData["ToastMessage"] = "Formation deleted.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(CancellationToken cancellationToken)
    {
        ViewBag.Categories = await _formationService.GetCategoryOptionsAsync(cancellationToken);
        ViewBag.Trainers = await _formationService.GetTrainerOptionsAsync(cancellationToken);
    }
}
