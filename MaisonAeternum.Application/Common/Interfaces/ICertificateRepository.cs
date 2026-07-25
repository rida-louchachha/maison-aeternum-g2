using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface ICertificateRepository : IRepository<Certificate>
{
    Task<bool> ExistsForLearnerAndFormationAsync(int learnerId, int formationId, CancellationToken cancellationToken = default);
    Task<int> CountByLearnerAsync(int learnerId, CancellationToken cancellationToken = default);

    /// <summary>True if this specific attempt is the one that earned the formation's certificate — correct on first view and any later revisit.</summary>
    Task<bool> ExistsForAttemptAsync(int quizAttemptId, CancellationToken cancellationToken = default);
}
