using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Catalog.Models;

public class TrainerAdminDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string AtelierAffiliation { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsFeatured { get; set; }
    public int FormationCount { get; set; }
}

public class TrainerProfileFormDto
{
    public int Id { get; set; }
    public string Biography { get; set; } = default!;
    public string AtelierAffiliation { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public bool IsFeatured { get; set; }
    public List<TrainerSocialLinkFormDto> SocialLinks { get; set; } = new();
}

public class TrainerSocialLinkFormDto
{
    public SocialPlatform Platform { get; set; }
    public string Url { get; set; } = default!;
}

public class CreateTrainerDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public TrainerProfileFormDto Profile { get; set; } = default!;
}
