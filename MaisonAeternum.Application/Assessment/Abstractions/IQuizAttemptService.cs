using MaisonAeternum.Application.Assessment.Models;

namespace MaisonAeternum.Application.Assessment.Abstractions;

public interface IQuizAttemptService
{
    Task<QuizAttemptStartDto> StartAttemptAsync(int learnerId, int quizId, CancellationToken cancellationToken = default);
    Task<QuizAttemptResultDto> SubmitAttemptAsync(int learnerId, SubmitQuizAttemptDto submission, CancellationToken cancellationToken = default);
    Task<QuizAttemptResultDto?> GetResultAsync(int learnerId, int attemptId, CancellationToken cancellationToken = default);
}
