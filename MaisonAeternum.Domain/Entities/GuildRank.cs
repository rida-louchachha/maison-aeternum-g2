using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class GuildRank : AuditableEntity
{
    public string Name { get; set; } = default!;
    public int Level { get; set; }
    public int MinFormationsCompleted { get; set; }
    public string BadgeIconUrl { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ICollection<LearnerProfile> LearnerProfiles { get; set; } = new List<LearnerProfile>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
