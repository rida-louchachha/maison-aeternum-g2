using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Quiz?> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.ModuleId == moduleId, cancellationToken);

    public async Task<Quiz?> GetFinalExamByFormationIdAsync(int formationId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.FormationId == formationId && q.Type == QuizType.FinalExam, cancellationToken);

    public async Task<Quiz?> GetWithQuestionsAsync(int quizId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(q => q.Questions.OrderBy(qu => qu.DisplayOrder)).ThenInclude(qu => qu.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);

    public async Task<Question?> GetQuestionWithOptionsAsync(int questionId, CancellationToken cancellationToken = default) =>
        await Context.Questions.Include(q => q.AnswerOptions).FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
}
