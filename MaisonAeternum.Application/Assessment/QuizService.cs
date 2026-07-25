using MaisonAeternum.Application.Assessment.Abstractions;
using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Assessment;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizzes;
    private readonly IRepository<Question> _questions;

    public QuizService(IQuizRepository quizzes, IRepository<Question> questions)
    {
        _quizzes = quizzes;
        _questions = questions;
    }

    public async Task<QuizDto?> GetForModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetByModuleIdAsync(moduleId, cancellationToken);
        return quiz is null ? null : MapToDto(quiz);
    }

    public async Task<QuizDto?> GetFinalExamForFormationAsync(int formationId, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetFinalExamByFormationIdAsync(formationId, cancellationToken);
        return quiz is null ? null : MapToDto(quiz);
    }

    public async Task<QuizFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetByIdAsync(id, cancellationToken);
        if (quiz is null) return null;

        return new QuizFormDto
        {
            Id = quiz.Id,
            FormationId = quiz.FormationId,
            ModuleId = quiz.ModuleId,
            Type = quiz.Type,
            Title = quiz.Title,
            Instructions = quiz.Instructions,
            TimeLimitSeconds = quiz.TimeLimitSeconds,
            PassingScore = quiz.PassingScore,
            MaxAttempts = quiz.MaxAttempts,
            RandomizeQuestions = quiz.RandomizeQuestions,
            QuestionsToServe = quiz.QuestionsToServe
        };
    }

    public async Task<int> CreateForModuleAsync(int moduleId, int formationId, QuizFormDto form, CancellationToken cancellationToken = default)
    {
        var quiz = BuildQuiz(form);
        quiz.FormationId = formationId;
        quiz.ModuleId = moduleId;
        if (quiz.Type == QuizType.FinalExam) quiz.Type = QuizType.ModuleQuiz;

        await _quizzes.AddAsync(quiz, cancellationToken);
        await _quizzes.SaveChangesAsync(cancellationToken);
        return quiz.Id;
    }

    public async Task<int> CreateFinalExamAsync(int formationId, QuizFormDto form, CancellationToken cancellationToken = default)
    {
        var quiz = BuildQuiz(form);
        quiz.FormationId = formationId;
        quiz.ModuleId = null;
        quiz.Type = QuizType.FinalExam;

        await _quizzes.AddAsync(quiz, cancellationToken);
        await _quizzes.SaveChangesAsync(cancellationToken);
        return quiz.Id;
    }

    public async Task UpdateAsync(QuizFormDto form, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetByIdAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz {form.Id} not found.");

        quiz.Title = form.Title;
        quiz.Instructions = form.Instructions;
        quiz.TimeLimitSeconds = form.TimeLimitSeconds;
        quiz.PassingScore = form.PassingScore;
        quiz.MaxAttempts = form.MaxAttempts;
        quiz.RandomizeQuestions = form.RandomizeQuestions;
        quiz.QuestionsToServe = form.QuestionsToServe;

        _quizzes.Update(quiz);
        await _quizzes.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz {id} not found.");

        _quizzes.Remove(quiz);
        await _quizzes.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<QuestionAdminDto>> GetQuestionsAsync(int quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetWithQuestionsAsync(quizId, cancellationToken);
        if (quiz is null) return new List<QuestionAdminDto>();

        return quiz.Questions.OrderBy(q => q.DisplayOrder).Select(q => new QuestionAdminDto
        {
            Id = q.Id,
            QuizId = q.QuizId,
            Text = q.Text,
            Type = q.Type,
            Points = q.Points,
            DisplayOrder = q.DisplayOrder,
            CorrectAnswersSummary = string.Join(", ", q.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Text))
        }).ToList();
    }

    public async Task<QuestionFormDto?> GetQuestionForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _quizzes.GetQuestionWithOptionsAsync(id, cancellationToken);
        if (question is null) return null;

        var options = question.AnswerOptions
            .OrderBy(o => o.DisplayOrder)
            .Select(o => new AnswerOptionFormDto { Text = o.Text, IsCorrect = o.IsCorrect })
            .ToList();
        while (options.Count < 4) options.Add(new AnswerOptionFormDto());

        return new QuestionFormDto
        {
            Id = question.Id,
            QuizId = question.QuizId,
            Text = question.Text,
            Type = question.Type,
            ImageUrl = question.ImageUrl,
            Explanation = question.Explanation,
            Points = question.Points,
            DisplayOrder = question.DisplayOrder,
            Options = options
        };
    }

    public async Task<int> CreateQuestionAsync(QuestionFormDto form, CancellationToken cancellationToken = default)
    {
        var question = new Question
        {
            QuizId = form.QuizId,
            Text = form.Text,
            Type = form.Type,
            ImageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl,
            Explanation = form.Explanation,
            Points = form.Points,
            DisplayOrder = form.DisplayOrder,
            AnswerOptions = BuildOptions(form.Options)
        };

        await _questions.AddAsync(question, cancellationToken);
        await _questions.SaveChangesAsync(cancellationToken);
        return question.Id;
    }

    public async Task UpdateQuestionAsync(QuestionFormDto form, CancellationToken cancellationToken = default)
    {
        var question = await _quizzes.GetQuestionWithOptionsAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Question {form.Id} not found.");

        question.Text = form.Text;
        question.Type = form.Type;
        question.ImageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl;
        question.Explanation = form.Explanation;
        question.Points = form.Points;
        question.DisplayOrder = form.DisplayOrder;

        question.AnswerOptions.Clear();
        foreach (var option in BuildOptions(form.Options))
        {
            question.AnswerOptions.Add(option);
        }

        _questions.Update(question);
        await _questions.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteQuestionAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _questions.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Question {id} not found.");

        _questions.Remove(question);
        await _questions.SaveChangesAsync(cancellationToken);
    }

    private static Quiz BuildQuiz(QuizFormDto form) => new()
    {
        Type = form.Type,
        Title = form.Title,
        Instructions = form.Instructions,
        TimeLimitSeconds = form.TimeLimitSeconds,
        PassingScore = form.PassingScore,
        MaxAttempts = form.MaxAttempts,
        RandomizeQuestions = form.RandomizeQuestions,
        QuestionsToServe = form.QuestionsToServe
    };

    private static List<AnswerOption> BuildOptions(List<AnswerOptionFormDto> options) =>
        options
            .Where(o => !string.IsNullOrWhiteSpace(o.Text))
            .Select((o, index) => new AnswerOption { Text = o.Text, IsCorrect = o.IsCorrect, DisplayOrder = index + 1 })
            .ToList();

    private static QuizDto MapToDto(Quiz quiz) => new()
    {
        Id = quiz.Id,
        Title = quiz.Title,
        Type = quiz.Type,
        QuestionCount = quiz.Questions.Count
    };
}
