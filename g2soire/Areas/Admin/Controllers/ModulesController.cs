using FluentValidation;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ModulesController : Controller
{
    private readonly IModuleService _moduleService;
    private readonly IFormationService _formationService;
    private readonly IValidator<ModuleFormDto> _moduleValidator;
    private readonly IValidator<ContentItemFormDto> _contentItemValidator;

    public ModulesController(
        IModuleService moduleService,
        IFormationService formationService,
        IValidator<ModuleFormDto> moduleValidator,
        IValidator<ContentItemFormDto> contentItemValidator)
    {
        _moduleService = moduleService;
        _formationService = formationService;
        _moduleValidator = moduleValidator;
        _contentItemValidator = contentItemValidator;
    }

    public async Task<IActionResult> Index(int formationId, CancellationToken cancellationToken)
    {
        var formation = await _formationService.GetForEditAsync(formationId, cancellationToken);
        if (formation is null) return NotFound();

        ViewBag.FormationId = formationId;
        ViewBag.FormationTitle = formation.Title;

        var modules = await _moduleService.GetByFormationAsync(formationId, cancellationToken);
        return View(modules);
    }

    [HttpGet]
    public IActionResult Create(int formationId) =>
        View(new ModuleFormDto { FormationId = formationId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ModuleFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _moduleValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _moduleService.CreateAsync(form, cancellationToken);

        TempData["ToastMessage"] = $"Module \"{form.Title}\" added.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index), new { formationId = form.FormationId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _moduleService.GetForEditAsync(id, cancellationToken);
        if (form is null) return NotFound();

        ViewBag.ContentItems = await _moduleService.GetContentItemsAsync(id, cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ModuleFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _moduleValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid)
        {
            ViewBag.ContentItems = await _moduleService.GetContentItemsAsync(form.Id, cancellationToken);
            return View(form);
        }

        await _moduleService.UpdateAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Module updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int formationId, CancellationToken cancellationToken)
    {
        await _moduleService.DeleteAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Module deleted.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Index), new { formationId });
    }

    [HttpGet]
    public IActionResult CreateContentItem(int moduleId) =>
        View(new ContentItemFormDto { ModuleId = moduleId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateContentItem(ContentItemFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _contentItemValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _moduleService.CreateContentItemAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Content added.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.ModuleId });
    }

    [HttpGet]
    public async Task<IActionResult> EditContentItem(int id, CancellationToken cancellationToken)
    {
        var form = await _moduleService.GetContentItemForEditAsync(id, cancellationToken);
        return form is null ? NotFound() : View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditContentItem(ContentItemFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _contentItemValidator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _moduleService.UpdateContentItemAsync(form, cancellationToken);

        TempData["ToastMessage"] = "Content updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Edit), new { id = form.ModuleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContentItem(int id, int moduleId, CancellationToken cancellationToken)
    {
        await _moduleService.DeleteContentItemAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Content removed.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Edit), new { id = moduleId });
    }
}
