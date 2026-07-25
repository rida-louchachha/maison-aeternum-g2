using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaisonAeternum.Web.Areas.Learner.Controllers;

[Area("Learner")]
[Authorize(Roles = "Learner")]
public class DashboardController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILearnerProfileRepository _learnerProfiles;

    public DashboardController(IEnrollmentService enrollmentService, ILearnerProfileRepository learnerProfiles)
    {
        _enrollmentService = enrollmentService;
        _learnerProfiles = learnerProfiles;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        var learnerId = await _learnerProfiles.GetLearnerIdByUserIdAsync(userId, cancellationToken);
        if (learnerId is null) return Forbid();

        var learnerContext = await _learnerProfiles.GetContextAsync(learnerId.Value, cancellationToken);
        ViewBag.LearnerContext = learnerContext;

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(learnerId.Value, cancellationToken);
        return View(enrollments);
    }
}
