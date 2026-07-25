using MaisonAeternum.Application.Assessment.Abstractions;
using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaisonAeternum.Web.Areas.Learner.Controllers;

[Area("Learner")]
[Authorize(Roles = "Learner")]
public class QuizzesController : Controller
{
    private readonly IQuizAttemptService _quizAttemptService;
    private readonly ILearnerProfileRepository _learnerProfiles;

    public QuizzesController(IQuizAttemptService quizAttemptService, ILearnerProfileRepository learnerProfiles)
    {
        _quizAttemptService = quizAttemptService;
        _learnerProfiles = learnerProfiles;
    }

    public async Task<IActionResult> Take(int quizId, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        try
        {
            var start = await _quizAttemptService.StartAttemptAsync(learnerId.Value, quizId, cancellationToken);
            return View(start);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastMessage"] = ex.Message;
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitQuizAttemptDto submission, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        var result = await _quizAttemptService.SubmitAttemptAsync(learnerId.Value, submission, cancellationToken);

        // NewGuildRankName is a one-time "you were just promoted" notice — it can't be re-derived
        // from persisted state the way CertificateIssued can, so it rides across the redirect in
        // TempData rather than being recomputed (and incorrectly re-shown) on every later revisit.
        if (!string.IsNullOrEmpty(result.NewGuildRankName))
        {
            TempData["NewGuildRankName"] = result.NewGuildRankName;
        }

        return RedirectToAction(nameof(Result), new { attemptId = result.AttemptId });
    }

    public async Task<IActionResult> Result(int attemptId, CancellationToken cancellationToken)
    {
        var learnerId = await ResolveLearnerIdAsync(cancellationToken);
        if (learnerId is null) return Forbid();

        var result = await _quizAttemptService.GetResultAsync(learnerId.Value, attemptId, cancellationToken);
        if (result is null) return NotFound();

        result.NewGuildRankName = TempData["NewGuildRankName"] as string;
        return View(result);
    }

    private async Task<int?> ResolveLearnerIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId) ? null : await _learnerProfiles.GetLearnerIdByUserIdAsync(userId, cancellationToken);
    }
}
