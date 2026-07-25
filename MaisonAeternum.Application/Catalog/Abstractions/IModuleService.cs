using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Abstractions;

public interface IModuleService
{
    Task<List<ModuleAdminDto>> GetByFormationAsync(int formationId, CancellationToken cancellationToken = default);
    Task<ModuleFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ModuleFormDto form, CancellationToken cancellationToken = default);
    Task UpdateAsync(ModuleFormDto form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<List<ContentItemDto>> GetContentItemsAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<ContentItemFormDto?> GetContentItemForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateContentItemAsync(ContentItemFormDto form, CancellationToken cancellationToken = default);
    Task UpdateContentItemAsync(ContentItemFormDto form, CancellationToken cancellationToken = default);
    Task DeleteContentItemAsync(int id, CancellationToken cancellationToken = default);
}
