using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Services;

public class AccountProvisioningService : IAccountProvisioningService
{
    private readonly IRepository<LearnerProfile> _learnerProfiles;
    private readonly IRepository<GuildRank> _guildRanks;

    public AccountProvisioningService(IRepository<LearnerProfile> learnerProfiles, IRepository<GuildRank> guildRanks)
    {
        _learnerProfiles = learnerProfiles;
        _guildRanks = guildRanks;
    }

    public async Task CreateLearnerProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var ranks = await _guildRanks.ListAllAsync(cancellationToken);
        var apprenticeRank = ranks.OrderBy(r => r.Level).First();

        var existingLearners = await _learnerProfiles.ListAllAsync(cancellationToken);
        var memberNumber = $"MA-{DateTime.UtcNow.Year}-{existingLearners.Count + 1:D6}";

        var profile = new LearnerProfile
        {
            UserId = userId,
            GuildRankId = apprenticeRank.Id,
            MemberNumber = memberNumber,
            MemberSince = DateTime.UtcNow,
            CurrentStreakDays = 0,
            LongestStreakDays = 0,
            TotalBenchMinutes = 0
        };

        await _learnerProfiles.AddAsync(profile, cancellationToken);
        await _learnerProfiles.SaveChangesAsync(cancellationToken);
    }
}
