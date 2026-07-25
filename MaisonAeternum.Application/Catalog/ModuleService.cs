using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using DomainModule = MaisonAeternum.Domain.Entities.Module;

namespace MaisonAeternum.Application.Catalog;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _modules;
    private readonly IRepository<ContentItem> _contentItems;

    public ModuleService(IModuleRepository modules, IRepository<ContentItem> contentItems)
    {
        _modules = modules;
        _contentItems = contentItems;
    }

    public async Task<List<ModuleAdminDto>> GetByFormationAsync(int formationId, CancellationToken cancellationToken = default)
    {
        var modules = await _modules.GetByFormationIdAsync(formationId, cancellationToken);

        return modules.Select(m => new ModuleAdminDto
        {
            Id = m.Id,
            FormationId = m.FormationId,
            Title = m.Title,
            DisplayOrder = m.DisplayOrder,
            EstimatedMinutes = m.EstimatedMinutes,
            ContentItemCount = m.ContentItems.Count,
            HasQuiz = m.Quizzes.Any()
        }).ToList();
    }

    public async Task<ModuleFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetByIdAsync(id, cancellationToken);
        if (module is null) return null;

        return new ModuleFormDto
        {
            Id = module.Id,
            FormationId = module.FormationId,
            Title = module.Title,
            Description = module.Description,
            DisplayOrder = module.DisplayOrder,
            EstimatedMinutes = module.EstimatedMinutes
        };
    }

    public async Task<int> CreateAsync(ModuleFormDto form, CancellationToken cancellationToken = default)
    {
        var module = new DomainModule
        {
            FormationId = form.FormationId,
            Title = form.Title,
            Description = form.Description,
            DisplayOrder = form.DisplayOrder,
            EstimatedMinutes = form.EstimatedMinutes
        };

        await _modules.AddAsync(module, cancellationToken);
        await _modules.SaveChangesAsync(cancellationToken);
        return module.Id;
    }

    public async Task UpdateAsync(ModuleFormDto form, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetByIdAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Module {form.Id} not found.");

        module.Title = form.Title;
        module.Description = form.Description;
        module.DisplayOrder = form.DisplayOrder;
        module.EstimatedMinutes = form.EstimatedMinutes;

        _modules.Update(module);
        await _modules.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Module {id} not found.");

        _modules.Remove(module);
        await _modules.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ContentItemDto>> GetContentItemsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetWithFormationAsync(moduleId, cancellationToken);
        if (module is null) return new List<ContentItemDto>();

        return module.ContentItems.OrderBy(c => c.DisplayOrder).Select(c => new ContentItemDto
        {
            Id = c.Id,
            ModuleId = c.ModuleId,
            Type = c.Type,
            Title = c.Title,
            ExternalUrl = c.ExternalUrl,
            DurationMinutes = c.DurationMinutes,
            DisplayOrder = c.DisplayOrder
        }).ToList();
    }

    public async Task<ContentItemFormDto?> GetContentItemForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _contentItems.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;

        return new ContentItemFormDto
        {
            Id = item.Id,
            ModuleId = item.ModuleId,
            Type = item.Type,
            Title = item.Title,
            ExternalUrl = item.ExternalUrl ?? string.Empty,
            DurationMinutes = item.DurationMinutes,
            DisplayOrder = item.DisplayOrder
        };
    }

    public async Task<int> CreateContentItemAsync(ContentItemFormDto form, CancellationToken cancellationToken = default)
    {
        var item = new ContentItem
        {
            ModuleId = form.ModuleId,
            Type = form.Type,
            Title = form.Title,
            ExternalUrl = form.ExternalUrl,
            DurationMinutes = form.DurationMinutes,
            DisplayOrder = form.DisplayOrder
        };

        await _contentItems.AddAsync(item, cancellationToken);
        await _contentItems.SaveChangesAsync(cancellationToken);
        return item.Id;
    }

    public async Task UpdateContentItemAsync(ContentItemFormDto form, CancellationToken cancellationToken = default)
    {
        var item = await _contentItems.GetByIdAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content item {form.Id} not found.");

        item.Type = form.Type;
        item.Title = form.Title;
        item.ExternalUrl = form.ExternalUrl;
        item.DurationMinutes = form.DurationMinutes;
        item.DisplayOrder = form.DisplayOrder;

        _contentItems.Update(item);
        await _contentItems.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteContentItemAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _contentItems.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content item {id} not found.");

        _contentItems.Remove(item);
        await _contentItems.SaveChangesAsync(cancellationToken);
    }
}
