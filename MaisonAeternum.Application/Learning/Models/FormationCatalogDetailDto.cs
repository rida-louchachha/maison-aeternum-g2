using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Learning.Models;

/// <summary>The catalog-facing (non-learner-specific) half of a formation's detail page.</summary>
public class FormationCatalogDetailDto
{
    public int FormationId { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string FullDescription { get; set; } = default!;
    public string PrerequisitesText { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string CategoryColorHex { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public DifficultyLevel Difficulty { get; set; }
    public int EstimatedMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public bool OffersCertificate { get; set; }
    public List<ModuleCatalogDto> Modules { get; set; } = new();
}

public class ModuleCatalogDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public int EstimatedMinutes { get; set; }
    public List<ContentItemDto> ContentItems { get; set; } = new();
    public bool HasQuiz { get; set; }
    public int? QuizId { get; set; }
}
