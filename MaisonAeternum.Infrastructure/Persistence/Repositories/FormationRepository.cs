using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Application.Learning.Models;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class FormationRepository : Repository<Formation>, IFormationRepository
{
    public FormationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<FormationSummaryDto>> GetFeaturedPublishedAsync(int count, CancellationToken cancellationToken = default) =>
        await ProjectToSummary(DbSet.Where(f => f.Status == FormationStatus.Published)
                .OrderByDescending(f => f.AverageRating)
                .ThenByDescending(f => f.EnrollmentCount)
                .Take(count))
            .ToListAsync(cancellationToken);

    public async Task<List<FormationSummaryDto>> GetPublishedByCategoryAsync(int? categoryId, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(f => f.Status == FormationStatus.Published);

        if (categoryId.HasValue)
            query = query.Where(f => f.CategoryId == categoryId.Value);

        return await ProjectToSummary(query.OrderBy(f => f.Title)).ToListAsync(cancellationToken);
    }

    public async Task<List<FormationAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default) =>
        await (from f in DbSet.AsNoTracking()
               join user in Context.Users.AsNoTracking() on f.Trainer.UserId equals user.Id
               orderby f.Title
               select new FormationAdminDto
               {
                   Id = f.Id,
                   Title = f.Title,
                   CategoryName = f.Category.Name,
                   TrainerName = user.FirstName + " " + user.LastName,
                   Difficulty = f.Difficulty,
                   Status = f.Status,
                   EnrollmentCount = f.EnrollmentCount,
                   AverageRating = f.AverageRating,
                   ModuleCount = f.Modules.Count
               })
            .ToListAsync(cancellationToken);

    public async Task<FormationCatalogDetailDto?> GetCatalogDetailAsync(int formationId, CancellationToken cancellationToken = default)
    {
        var detail = await (from f in DbSet.AsNoTracking()
                             join user in Context.Users.AsNoTracking() on f.Trainer.UserId equals user.Id
                             where f.Id == formationId
                             select new FormationCatalogDetailDto
                             {
                                 FormationId = f.Id,
                                 Title = f.Title,
                                 Slug = f.Slug,
                                 ShortDescription = f.ShortDescription,
                                 FullDescription = f.FullDescription,
                                 PrerequisitesText = f.PrerequisitesText,
                                 CategoryName = f.Category.Name,
                                 CategoryColorHex = f.Category.ColorHex,
                                 TrainerName = user.FirstName + " " + user.LastName,
                                 Difficulty = f.Difficulty,
                                 EstimatedMinutes = f.EstimatedMinutes,
                                 AverageRating = f.AverageRating,
                                 OffersCertificate = f.HasCertificate
                             })
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null) return null;

        var modules = await Context.Modules.AsNoTracking()
            .Include(m => m.ContentItems)
            .Include(m => m.Quizzes)
            .Where(m => m.FormationId == formationId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        detail.Modules = modules.Select(m => new ModuleCatalogDto
        {
            ModuleId = m.Id,
            Title = m.Title,
            Description = m.Description,
            DisplayOrder = m.DisplayOrder,
            EstimatedMinutes = m.EstimatedMinutes,
            ContentItems = m.ContentItems.OrderBy(c => c.DisplayOrder).Select(c => new ContentItemDto
            {
                Id = c.Id,
                ModuleId = c.ModuleId,
                Type = c.Type,
                Title = c.Title,
                ExternalUrl = c.ExternalUrl,
                DurationMinutes = c.DurationMinutes,
                DisplayOrder = c.DisplayOrder
            }).ToList(),
            HasQuiz = m.Quizzes.Any(),
            QuizId = m.Quizzes.Select(q => (int?)q.Id).FirstOrDefault()
        }).ToList();

        return detail;
    }

    public async Task<Formation?> GetWithObjectivesAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(f => f.Objectives).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    private IQueryable<FormationSummaryDto> ProjectToSummary(IQueryable<Formation> query) =>
        from f in query.AsNoTracking()
        join user in Context.Users.AsNoTracking() on f.Trainer.UserId equals user.Id
        select new FormationSummaryDto
        {
            Id = f.Id,
            Title = f.Title,
            Slug = f.Slug,
            ShortDescription = f.ShortDescription,
            CategoryName = f.Category.Name,
            CategoryColorHex = f.Category.ColorHex,
            CategoryIconClass = f.Category.IconClass,
            TrainerName = user.FirstName + " " + user.LastName,
            Difficulty = f.Difficulty,
            EstimatedMinutes = f.EstimatedMinutes,
            AverageRating = f.AverageRating,
            EnrollmentCount = f.EnrollmentCount
        };
}
