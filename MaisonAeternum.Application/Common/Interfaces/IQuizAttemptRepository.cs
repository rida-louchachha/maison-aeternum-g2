using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IQuizAttemptRepository : IRepository<QuizAttempt>
{
    /// <summary>
    /// Loads a quiz attempt with its quiz, questions, answer options, and the
    /// learner's selected/correct answers — everything Aurèle needs to explain a mistake.
    /// </summary>
    Task<QuizAttempt?> GetWithDetailsAsync(int quizAttemptId, CancellationToken cancellationToken = default);

    Task<List<QuizAttempt>> GetRecentByLearnerAsync(int learnerId, int count, CancellationToken cancellationToken = default);

    Task<int> CountAttemptsAsync(int learnerId, int quizId, CancellationToken cancellationToken = default);
}
