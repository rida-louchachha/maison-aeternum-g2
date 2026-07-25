using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Badge : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string IconUrl { get; set; } = default!;
    public BadgeCategory Category { get; set; }
    public string CriteriaDescription { get; set; } = default!;

    public ICollection<LearnerBadge> LearnerBadges { get; set; } = new List<LearnerBadge>();
}
