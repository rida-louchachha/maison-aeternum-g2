using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Learning.Models;

public class MyEnrollmentDto
{
    public int Id { get; set; }
    public int FormationId { get; set; }
    public string FormationTitle { get; set; } = default!;
    public string FormationSlug { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string CategoryColorHex { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public EnrollmentStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public class ModuleProgressDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = default!;
    public string ModuleDescription { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsUnlocked { get; set; }
    public List<ContentItemDto> ContentItems { get; set; } = new();
    public bool HasQuiz { get; set; }
    public int? QuizId { get; set; }
}

public class ModulePlayerDto
{
    public int ModuleId { get; set; }
    public int FormationId { get; set; }
    public string FormationTitle { get; set; } = default!;
    public string ModuleTitle { get; set; } = default!;
    public string ModuleDescription { get; set; } = default!;
    public List<ContentItemDto> ContentItems { get; set; } = new();
    public bool IsCompleted { get; set; }
    public bool HasQuiz { get; set; }
    public int? QuizId { get; set; }
}

public class FormationLearnerDetailDto
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
    public bool IsEnrolled { get; set; }
    public EnrollmentStatus? Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public List<ModuleProgressDto> Modules { get; set; } = new();
    public bool HasFinalExam { get; set; }
    public int? FinalExamQuizId { get; set; }
    public bool AllModulesCompleted { get; set; }
    public bool HasCertificate { get; set; }
}
