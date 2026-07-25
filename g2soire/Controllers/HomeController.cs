using AutoMapper;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Web.Models;
using MaisonAeternum.Web.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace MaisonAeternum.Web.Controllers;

public class HomeController : Controller
{
    private readonly IFormationRepository _formationRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IMapper _mapper;

    public HomeController(
        IFormationRepository formationRepository,
        ICategoryRepository categoryRepository,
        ITrainerRepository trainerRepository,
        IDashboardRepository dashboardRepository,
        IMapper mapper)
    {
        _formationRepository = formationRepository;
        _categoryRepository = categoryRepository;
        _trainerRepository = trainerRepository;
        _dashboardRepository = dashboardRepository;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var featuredFormations = await _formationRepository.GetFeaturedPublishedAsync(6, cancellationToken);
        var categories = await _categoryRepository.GetOrderedWithFormationCountsAsync(cancellationToken);
        var featuredTrainers = await _trainerRepository.GetFeaturedAsync(3, cancellationToken);
        var snapshot = await _dashboardRepository.GetSnapshotAsync(cancellationToken);

        var viewModel = new LandingPageViewModel
        {
            FeaturedFormations = _mapper.Map<List<FormationCardViewModel>>(featuredFormations),
            Categories = _mapper.Map<List<CategoryChipViewModel>>(categories),
            FeaturedTrainers = _mapper.Map<List<TrainerHighlightViewModel>>(featuredTrainers),
            TotalLearners = snapshot.TotalLearners,
            TotalFormations = snapshot.TotalFormations,
            CertificatesIssued = snapshot.CertificatesIssued
        };

        return View(viewModel);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
