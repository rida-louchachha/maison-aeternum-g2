using MaisonAeternum.Application.AiMentor.Models;

namespace MaisonAeternum.Application.AiMentor.Abstractions;

/// <summary>
/// Aurèle, the Maison's AI mentor. This is the ONLY AI-related type the rest of the
/// application (controllers, Razor components) is allowed to depend on — it hides the
/// avatar provider, the conversation store, and the response-generation logic behind one
/// seam. Every method builds context from the learner's real data (via repositories),
/// composes a spoken reply, has the avatar speak it if a session is active, and persists
/// the exchange.
/// </summary>
public interface IAiMentorService
{
    /// <summary>Opens (or resumes) an avatar session for the current learner, for the frontend SDK to connect to.</summary>
    Task<AvatarSessionDto> StartSessionAsync(int learnerId, CancellationToken cancellationToken = default);

    Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Feature 1 — greets a learner (first login of the day, or first visit).</summary>
    Task<AiReplyDto> WelcomeLearnerAsync(int learnerId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Feature 2 — introduces a formation's objectives, difficulty, and what to expect.</summary>
    Task<AiReplyDto> IntroduceFormationAsync(int learnerId, int formationId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Feature 3 — introduces a specific module within a formation.</summary>
    Task<AiReplyDto> IntroduceModuleAsync(int learnerId, int moduleId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Features 4 &amp; 5 — free-form question answering (concept explanations included), continuing a conversation if one is open.</summary>
    Task<AiReplyDto> AskAsync(int learnerId, string question, int? conversationId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Feature 6 — explains why a specific answer on a quiz attempt was wrong.</summary>
    Task<AiReplyDto> ExplainQuizMistakeAsync(int learnerId, int quizAttemptId, int questionId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Feature 7 — recommends the learner's next best module across their active enrollments.</summary>
    Task<AiReplyDto> RecommendNextStepAsync(int learnerId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Features 8 &amp; 9 — reacts to a completed quiz attempt: congratulates on a pass, encourages after a fail.</summary>
    Task<AiReplyDto> ReactToQuizResultAsync(int learnerId, int quizAttemptId, string? sessionId, CancellationToken cancellationToken = default);

    Task<ConversationDto?> GetConversationAsync(int conversationId, CancellationToken cancellationToken = default);

    Task<List<ConversationSummaryDto>> GetHistoryAsync(int learnerId, CancellationToken cancellationToken = default);
}
