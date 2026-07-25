using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class FormationObjective : AuditableEntity
{
    public int FormationId { get; set; }
    public string Text { get; set; } = default!;
    public int DisplayOrder { get; set; }

    public Formation Formation { get; set; } = default!;
}
