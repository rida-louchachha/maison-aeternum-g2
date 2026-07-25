using MaisonAeternum.Application.AiMentor.Abstractions;
using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MaisonAeternum.Application.AiMentor;

/// <summary>
/// Orchestrates Aurèle. This is the only class in the application that talks to
/// <see cref="IAvatarClient"/> — everything else (controllers, Razor) depends on
/// <see cref="IAiMentorService"/> alone, so the avatar provider can change without
/// touching a single line outside Infrastructure's DI wiring.
/// </summary>
public partial class AiMentorService : IAiMentorService
{
    private readonly IAvatarClient _avatarClient;
    private readonly IConversationService _conversationService;
    private readonly ILearnerProfileRepository _learnerProfiles;
    private readonly IFormationRepository _formations;
    private readonly ICategoryRepository _categories;
    private readonly IModuleRepository _modules;
    private readonly IQuizAttemptRepository _quizAttempts;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ILogger<AiMentorService> _logger;

    public AiMentorService(
        IAvatarClient avatarClient,
        IConversationService conversationService,
        ILearnerProfileRepository learnerProfiles,
        IFormationRepository formations,
        ICategoryRepository categories,
        IModuleRepository modules,
        IQuizAttemptRepository quizAttempts,
        IEnrollmentRepository enrollments,
        ILogger<AiMentorService> logger)
    {
        _avatarClient = avatarClient;
        _conversationService = conversationService;
        _learnerProfiles = learnerProfiles;
        _formations = formations;
        _categories = categories;
        _modules = modules;
        _quizAttempts = quizAttempts;
        _enrollments = enrollments;
        _logger = logger;
    }

    public async Task<AvatarSessionDto> StartSessionAsync(int learnerId, CancellationToken cancellationToken = default)
    {
        var learner = await _learnerProfiles.GetContextAsync(learnerId, cancellationToken);
        if (learner is null)
        {
            return new AvatarSessionDto { Success = false, ProviderName = _avatarClient.ProviderName, ErrorMessage = "Learner profile not found." };
        }

        try
        {
            var result = await _avatarClient.CreateSessionAsync(new AvatarSessionRequest { LearnerDisplayName = learner.FirstName }, cancellationToken);

            return new AvatarSessionDto
            {
                Success = result.Success,
                SessionId = result.SessionId,
                ProviderName = result.ProviderName,
                ClientPayloadJson = result.ClientPayloadJson,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avatar provider {Provider} failed to create a session for learner {LearnerId}", _avatarClient.ProviderName, learnerId);
            return new AvatarSessionDto
            {
                Success = false,
                ProviderName = _avatarClient.ProviderName,
                ErrorMessage = "Aurèle's avatar is temporarily unavailable. You can still chat by text."
            };
        }
    }

    public async Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _avatarClient.CloseSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanly close avatar session {SessionId} (provider {Provider}) — ignoring, session will expire on its own.",
                sessionId, _avatarClient.ProviderName);
        }
    }

    public async Task<AiReplyDto> WelcomeLearnerAsync(int learnerId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);
        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.General, null, null, null, cancellationToken);

        var text = BuildWelcomeScript(learner);
        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> IntroduceFormationAsync(int learnerId, int formationId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var formation = await _formations.GetByIdAsync(formationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {formationId} not found.");
        var category = await _categories.GetByIdAsync(formation.CategoryId, cancellationToken);

        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.ModuleIntroduction, formationId, null, null, cancellationToken);

        var text = BuildFormationIntroScript(learner, formation, category?.Name);
        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> IntroduceModuleAsync(int learnerId, int moduleId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var module = await _modules.GetWithFormationAsync(moduleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Module {moduleId} not found.");

        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.ModuleIntroduction, module.FormationId, moduleId, null, cancellationToken);

        var text = BuildModuleIntroScript(learner, module);
        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> AskAsync(int learnerId, string question, int? conversationId, string? sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("A question is required.", nameof(question));

        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var resolvedConversationId = conversationId
            ?? await _conversationService.GetOrStartConversationAsync(learnerId, AiConversationContext.General, null, null, null, cancellationToken);

        await _conversationService.AppendMessageAsync(resolvedConversationId, MessageSender.Learner, question, cancellationToken);

        var text = BuildAnswerScript(question, learner);
        return await SpeakAndPersistAsync(resolvedConversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> ExplainQuizMistakeAsync(int learnerId, int quizAttemptId, int questionId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var attempt = await _quizAttempts.GetWithDetailsAsync(quizAttemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz attempt {quizAttemptId} not found.");

        if (attempt.LearnerId != learnerId)
            throw new UnauthorizedAccessException("This quiz attempt does not belong to the current learner.");

        var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == questionId)
            ?? throw new KeyNotFoundException($"No answer for question {questionId} on attempt {quizAttemptId}.");

        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.QuizReview, attempt.Quiz.FormationId, attempt.Quiz.ModuleId, quizAttemptId, cancellationToken);

        var text = BuildQuizMistakeScript(learner, answer);
        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> RecommendNextStepAsync(int learnerId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var enrollments = await _enrollments.GetByLearnerWithDetailsAsync(learnerId, cancellationToken);
        var recommendation = ResolveNextStepRecommendation(enrollments);

        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.StudyRecommendation, recommendation.FormationId, recommendation.ModuleId, null, cancellationToken);

        var text = BuildRecommendationScript(learner, recommendation);
        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public async Task<AiReplyDto> ReactToQuizResultAsync(int learnerId, int quizAttemptId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var learner = await RequireLearnerContextAsync(learnerId, cancellationToken);

        var attempt = await _quizAttempts.GetWithDetailsAsync(quizAttemptId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quiz attempt {quizAttemptId} not found.");

        if (attempt.LearnerId != learnerId)
            throw new UnauthorizedAccessException("This quiz attempt does not belong to the current learner.");

        var conversationId = await _conversationService.GetOrStartConversationAsync(
            learnerId, AiConversationContext.QuizReview, attempt.Quiz.FormationId, attempt.Quiz.ModuleId, quizAttemptId, cancellationToken);

        var text = attempt.Passed
            ? BuildCongratulationScript(learner, attempt)
            : BuildEncouragementScript(learner, attempt);

        return await SpeakAndPersistAsync(conversationId, text, sessionId, cancellationToken);
    }

    public Task<ConversationDto?> GetConversationAsync(int conversationId, CancellationToken cancellationToken = default) =>
        _conversationService.GetConversationAsync(conversationId, cancellationToken);

    public Task<List<ConversationSummaryDto>> GetHistoryAsync(int learnerId, CancellationToken cancellationToken = default) =>
        _conversationService.GetRecentConversationsAsync(learnerId, 20, cancellationToken);

    private async Task<LearnerContextDto> RequireLearnerContextAsync(int learnerId, CancellationToken cancellationToken) =>
        await _learnerProfiles.GetContextAsync(learnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Learner profile {learnerId} not found.");

    private async Task<AiReplyDto> SpeakAndPersistAsync(int conversationId, string text, string? sessionId, CancellationToken cancellationToken)
    {
        await _conversationService.AppendMessageAsync(conversationId, MessageSender.Aurele, text, cancellationToken);

        var (spoken, clientSide, warning) = await TrySpeakAsync(sessionId, text, cancellationToken);

        return new AiReplyDto
        {
            ConversationId = conversationId,
            Text = text,
            SpokenByAvatar = spoken,
            HandledClientSide = clientSide,
            Warning = warning
        };
    }

    private async Task<(bool spoken, bool clientSide, string? warning)> TrySpeakAsync(string? sessionId, string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sessionId)) return (false, false, null);

        try
        {
            var result = await _avatarClient.SendSpeechAsync(sessionId, text, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Avatar speech failed for session {SessionId} ({Provider}): {Error}",
                    sessionId, _avatarClient.ProviderName, result.ErrorMessage);
                return (false, false, "Aurèle couldn't speak this reply aloud, but here's the answer.");
            }

            return (true, result.HandledClientSide, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avatar provider {Provider} threw while sending speech for session {SessionId}", _avatarClient.ProviderName, sessionId);
            return (false, false, "Aurèle's voice is temporarily unavailable — showing the text reply instead.");
        }
    }
}
