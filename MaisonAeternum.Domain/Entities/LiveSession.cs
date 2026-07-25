using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class LiveSession : AuditableEntity
{
    public int? FormationId { get; set; }
    public int TrainerId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? MeetingUrl { get; set; }

    public Formation? Formation { get; set; }
    public TrainerProfile Trainer { get; set; } = default!;
}
