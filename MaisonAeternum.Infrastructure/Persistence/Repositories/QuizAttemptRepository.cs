using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
{
    public QuizAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<QuizAttempt?> GetWithDetailsAsync(int quizAttemptId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(a => a.Quiz).ThenInclude(q => q.Formation)
            .Include(a => a.Learner)
            .Include(a => a.Answers).ThenInclude(ans => ans.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Answers).ThenInclude(ans => ans.SelectedOptions).ThenInclude(so => so.AnswerOption)
            .FirstOrDefaultAsync(a => a.Id == quizAttemptId, cancellationToken);

    public async Task<List<QuizAttempt>> GetRecentByLearnerAsync(int learnerId, int count, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(a => a.Quiz)
            .Where(a => a.LearnerId == learnerId && a.SubmittedAt != null)
            .OrderByDescending(a => a.SubmittedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<int> CountAttemptsAsync(int learnerId, int quizId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(a => a.LearnerId == learnerId && a.QuizId == quizId, cancellationToken);
}
