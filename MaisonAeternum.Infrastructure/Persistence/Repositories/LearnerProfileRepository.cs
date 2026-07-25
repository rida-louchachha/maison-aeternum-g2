using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class LearnerProfileRepository : Repository<LearnerProfile>, ILearnerProfileRepository
{
    public LearnerProfileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<LearnerContextDto?> GetContextAsync(int learnerId, CancellationToken cancellationToken = default) =>
        await (from l in DbSet.AsNoTracking()
               join user in Context.Users.AsNoTracking() on l.UserId equals user.Id
               join rank in Context.GuildRanks.AsNoTracking() on l.GuildRankId equals rank.Id
               where l.Id == learnerId
               select new LearnerContextDto
               {
                   LearnerId = l.Id,
                   FirstName = user.FirstName,
                   DisplayName = user.FirstName + " " + user.LastName,
                   GuildRankName = rank.Name,
                   GuildRankLevel = rank.Level,
                   CurrentStreakDays = l.CurrentStreakDays
               })
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int?> GetLearnerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);
}
