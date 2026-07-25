using MaisonAeternum.Application.Learning.Models;

namespace MaisonAeternum.Application.Learning.Abstractions;

public interface IEnrollmentService
{
    Task<List<MyEnrollmentDto>> GetMyEnrollmentsAsync(int learnerId, CancellationToken cancellationToken = default);

    Task<FormationLearnerDetailDto?> GetFormationDetailAsync(int learnerId, int formationId, CancellationToken cancellationToken = default);

    Task<ModulePlayerDto?> GetModulePlayerAsync(int learnerId, int moduleId, CancellationToken cancellationToken = default);

    Task EnrollAsync(int learnerId, int formationId, CancellationToken cancellationToken = default);

    /// <summary>Marks a module complete for the learner, recomputes enrollment progress, and updates the activity streak.</summary>
    Task MarkModuleCompleteAsync(int learnerId, int moduleId, CancellationToken cancellationToken = default);
}
