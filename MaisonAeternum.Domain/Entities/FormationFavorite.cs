namespace MaisonAeternum.Domain.Entities;

public class FormationFavorite
{
    public int LearnerId { get; set; }
    public int FormationId { get; set; }
    public DateTime CreatedAt { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Formation Formation { get; set; } = default!;
}
