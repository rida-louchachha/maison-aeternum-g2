namespace MaisonAeternum.Domain.Entities;

public class FormationPrerequisite
{
    public int FormationId { get; set; }
    public int RequiredFormationId { get; set; }

    public Formation Formation { get; set; } = default!;
    public Formation RequiredFormation { get; set; } = default!;
}
