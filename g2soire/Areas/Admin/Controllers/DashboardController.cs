using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardController(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var snapshot = await _dashboardRepository.GetSnapshotAsync(cancellationToken);
        var viewModel = DashboardViewModel.FromSnapshot(snapshot);
        return View(viewModel);
    }
}
