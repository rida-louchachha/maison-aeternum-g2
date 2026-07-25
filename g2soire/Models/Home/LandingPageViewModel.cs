namespace MaisonAeternum.Web.Models.Home;

public class LandingPageViewModel
{
    public List<FormationCardViewModel> FeaturedFormations { get; set; } = new();
    public List<CategoryChipViewModel> Categories { get; set; } = new();
    public List<TrainerHighlightViewModel> FeaturedTrainers { get; set; } = new();
    public int TotalLearners { get; set; }
    public int TotalFormations { get; set; }
    public int CertificatesIssued { get; set; }
}

public class FormationCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string CategoryColorHex { get; set; } = default!;
    public string CategoryIconClass { get; set; } = default!;
    public string TrainerName { get; set; } = default!;
    public string DifficultyLabel { get; set; } = default!;
    public int EstimatedMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public int EnrollmentCount { get; set; }
}

public class CategoryChipViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string IconClass { get; set; } = default!;
    public string ColorHex { get; set; } = default!;
    public int FormationCount { get; set; }
}

public class TrainerHighlightViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Biography { get; set; } = default!;
    public string AtelierAffiliation { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public decimal AverageRating { get; set; }
    public int FormationCount { get; set; }
    public string Initials { get; set; } = default!;
}
