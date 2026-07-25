namespace MaisonAeternum.Application.Catalog.Models;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string IconClass { get; set; } = default!;
    public string ColorHex { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public int FormationCount { get; set; }
}

public class CategoryFormDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string IconClass { get; set; } = default!;
    public string ColorHex { get; set; } = default!;
    public int DisplayOrder { get; set; }
}
