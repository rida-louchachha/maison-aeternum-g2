namespace MaisonAeternum.Application.AiMentor.Models;

/// <summary>Everything Aurèle needs to know about who she's talking to, in one query.</summary>
public class LearnerContextDto
{
    public int LearnerId { get; set; }
    public string FirstName { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string GuildRankName { get; set; } = default!;
    public int GuildRankLevel { get; set; }
    public int CurrentStreakDays { get; set; }
}
