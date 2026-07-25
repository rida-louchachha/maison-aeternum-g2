using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class TrainerRepository : Repository<TrainerProfile>, ITrainerRepository
{
    public TrainerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<TrainerHighlightDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default) =>
        await (from t in DbSet.AsNoTracking().Where(t => t.IsFeatured)
               join user in Context.Users.AsNoTracking() on t.UserId equals user.Id
               orderby t.AverageRating descending
               select new TrainerHighlightDto
               {
                   Id = t.Id,
                   FullName = user.FirstName + " " + user.LastName,
                   Biography = t.Biography,
                   AtelierAffiliation = t.AtelierAffiliation,
                   YearsOfExperience = t.YearsOfExperience,
                   AverageRating = t.AverageRating,
                   FormationCount = t.Formations.Count
               })
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<List<TrainerAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default) =>
        await (from t in DbSet.AsNoTracking()
               join user in Context.Users.AsNoTracking() on t.UserId equals user.Id
               orderby user.FirstName
               select new TrainerAdminDto
               {
                   Id = t.Id,
                   FullName = user.FirstName + " " + user.LastName,
                   Email = user.Email!,
                   AtelierAffiliation = t.AtelierAffiliation,
                   YearsOfExperience = t.YearsOfExperience,
                   AverageRating = t.AverageRating,
                   IsFeatured = t.IsFeatured,
                   FormationCount = t.Formations.Count
               })
            .ToListAsync(cancellationToken);

    public async Task<TrainerProfile?> GetWithSocialLinksAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(t => t.SocialLinks).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
