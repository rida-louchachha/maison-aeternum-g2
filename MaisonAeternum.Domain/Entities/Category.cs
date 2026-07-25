using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class Category : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string IconClass { get; set; } = default!;
    public string ColorHex { get; set; } = default!;
    public int DisplayOrder { get; set; }

    public ICollection<Formation> Formations { get; set; } = new List<Formation>();
}
