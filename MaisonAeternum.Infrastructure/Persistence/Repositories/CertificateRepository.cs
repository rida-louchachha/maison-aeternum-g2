using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class CertificateRepository : Repository<Certificate>, ICertificateRepository
{
    public CertificateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsForLearnerAndFormationAsync(int learnerId, int formationId, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(c => c.LearnerId == learnerId && c.FormationId == formationId, cancellationToken);

    public async Task<int> CountByLearnerAsync(int learnerId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(c => c.LearnerId == learnerId && !c.IsRevoked, cancellationToken);

    public async Task<bool> ExistsForAttemptAsync(int quizAttemptId, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(c => c.QuizAttemptId == quizAttemptId, cancellationToken);
}
