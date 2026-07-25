using MaisonAeternum.Application.Learning.Models;
using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    /// <summary>
    /// Enrollments for a learner with formation, modules, and per-module progress loaded —
    /// the data Aurèle needs to recommend the next best step.
    /// </summary>
    Task<List<Enrollment>> GetByLearnerWithDetailsAsync(int learnerId, CancellationToken cancellationToken = default);

    /// <summary>Tracked — used when the enrollment/progress needs to be mutated (enroll, mark module complete).</summary>
    Task<Enrollment?> GetByLearnerAndFormationAsync(int learnerId, int formationId, CancellationToken cancellationToken = default);

    /// <summary>"My Formations" list — enrollment + category + trainer name, ready to display.</summary>
    Task<List<MyEnrollmentDto>> GetMyEnrollmentSummariesAsync(int learnerId, CancellationToken cancellationToken = default);
}
