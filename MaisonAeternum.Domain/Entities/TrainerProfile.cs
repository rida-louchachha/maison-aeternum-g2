using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class TrainerProfile : AuditableEntity
{
    public string UserId { get; set; } = default!;
    public string Biography { get; set; } = default!;
    public string AtelierAffiliation { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsFeatured { get; set; }

    public ICollection<TrainerSocialLink> SocialLinks { get; set; } = new List<TrainerSocialLink>();
    public ICollection<Formation> Formations { get; set; } = new List<Formation>();
    public ICollection<LiveSession> LiveSessions { get; set; } = new List<LiveSession>();
}
