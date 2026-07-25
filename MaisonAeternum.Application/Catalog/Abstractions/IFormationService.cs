using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Abstractions;

public interface IFormationService
{
    Task<List<FormationAdminDto>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
    Task<FormationFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(FormationFormDto form, CancellationToken cancellationToken = default);
    Task UpdateAsync(FormationFormDto form, CancellationToken cancellationToken = default);
    Task PublishAsync(int id, CancellationToken cancellationToken = default);
    Task ArchiveAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<List<SelectOptionDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken = default);
    Task<List<SelectOptionDto>> GetTrainerOptionsAsync(CancellationToken cancellationToken = default);
}
