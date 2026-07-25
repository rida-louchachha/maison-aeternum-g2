using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Formation : AuditableEntity
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public int CategoryId { get; set; }
    public int TrainerId { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int EstimatedMinutes { get; set; }
    public int? CoverImageId { get; set; }
    public string ShortDescription { get; set; } = default!;
    public string FullDescription { get; set; } = default!;
    public string PrerequisitesText { get; set; } = default!;
    public FormationStatus Status { get; set; }
    public bool HasCertificate { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal AverageRating { get; set; }

    public Category Category { get; set; } = default!;
    public TrainerProfile Trainer { get; set; } = default!;
    public MediaFile? CoverImage { get; set; }
    public ICollection<FormationObjective> Objectives { get; set; } = new List<FormationObjective>();
    public ICollection<FormationPrerequisite> Prerequisites { get; set; } = new List<FormationPrerequisite>();
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<FormationFavorite> Favorites { get; set; } = new List<FormationFavorite>();
    public ICollection<FormationReview> Reviews { get; set; } = new List<FormationReview>();
    public ICollection<LiveSession> LiveSessions { get; set; } = new List<LiveSession>();
}
