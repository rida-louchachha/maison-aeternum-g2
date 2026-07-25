using MaisonAeternum.Application.Catalog;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Moq;

namespace MaisonAeternum.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_categories.Object);
    }

    [Fact]
    public async Task CreateAsync_SlugifiesTheName()
    {
        Category? captured = null;
        _categories.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);

        var form = new CategoryFormDto { Name = "Grand Complications & Rare Movements", Description = "d", IconClass = "bi-stars", ColorHex = "#8b5cf6", DisplayOrder = 1 };

        await _sut.CreateAsync(form);

        Assert.NotNull(captured);
        Assert.Equal("grand-complications-rare-movements", captured!.Slug);
        _categories.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoryDoesNotExist()
    {
        _categories.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var form = new CategoryFormDto { Id = 42, Name = "x", Description = "d", IconClass = "bi-x", ColorHex = "#000000" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(form));
    }

    [Fact]
    public async Task UpdateAsync_AppliesFormValuesToTheTrackedEntity()
    {
        var existing = new Category { Id = 5, Name = "Old", Slug = "old", Description = "old", IconClass = "bi-old", ColorHex = "#111111", DisplayOrder = 9 };
        _categories.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var form = new CategoryFormDto { Id = 5, Name = "New Name", Description = "new", IconClass = "bi-new", ColorHex = "#abcabc", DisplayOrder = 3 };

        await _sut.UpdateAsync(form);

        Assert.Equal("New Name", existing.Name);
        Assert.Equal("new-name", existing.Slug);
        Assert.Equal(3, existing.DisplayOrder);
        _categories.Verify(r => r.Update(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenCategoryDoesNotExist()
    {
        _categories.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(1));
    }
}
