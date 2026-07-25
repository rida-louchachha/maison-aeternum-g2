using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaisonAeternum.Web.Areas.Learner.Controllers;

[Area("Learner")]
[Authorize(Roles = "Learner")]
public class ModulesController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILearnerProfileRepository _learnerProfiles;

    public ModulesController(IEnrollmentService enrollmentService, ILearnerProfileRepository learnerProfiles)
    {
        _enrollmentService = enrollmentService;
        _learnerProfiles = learnerProfiles;
    }

    public async Task<IActionResult> Player(int id, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        var player = await _enrollmentService.GetModulePlayerAsync(learnerId.Value, id, cancellationToken);
        return player is null ? NotFound() : View(player);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id, int formationId, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        await _enrollmentService.MarkModuleCompleteAsync(learnerId.Value, id, cancellationToken);

        TempData["ToastMessage"] = "Module completed. Well done.";
        TempData["ToastType"] = "success";
        return RedirectToAction("Details", "Formations", new { id = formationId });
    }

    private async Task<int?> ResolveLearnerIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? null : await _learnerProfiles.GetLearnerIdByUserIdAsync(userId, cancellationToken);
    }
}
