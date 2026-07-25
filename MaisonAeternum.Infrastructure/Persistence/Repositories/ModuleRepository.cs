using MaisonAeternum.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using DomainModule = MaisonAeternum.Domain.Entities.Module;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class ModuleRepository : Repository<DomainModule>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DomainModule?> GetWithFormationAsync(int moduleId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(m => m.Formation).ThenInclude(f => f.Category)
            .Include(m => m.ContentItems)
            .Include(m => m.Quizzes)
            .FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken);

    public async Task<List<DomainModule>> GetByFormationIdAsync(int formationId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(m => m.ContentItems)
            .Include(m => m.Quizzes)
            .Where(m => m.FormationId == formationId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
}
