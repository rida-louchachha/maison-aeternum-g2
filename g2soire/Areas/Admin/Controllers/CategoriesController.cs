using FluentValidation;
using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CategoryFormDto> _validator;

    public CategoriesController(ICategoryService categoryService, IValidator<CategoryFormDto> validator)
    {
        _categoryService = categoryService;
        _validator = validator;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormDto { DisplayOrder = 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _categoryService.CreateAsync(form, cancellationToken);

        TempData["ToastMessage"] = $"Category \"{form.Name}\" created.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var form = await _categoryService.GetForEditAsync(id, cancellationToken);
        return form is null ? NotFound() : View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormDto form, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(form, cancellationToken);
        validation.AddToModelState(ModelState);
        if (!ModelState.IsValid) return View(form);

        await _categoryService.UpdateAsync(form, cancellationToken);

        TempData["ToastMessage"] = $"Category \"{form.Name}\" updated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);

        TempData["ToastMessage"] = "Category deleted.";
        TempData["ToastType"] = "info";
        return RedirectToAction(nameof(Index));
    }
}
