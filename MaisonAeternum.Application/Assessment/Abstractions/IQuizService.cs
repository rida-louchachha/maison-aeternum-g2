using MaisonAeternum.Application.Assessment.Models;

namespace MaisonAeternum.Application.Assessment.Abstractions;

public interface IQuizService
{
    Task<QuizDto?> GetForModuleAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<QuizDto?> GetFinalExamForFormationAsync(int formationId, CancellationToken cancellationToken = default);
    Task<QuizFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateForModuleAsync(int moduleId, int formationId, QuizFormDto form, CancellationToken cancellationToken = default);
    Task<int> CreateFinalExamAsync(int formationId, QuizFormDto form, CancellationToken cancellationToken = default);
    Task UpdateAsync(QuizFormDto form, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<List<QuestionAdminDto>> GetQuestionsAsync(int quizId, CancellationToken cancellationToken = default);
    Task<QuestionFormDto?> GetQuestionForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateQuestionAsync(QuestionFormDto form, CancellationToken cancellationToken = default);
    Task UpdateQuestionAsync(QuestionFormDto form, CancellationToken cancellationToken = default);
    Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default);
}
