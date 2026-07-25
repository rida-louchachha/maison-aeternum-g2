using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Catalog.Models;

public class ModuleAdminDto
{
    public int Id { get; set; }
    public int FormationId { get; set; }
    public string Title { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public int EstimatedMinutes { get; set; }
    public int ContentItemCount { get; set; }
    public bool HasQuiz { get; set; }
}

public class ModuleFormDto
{
    public int Id { get; set; }
    public int FormationId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DisplayOrder { get; set; } = 1;
    public int EstimatedMinutes { get; set; } = 30;
}

public class ContentItemDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public ContentItemType Type { get; set; }
    public string Title { get; set; } = default!;
    public string? ExternalUrl { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContentItemFormDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public ContentItemType Type { get; set; }
    public string Title { get; set; } = default!;
    public string ExternalUrl { get; set; } = default!;
    public int DurationMinutes { get; set; } = 10;
    public int DisplayOrder { get; set; } = 1;
}
