namespace MaisonAeternum.Application.Common.Models;

public class TrainerHighlightDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Biography { get; set; } = default!;
    public string AtelierAffiliation { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public decimal AverageRating { get; set; }
    public int FormationCount { get; set; }
}
