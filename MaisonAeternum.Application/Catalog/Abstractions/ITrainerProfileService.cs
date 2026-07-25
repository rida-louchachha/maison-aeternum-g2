using MaisonAeternum.Application.Catalog.Models;

namespace MaisonAeternum.Application.Catalog.Abstractions;

public interface ITrainerProfileService
{
    Task<List<TrainerAdminDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TrainerProfileFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates the TrainerProfile for an Identity user that the caller has already created (see TrainersController).</summary>
    Task<int> CreateAsync(string userId, TrainerProfileFormDto form, CancellationToken cancellationToken = default);

    Task UpdateAsync(TrainerProfileFormDto form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
