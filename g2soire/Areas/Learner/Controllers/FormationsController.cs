using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaisonAeternum.Web.Areas.Learner.Controllers;

[Area("Learner")]
[Authorize(Roles = "Learner")]
public class FormationsController : Controller
{
    private readonly IFormationRepository _formations;
    private readonly ICategoryRepository _categories;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILearnerProfileRepository _learnerProfiles;

    public FormationsController(
        IFormationRepository formations,
        ICategoryRepository categories,
        IEnrollmentService enrollmentService,
        ILearnerProfileRepository learnerProfiles)
    {
        _formations = formations;
        _categories = categories;
        _enrollmentService = enrollmentService;
        _learnerProfiles = learnerProfiles;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search, CancellationToken cancellationToken)
    {
        var formations = await _formations.GetPublishedByCategoryAsync(categoryId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            formations = formations
                .Where(f => f.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        ViewBag.Categories = await _categories.GetOrderedWithFormationCountsAsync(cancellationToken);
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.Search = search;

        return View(formations);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        var detail = await _enrollmentService.GetFormationDetailAsync(learnerId.Value, id, cancellationToken);
        return detail is null ? NotFound() : View(detail);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        try
        {
            await _enrollmentService.EnrollAsync(learnerId.Value, id, cancellationToken);
            TempData["ToastMessage"] = "Enrolled! Your apprenticeship in this formation begins now.";
            TempData["ToastType"] = "success";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastMessage"] = ex.Message;
            TempData["ToastType"] = "info";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<int?> ResolveLearnerIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? null : await _learnerProfiles.GetLearnerIdByUserIdAsync(userId, cancellationToken);
    }
}
