using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning.Models;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Enrollment>> GetByLearnerWithDetailsAsync(int learnerId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(e => e.Formation).ThenInclude(f => f.Modules)
            .Include(e => e.ModuleProgresses).ThenInclude(mp => mp.Module)
            .Where(e => e.LearnerId == learnerId)
            .ToListAsync(cancellationToken);

    public async Task<Enrollment?> GetByLearnerAndFormationAsync(int learnerId, int formationId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.ModuleProgresses)
            .Include(e => e.Formation).ThenInclude(f => f.Modules)
            .FirstOrDefaultAsync(e => e.LearnerId == learnerId && e.FormationId == formationId, cancellationToken);

    public async Task<List<MyEnrollmentDto>> GetMyEnrollmentSummariesAsync(int learnerId, CancellationToken cancellationToken = default) =>
        await (from e in DbSet.AsNoTracking()
               join user in Context.Users.AsNoTracking() on e.Formation.Trainer.UserId equals user.Id
               where e.LearnerId == learnerId
               orderby e.LastAccessedAt descending
               select new MyEnrollmentDto
               {
                   Id = e.Id,
                   FormationId = e.FormationId,
                   FormationTitle = e.Formation.Title,
                   FormationSlug = e.Formation.Slug,
                   CategoryName = e.Formation.Category.Name,
                   CategoryColorHex = e.Formation.Category.ColorHex,
                   TrainerName = user.FirstName + " " + user.LastName,
                   Status = e.Status,
                   ProgressPercentage = e.ProgressPercentage,
                   EnrolledAt = e.EnrolledAt,
                   LastAccessedAt = e.LastAccessedAt
               })
            .ToListAsync(cancellationToken);
}
