using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class Module : AuditableEntity
{
    public int FormationId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public int EstimatedMinutes { get; set; }

    public Formation Formation { get; set; } = default!;
    public ICollection<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<ModuleProgress> ModuleProgresses { get; set; } = new List<ModuleProgress>();
}
