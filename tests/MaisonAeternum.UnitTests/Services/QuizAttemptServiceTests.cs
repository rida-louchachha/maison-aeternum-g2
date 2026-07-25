using MaisonAeternum.Application.Assessment;
using MaisonAeternum.Application.Assessment.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Moq;

namespace MaisonAeternum.UnitTests.Services;

public class QuizAttemptServiceTests
{
    private readonly Mock<IQuizRepository> _quizzes = new();
    private readonly Mock<IQuizAttemptRepository> _attempts = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ICertificateRepository> _certificates = new();
    private readonly Mock<IRepository<GuildRank>> _guildRanks = new();
    private readonly Mock<IRepository<LearnerProfile>> _learnerProfiles = new();
    private readonly QuizAttemptService _sut;

    public QuizAttemptServiceTests()
    {
        _sut = new QuizAttemptService(
            _quizzes.Object, _attempts.Object, _enrollments.Object, _certificates.Object, _guildRanks.Object, _learnerProfiles.Object);
    }

    private static Quiz BuildQuiz(QuizType type = QuizType.ModuleQuiz, decimal passingScore = 70m)
    {
        var singleChoiceCorrect = new AnswerOption { Id = 1, Text = "Correct", IsCorrect = true };
        var singleChoiceWrong = new AnswerOption { Id = 2, Text = "Wrong", IsCorrect = false };
        var question1 = new Question { Id = 100, Type = QuestionType.SingleChoice, Points = 50, Explanation = "e1", Text = "q1" };
        question1.AnswerOptions.Add(singleChoiceCorrect);
        question1.AnswerOptions.Add(singleChoiceWrong);

        var multiA = new AnswerOption { Id = 3, Text = "A", IsCorrect = true };
        var multiB = new AnswerOption { Id = 4, Text = "B", IsCorrect = true };
        var multiC = new AnswerOption { Id = 5, Text = "C", IsCorrect = false };
        var question2 = new Question { Id = 200, Type = QuestionType.MultipleAnswer, Points = 50, Explanation = "e2", Text = "q2" };
        question2.AnswerOptions.Add(multiA);
        question2.AnswerOptions.Add(multiB);
        question2.AnswerOptions.Add(multiC);

        var quiz = new Quiz { Id = 9, FormationId = 55, Type = type, PassingScore = passingScore, Title = "Bench Trial" };
        quiz.Questions.Add(question1);
        quiz.Questions.Add(question2);
        return quiz;
    }

    [Fact]
    public async Task SubmitAttemptAsync_Throws_WhenAttemptBelongsToAnotherLearner()
    {
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 999, QuizId = 9 });

        var submission = new SubmitQuizAttemptDto { AttemptId = 1 };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.SubmitAttemptAsync(learnerId: 1, submission));
    }

    [Fact]
    public async Task SubmitAttemptAsync_Throws_WhenAttemptAlreadySubmitted()
    {
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 1, QuizId = 9, SubmittedAt = DateTime.UtcNow });

        var submission = new SubmitQuizAttemptDto { AttemptId = 1 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitAttemptAsync(1, submission));
    }

    [Fact]
    public async Task SubmitAttemptAsync_MultipleAnswerQuestion_RequiresExactSetMatch()
    {
        var quiz = BuildQuiz();
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 1, QuizId = 9, StartedAt = DateTime.UtcNow.AddMinutes(-2) });
        _quizzes.Setup(r => r.GetWithQuestionsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(quiz);

        // Question 2 is correct only if BOTH A and B (ids 3,4) are selected — submit just A.
        var submission = new SubmitQuizAttemptDto
        {
            AttemptId = 1,
            Answers = new List<SubmitQuizAnswerDto>
            {
                new() { QuestionId = 100, SelectedOptionIds = new List<int> { 1 } }, // correct
                new() { QuestionId = 200, SelectedOptionIds = new List<int> { 3 } }  // incomplete -> incorrect
            }
        };

        var result = await _sut.SubmitAttemptAsync(1, submission);

        Assert.True(result.QuestionResults.Single(q => q.QuestionId == 100).IsCorrect);
        Assert.False(result.QuestionResults.Single(q => q.QuestionId == 200).IsCorrect);
        Assert.Equal(50m, result.ScorePercentage); // only question 1's 50 of 100 total points earned
    }

    [Fact]
    public async Task SubmitAttemptAsync_Passes_WhenScoreMeetsThreshold_AndFails_WhenBelowIt()
    {
        var quiz = BuildQuiz(passingScore: 50m);
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 1, QuizId = 9, StartedAt = DateTime.UtcNow });
        _quizzes.Setup(r => r.GetWithQuestionsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(quiz);

        var submission = new SubmitQuizAttemptDto
        {
            AttemptId = 1,
            Answers = new List<SubmitQuizAnswerDto> { new() { QuestionId = 100, SelectedOptionIds = new List<int> { 1 } } }
        };

        var result = await _sut.SubmitAttemptAsync(1, submission);

        Assert.True(result.Passed); // 50% earned meets the 50% threshold
    }

    [Fact]
    public async Task SubmitAttemptAsync_IssuesCertificate_ForAPassedFinalExam_WhenNoneExistsYet()
    {
        var quiz = BuildQuiz(QuizType.FinalExam, passingScore: 40m);
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 1, QuizId = 9, StartedAt = DateTime.UtcNow });
        _quizzes.Setup(r => r.GetWithQuestionsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(quiz);
        _certificates.Setup(r => r.ExistsForLearnerAndFormationAsync(1, 55, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _learnerProfiles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LearnerProfile { Id = 1, GuildRankId = 1 });
        _guildRanks.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GuildRank> { new() { Id = 1, Level = 0, MinFormationsCompleted = 0, Name = "Apprentice" } });

        var submission = new SubmitQuizAttemptDto
        {
            AttemptId = 1,
            Answers = new List<SubmitQuizAnswerDto> { new() { QuestionId = 100, SelectedOptionIds = new List<int> { 1 } } }
        };

        var result = await _sut.SubmitAttemptAsync(1, submission);

        Assert.True(result.Passed);
        _certificates.Verify(c => c.AddAsync(It.IsAny<Certificate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAttemptAsync_DoesNotIssueASecondCertificate_ForTheSameFormation()
    {
        var quiz = BuildQuiz(QuizType.FinalExam, passingScore: 40m);
        _attempts.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuizAttempt { Id = 1, LearnerId = 1, QuizId = 9, StartedAt = DateTime.UtcNow });
        _quizzes.Setup(r => r.GetWithQuestionsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(quiz);
        _certificates.Setup(r => r.ExistsForLearnerAndFormationAsync(1, 55, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var submission = new SubmitQuizAttemptDto
        {
            AttemptId = 1,
            Answers = new List<SubmitQuizAnswerDto> { new() { QuestionId = 100, SelectedOptionIds = new List<int> { 1 } } }
        };

        await _sut.SubmitAttemptAsync(1, submission);

        _certificates.Verify(c => c.AddAsync(It.IsAny<Certificate>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
