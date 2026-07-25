using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Catalog;

public class FormationService : IFormationService
{
    private readonly IFormationRepository _formations;
    private readonly ICategoryRepository _categories;
    private readonly ITrainerRepository _trainers;

    public FormationService(IFormationRepository formations, ICategoryRepository categories, ITrainerRepository trainers)
    {
        _formations = formations;
        _categories = categories;
        _trainers = trainers;
    }

    public Task<List<FormationAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default) =>
        _formations.GetAllForAdminAsync(cancellationToken);

    public async Task<FormationFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var formation = await _formations.GetWithObjectivesAsync(id, cancellationToken);
        if (formation is null) return null;

        var objectives = formation.Objectives.OrderBy(o => o.DisplayOrder).Select(o => o.Text).ToList();
        while (objectives.Count < 3) objectives.Add(string.Empty);

        return new FormationFormDto
        {
            Id = formation.Id,
            Title = formation.Title,
            CategoryId = formation.CategoryId,
            TrainerId = formation.TrainerId,
            Difficulty = formation.Difficulty,
            EstimatedMinutes = formation.EstimatedMinutes,
            ShortDescription = formation.ShortDescription,
            FullDescription = formation.FullDescription,
            PrerequisitesText = formation.PrerequisitesText,
            HasCertificate = formation.HasCertificate,
            Objectives = objectives
        };
    }

    public async Task<int> CreateAsync(FormationFormDto form, CancellationToken cancellationToken = default)
    {
        var formation = new Formation
        {
            Title = form.Title,
            Slug = Slugify(form.Title),
            CategoryId = form.CategoryId,
            TrainerId = form.TrainerId,
            Difficulty = form.Difficulty,
            EstimatedMinutes = form.EstimatedMinutes,
            ShortDescription = form.ShortDescription,
            FullDescription = form.FullDescription,
            PrerequisitesText = form.PrerequisitesText,
            HasCertificate = form.HasCertificate,
            Status = FormationStatus.Draft,
            EnrollmentCount = 0,
            AverageRating = 0,
            Objectives = BuildObjectives(form.Objectives)
        };

        await _formations.AddAsync(formation, cancellationToken);
        await _formations.SaveChangesAsync(cancellationToken);
        return formation.Id;
    }

    public async Task UpdateAsync(FormationFormDto form, CancellationToken cancellationToken = default)
    {
        var formation = await _formations.GetWithObjectivesAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {form.Id} not found.");

        formation.Title = form.Title;
        formation.Slug = Slugify(form.Title);
        formation.CategoryId = form.CategoryId;
        formation.TrainerId = form.TrainerId;
        formation.Difficulty = form.Difficulty;
        formation.EstimatedMinutes = form.EstimatedMinutes;
        formation.ShortDescription = form.ShortDescription;
        formation.FullDescription = form.FullDescription;
        formation.PrerequisitesText = form.PrerequisitesText;
        formation.HasCertificate = form.HasCertificate;

        formation.Objectives.Clear();
        foreach (var objective in BuildObjectives(form.Objectives))
        {
            formation.Objectives.Add(objective);
        }

        _formations.Update(formation);
        await _formations.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var formation = await _formations.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {id} not found.");

        formation.Status = FormationStatus.Published;
        formation.PublishedAt ??= DateTime.UtcNow;

        _formations.Update(formation);
        await _formations.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var formation = await _formations.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {id} not found.");

        formation.Status = FormationStatus.Archived;

        _formations.Update(formation);
        await _formations.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var formation = await _formations.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {id} not found.");

        _formations.Remove(formation);
        await _formations.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SelectOptionDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categories.ListAllAsync(cancellationToken);
        return categories.OrderBy(c => c.DisplayOrder).Select(c => new SelectOptionDto { Id = c.Id, Name = c.Name }).ToList();
    }

    public async Task<List<SelectOptionDto>> GetTrainerOptionsAsync(CancellationToken cancellationToken = default)
    {
        var trainers = await _trainers.GetAllForAdminAsync(cancellationToken);
        return trainers.Select(t => new SelectOptionDto { Id = t.Id, Name = t.FullName }).ToList();
    }

    private static List<FormationObjective> BuildObjectives(List<string> objectives) =>
        objectives
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select((text, index) => new FormationObjective { Text = text, DisplayOrder = index + 1 })
            .ToList();

    private static string Slugify(string title) =>
        title.ToLowerInvariant().Replace(" & ", "-").Replace(" ", "-").Replace("'", "");
}
