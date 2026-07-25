namespace MaisonAeternum.Domain.Entities;

public class QuizAttemptSelectedOption
{
    public int QuizAttemptAnswerId { get; set; }
    public int AnswerOptionId { get; set; }

    public QuizAttemptAnswer QuizAttemptAnswer { get; set; } = default!;
    public AnswerOption AnswerOption { get; set; } = default!;
}
