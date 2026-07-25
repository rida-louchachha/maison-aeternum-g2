using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<ActivityLog?> GetForDateAsync(int learnerId, DateOnly date, CancellationToken cancellationToken = default);
}
