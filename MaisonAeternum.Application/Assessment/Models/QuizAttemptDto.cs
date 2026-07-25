using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Assessment.Models;

public class QuizAttemptStartDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = default!;
    public int? TimeLimitSeconds { get; set; }
    public List<QuizAttemptQuestionDto> Questions { get; set; } = new();
}

/// <summary>What the learner sees while taking the quiz — deliberately excludes IsCorrect.</summary>
public class QuizAttemptQuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = default!;
    public QuestionType Type { get; set; }
    public string? ImageUrl { get; set; }
    public int Points { get; set; }
    public List<QuizAttemptOptionDto> Options { get; set; } = new();
}

public class QuizAttemptOptionDto
{
    public int OptionId { get; set; }
    public string Text { get; set; } = default!;
}

public class SubmitQuizAttemptDto
{
    public int AttemptId { get; set; }
    public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
}

public class SubmitQuizAnswerDto
{
    public int QuestionId { get; set; }
    public List<int> SelectedOptionIds { get; set; } = new();
}

public class QuizAttemptResultDto
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = default!;
    public decimal ScorePercentage { get; set; }
    public decimal PassingScore { get; set; }
    public bool Passed { get; set; }
    public int TimeTakenSeconds { get; set; }
    public bool CertificateIssued { get; set; }
    public string? NewGuildRankName { get; set; }
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = default!;
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; } = default!;
    public List<string> SelectedOptionTexts { get; set; } = new();
    public List<string> CorrectOptionTexts { get; set; } = new();
}
