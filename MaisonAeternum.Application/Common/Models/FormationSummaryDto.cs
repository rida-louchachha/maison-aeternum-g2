using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Common.Models;

public class FormationSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string CategoryColorHex { get; set; } = default!;
    public string CategoryIconClass { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public DifficultyLevel Difficulty { get; set; }
    public int EstimatedMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public int EnrollmentCount { get; set; }
}
