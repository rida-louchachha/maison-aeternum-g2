using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Catalog.Models;

public class FormationAdminDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public DifficultyLevel Difficulty { get; set; }
    public FormationStatus Status { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ModuleCount { get; set; }
}

public class FormationFormDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public int CategoryId { get; set; }
    public int TrainerId { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int EstimatedMinutes { get; set; }
    public string ShortDescription { get; set; } = default!;
    public string FullDescription { get; set; } = default!;
    public string PrerequisitesText { get; set; } = default!;
    public bool HasCertificate { get; set; } = true;
    public List<string> Objectives { get; set; } = new() { "", "", "" };
}

public class SelectOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}
