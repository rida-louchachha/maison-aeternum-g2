using MaisonAeternum.Application.Common.Models;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
