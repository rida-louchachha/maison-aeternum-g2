using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetOrderedWithFormationCountsAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(c => c.Formations)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
}
