using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetOrderedWithFormationCountsAsync(CancellationToken cancellationToken = default);
}
