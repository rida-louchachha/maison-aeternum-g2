using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface ILearnerProfileRepository : IRepository<LearnerProfile>
{
    /// <summary>Learner + display name (joined from Identity) + guild rank, in one query — Aurèle's greeting context.</summary>
    Task<LearnerContextDto?> GetContextAsync(int learnerId, CancellationToken cancellationToken = default);

    /// <summary>Resolves the LearnerProfile.Id for the currently authenticated Identity user, if any.</summary>
    Task<int?> GetLearnerIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
