using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Catalog;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categories.GetOrderedWithFormationCountsAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            IconClass = c.IconClass,
            ColorHex = c.ColorHex,
            DisplayOrder = c.DisplayOrder,
            FormationCount = c.Formations.Count
        }).ToList();
    }

    public async Task<CategoryFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken);
        if (category is null) return null;

        return new CategoryFormDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            ColorHex = category.ColorHex,
            DisplayOrder = category.DisplayOrder
        };
    }

    public async Task<int> CreateAsync(CategoryFormDto form, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = form.Name,
            Slug = Slugify(form.Name),
            Description = form.Description,
            IconClass = form.IconClass,
            ColorHex = form.ColorHex,
            DisplayOrder = form.DisplayOrder
        };

        await _categories.AddAsync(category, cancellationToken);
        await _categories.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public async Task UpdateAsync(CategoryFormDto form, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category {form.Id} not found.");

        category.Name = form.Name;
        category.Slug = Slugify(form.Name);
        category.Description = form.Description;
        category.IconClass = form.IconClass;
        category.ColorHex = form.ColorHex;
        category.DisplayOrder = form.DisplayOrder;

        _categories.Update(category);
        await _categories.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        _categories.Remove(category); // AuditSaveChangesInterceptor converts this to a soft delete
        await _categories.SaveChangesAsync(cancellationToken);
    }

    private static string Slugify(string name) =>
        name.ToLowerInvariant().Replace(" & ", "-").Replace(" ", "-").Replace("'", "");
}
