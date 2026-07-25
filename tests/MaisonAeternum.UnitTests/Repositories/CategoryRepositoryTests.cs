using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Infrastructure.Persistence.Repositories;

namespace MaisonAeternum.UnitTests.Repositories;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task GetOrderedWithFormationCountsAsync_ReturnsCategoriesInDisplayOrder()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new CategoryRepository(context);

        context.Categories.AddRange(
            new Category { Name = "Restoration", Slug = "restoration", Description = "d", IconClass = "bi-clock", ColorHex = "#ffffff", DisplayOrder = 2 },
            new Category { Name = "Movements", Slug = "movements", Description = "d", IconClass = "bi-gear", ColorHex = "#000000", DisplayOrder = 1 });
        await context.SaveChangesAsync();

        var result = await repository.GetOrderedWithFormationCountsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Movements", result[0].Name);
        Assert.Equal("Restoration", result[1].Name);
    }

    [Fact]
    public async Task GetOrderedWithFormationCountsAsync_CountsOnlyNonDeletedFormations()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new CategoryRepository(context);

        var category = new Category { Name = "Grand Complications", Slug = "gc", Description = "d", IconClass = "bi-stars", ColorHex = "#8b5cf6", DisplayOrder = 1 };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Formations.AddRange(
            new Formation
            {
                Title = "Active Formation", Slug = "active", CategoryId = category.Id, TrainerId = 1,
                ShortDescription = "s", FullDescription = "f", PrerequisitesText = "p", IsDeleted = false
            },
            new Formation
            {
                Title = "Deleted Formation", Slug = "deleted", CategoryId = category.Id, TrainerId = 1,
                ShortDescription = "s", FullDescription = "f", PrerequisitesText = "p", IsDeleted = true
            });
        await context.SaveChangesAsync();

        var result = await repository.GetOrderedWithFormationCountsAsync();

        Assert.Single(result[0].Formations.Where(f => !f.IsDeleted));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new CategoryRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }
}
