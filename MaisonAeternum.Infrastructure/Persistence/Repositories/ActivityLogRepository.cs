using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ActivityLog?> GetForDateAsync(int learnerId, DateOnly date, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(a => a.LearnerId == learnerId && a.ActivityDate == date, cancellationToken);
}
