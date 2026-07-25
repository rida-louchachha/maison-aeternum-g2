using AutoMapper;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using MaisonAeternum.Web.Models.Home;

namespace MaisonAeternum.Web.Mapping;

public class WebMappingProfile : Profile
{
    public WebMappingProfile()
    {
        CreateMap<FormationSummaryDto, FormationCardViewModel>()
            .ForMember(d => d.DifficultyLabel, opt => opt.MapFrom(s => HumanizeDifficulty(s.Difficulty)));

        CreateMap<Category, CategoryChipViewModel>()
            .ForMember(d => d.FormationCount, opt => opt.MapFrom(s => s.Formations.Count));

        CreateMap<TrainerHighlightDto, TrainerHighlightViewModel>()
            .ForMember(d => d.Initials, opt => opt.MapFrom(s => Initials(s.FullName)));
    }

    private static string HumanizeDifficulty(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Apprentice => "Apprentice",
        DifficultyLevel.Journeyman => "Journeyman",
        DifficultyLevel.CertifiedHorologer => "Certified Horologer",
        DifficultyLevel.MasterOfTheMaison => "Master of the Maison",
        _ => level.ToString()
    };

    private static string Initials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant() : fullName[..Math.Min(2, fullName.Length)].ToUpperInvariant();
    }
}
