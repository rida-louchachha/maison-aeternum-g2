using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface ITrainerRepository : IRepository<TrainerProfile>
{
    Task<List<TrainerHighlightDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);

    Task<List<TrainerAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);

    Task<TrainerProfile?> GetWithSocialLinksAsync(int id, CancellationToken cancellationToken = default);
}
