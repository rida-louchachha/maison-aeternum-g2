namespace MaisonAeternum.Domain.Entities;

public class ModulePrerequisite
{
    public int ModuleId { get; set; }
    public int RequiredModuleId { get; set; }

    public Module Module { get; set; } = default!;
    public Module RequiredModule { get; set; } = default!;
}
