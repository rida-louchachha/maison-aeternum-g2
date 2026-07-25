using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<Quiz?> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetFinalExamByFormationIdAsync(int formationId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetWithQuestionsAsync(int quizId, CancellationToken cancellationToken = default);
    Task<Question?> GetQuestionWithOptionsAsync(int questionId, CancellationToken cancellationToken = default);
}
