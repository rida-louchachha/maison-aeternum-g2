using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Assessment.Models;

public class QuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public QuizType Type { get; set; }
    public int QuestionCount { get; set; }
}

public class QuizFormDto
{
    public int Id { get; set; }
    public int FormationId { get; set; }
    public int? ModuleId { get; set; }
    public QuizType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Instructions { get; set; } = default!;
    public int? TimeLimitSeconds { get; set; } = 600;
    public decimal PassingScore { get; set; } = 70;
    public int? MaxAttempts { get; set; } = 3;
    public bool RandomizeQuestions { get; set; } = true;
    public int? QuestionsToServe { get; set; }
}

public class QuestionAdminDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; } = default!;
    public QuestionType Type { get; set; }
    public int Points { get; set; }
    public int DisplayOrder { get; set; }
    public string CorrectAnswersSummary { get; set; } = default!;
}

public class QuestionFormDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; } = default!;
    public QuestionType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string Explanation { get; set; } = default!;
    public int Points { get; set; } = 10;
    public int DisplayOrder { get; set; } = 1;
    public List<AnswerOptionFormDto> Options { get; set; } = new()
    {
        new(), new(), new(), new()
    };
}

public class AnswerOptionFormDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
