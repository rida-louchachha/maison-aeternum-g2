using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Application.Learning.Models;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IFormationRepository : IRepository<Formation>
{
    Task<List<FormationSummaryDto>> GetFeaturedPublishedAsync(int count, CancellationToken cancellationToken = default);
    Task<List<FormationSummaryDto>> GetPublishedByCategoryAsync(int? categoryId, CancellationToken cancellationToken = default);

    Task<List<FormationAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
    Task<Formation?> GetWithObjectivesAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>The catalog-facing half of a formation's learner detail page (category, trainer name, modules, content, quizzes).</summary>
    Task<FormationCatalogDetailDto?> GetCatalogDetailAsync(int formationId, CancellationToken cancellationToken = default);
}
