using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class TrainerSocialLink : AuditableEntity
{
    public int TrainerProfileId { get; set; }
    public SocialPlatform Platform { get; set; }
    public string Url { get; set; } = default!;

    public TrainerProfile TrainerProfile { get; set; } = default!;
}
