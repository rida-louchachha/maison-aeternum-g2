using MaisonAeternum.Application.Assessment.Abstractions;
using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Assessment;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizRepository _quizzes;
    private readonly IQuizAttemptRepository _attempts;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICertificateRepository _certificates;
    private readonly IRepository<GuildRank> _guildRanks;
    private readonly IRepository<LearnerProfile> _learnerProfiles;

    public QuizAttemptService(
        IQuizRepository quizzes,
        IQuizAttemptRepository attempts,
        IEnrollmentRepository enrollments,
        ICertificateRepository certificates,
        IRepository<GuildRank> guildRanks,
        IRepository<LearnerProfile> learnerProfiles)
    {
        _quizzes = quizzes;
        _attempts = attempts;
        _enrollments = enrollments;
        _certificates = certificates;
        _guildRanks = guildRanks;
        _learnerProfiles = learnerProfiles;
    }

    public async Task<QuizAttemptStartDto> StartAttemptAsync(int learnerId, int quizId, CancellationToken cancellationToken = default)
    {
        var quiz = await _quizzes.GetWithQuestionsAsync(quizId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz {quizId} not found.");

        var attemptCount = await _attempts.CountAttemptsAsync(learnerId, quizId, cancellationToken);
        if (quiz.MaxAttempts.HasValue && attemptCount >= quiz.MaxAttempts.Value)
            throw new InvalidOperationException($"Maximum of {quiz.MaxAttempts} attempts already reached for this quiz.");

        var random = Random.Shared;
        var pool = quiz.Questions.ToList();
        if (quiz.RandomizeQuestions) pool = pool.OrderBy(_ => random.Next()).ToList();
        if (quiz.QuestionsToServe.HasValue) pool = pool.Take(quiz.QuestionsToServe.Value).ToList();

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            LearnerId = learnerId,
            AttemptNumber = attemptCount + 1,
            StartedAt = DateTime.UtcNow,
            ScorePercentage = 0,
            Passed = false,
            TimeTakenSeconds = 0
        };

        await _attempts.AddAsync(attempt, cancellationToken);
        await _attempts.SaveChangesAsync(cancellationToken);

        return new QuizAttemptStartDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            TimeLimitSeconds = quiz.TimeLimitSeconds,
            Questions = pool.Select(q => new QuizAttemptQuestionDto
            {
                QuestionId = q.Id,
                Text = q.Text,
                Type = q.Type,
                ImageUrl = q.ImageUrl,
                Points = q.Points,
                Options = q.AnswerOptions
                    .OrderBy(_ => random.Next())
                    .Select(o => new QuizAttemptOptionDto { OptionId = o.Id, Text = o.Text })
                    .ToList()
            }).ToList()
        };
    }

    public async Task<QuizAttemptResultDto> SubmitAttemptAsync(int learnerId, SubmitQuizAttemptDto submission, CancellationToken cancellationToken = default)
    {
        var attempt = await _attempts.GetByIdAsync(submission.AttemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attempt {submission.AttemptId} not found.");

        if (attempt.LearnerId != learnerId)
            throw new UnauthorizedAccessException("This quiz attempt does not belong to the current learner.");
        if (attempt.SubmittedAt is not null)
            throw new InvalidOperationException("This attempt has already been submitted.");

        var quiz = await _quizzes.GetWithQuestionsAsync(attempt.QuizId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz {attempt.QuizId} not found.");

        var questionResults = new List<QuestionResultDto>();
        var answerEntities = new List<QuizAttemptAnswer>();
        var totalPoints = 0;
        var earnedPoints = 0;

        foreach (var submittedAnswer in submission.Answers)
        {
            var question = quiz.Questions.FirstOrDefault(q => q.Id == submittedAnswer.QuestionId);
            if (question is null) continue; // ignore answers for questions that aren't part of this quiz

            var correctOptionIds = question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
            var selectedOptionIds = submittedAnswer.SelectedOptionIds.ToHashSet();
            var isCorrect = correctOptionIds.SetEquals(selectedOptionIds);

            totalPoints += question.Points;
            if (isCorrect) earnedPoints += question.Points;

            var answerEntity = new QuizAttemptAnswer
            {
                QuizAttemptId = attempt.Id,
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                PointsAwarded = isCorrect ? question.Points : 0,
                SelectedOptions = selectedOptionIds
                    .Select(optionId => new QuizAttemptSelectedOption { AnswerOptionId = optionId })
                    .ToList()
            };
            answerEntities.Add(answerEntity);

            questionResults.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                Text = question.Text,
                IsCorrect = isCorrect,
                Explanation = question.Explanation,
                SelectedOptionTexts = question.AnswerOptions.Where(o => selectedOptionIds.Contains(o.Id)).Select(o => o.Text).ToList(),
                CorrectOptionTexts = question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Text).ToList()
            });
        }

        attempt.ScorePercentage = totalPoints == 0 ? 0 : Math.Round(earnedPoints * 100m / totalPoints, 1);
        attempt.Passed = attempt.ScorePercentage >= quiz.PassingScore;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TimeTakenSeconds = (int)(attempt.SubmittedAt.Value - attempt.StartedAt).TotalSeconds;
        attempt.Answers = answerEntities;

        _attempts.Update(attempt);
        await _attempts.SaveChangesAsync(cancellationToken);

        var result = new QuizAttemptResultDto
        {
            AttemptId = attempt.Id,
            QuizTitle = quiz.Title,
            ScorePercentage = attempt.ScorePercentage,
            PassingScore = quiz.PassingScore,
            Passed = attempt.Passed,
            TimeTakenSeconds = attempt.TimeTakenSeconds,
            QuestionResults = questionResults
        };

        if (quiz.Type == QuizType.FinalExam && attempt.Passed)
        {
            await IssueCertificateIfNeededAsync(learnerId, quiz.FormationId, attempt.Id, result, cancellationToken);
        }

        return result;
    }

    public async Task<QuizAttemptResultDto?> GetResultAsync(int learnerId, int attemptId, CancellationToken cancellationToken = default)
    {
        var attempt = await _attempts.GetWithDetailsAsync(attemptId, cancellationToken);
        if (attempt is null || attempt.LearnerId != learnerId) return null;

        return new QuizAttemptResultDto
        {
            AttemptId = attempt.Id,
            QuizTitle = attempt.Quiz.Title,
            ScorePercentage = attempt.ScorePercentage,
            PassingScore = attempt.Quiz.PassingScore,
            Passed = attempt.Passed,
            TimeTakenSeconds = attempt.TimeTakenSeconds,
            // Re-derived from the Certificate table rather than a transient flag, so this is
            // correct both immediately after submission and on any later revisit of this page.
            CertificateIssued = await _certificates.ExistsForAttemptAsync(attemptId, cancellationToken),
            QuestionResults = attempt.Answers.Select(a => new QuestionResultDto
            {
                QuestionId = a.QuestionId,
                Text = a.Question.Text,
                IsCorrect = a.IsCorrect,
                Explanation = a.Question.Explanation,
                SelectedOptionTexts = a.SelectedOptions.Select(so => so.AnswerOption.Text).ToList(),
                CorrectOptionTexts = a.Question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Text).ToList()
            }).ToList()
        };
    }

    private async Task IssueCertificateIfNeededAsync(
        int learnerId, int formationId, int attemptId, QuizAttemptResultDto result, CancellationToken cancellationToken)
    {
        if (await _certificates.ExistsForLearnerAndFormationAsync(learnerId, formationId, cancellationToken))
            return;

        var learner = await _learnerProfiles.GetByIdAsync(learnerId, cancellationToken);
        if (learner is null) return;

        var certificate = new Certificate
        {
            LearnerId = learnerId,
            FormationId = formationId,
            QuizAttemptId = attemptId,
            GuildRankId = learner.GuildRankId,
            CertificateNumber = $"MA-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            VerificationToken = Guid.NewGuid(),
            IssuedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _certificates.AddAsync(certificate, cancellationToken);

        var enrollment = await _enrollments.GetByLearnerAndFormationAsync(learnerId, formationId, cancellationToken);
        if (enrollment is not null)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt ??= DateTime.UtcNow;
            _enrollments.Update(enrollment);
        }

        await _certificates.SaveChangesAsync(cancellationToken);
        result.CertificateIssued = true;

        // Guild rank advancement: the highest rank whose threshold the learner's total certificate count now meets.
        var certificateCount = await _certificates.CountByLearnerAsync(learnerId, cancellationToken);
        var ranks = await _guildRanks.ListAllAsync(cancellationToken);
        var earnedRank = ranks
            .Where(r => r.MinFormationsCompleted <= certificateCount)
            .OrderByDescending(r => r.Level)
            .FirstOrDefault();

        if (earnedRank is not null && earnedRank.Id != learner.GuildRankId)
        {
            learner.GuildRankId = earnedRank.Id;
            _learnerProfiles.Update(learner);
            await _learnerProfiles.SaveChangesAsync(cancellationToken);
            result.NewGuildRankName = earnedRank.Name;
        }
    }
}
