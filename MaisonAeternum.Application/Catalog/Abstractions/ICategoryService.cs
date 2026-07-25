using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Abstractions;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CategoryFormDto form, CancellationToken cancellationToken = default);
    Task UpdateAsync(CategoryFormDto form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
