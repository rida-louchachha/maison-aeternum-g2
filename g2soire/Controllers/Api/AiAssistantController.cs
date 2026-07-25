using MaisonAeternum.Application.AiMentor.Abstractions;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Web.Models.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaisonAeternum.Web.Controllers.Api;

/// <summary>
/// Thin JSON API for the floating Aurèle assistant. Every action resolves the caller's
/// LearnerProfile and delegates entirely to <see cref="IAiMentorService"/> — no business
/// logic, no avatar-provider awareness, lives here.
/// </summary>
[Authorize]
[ApiController]
[Route("api/ai")]
public class AiAssistantController : ControllerBase
{
    private readonly IAiMentorService _aiMentor;
    private readonly ILearnerProfileRepository _learnerProfiles;
    private readonly ILogger<AiAssistantController> _logger;

    public AiAssistantController(IAiMentorService aiMentor, ILearnerProfileRepository learnerProfiles, ILogger<AiAssistantController> logger)
    {
        _aiMentor = aiMentor;
        _learnerProfiles = learnerProfiles;
        _logger = logger;
    }

    [HttpPost("session")]
    public Task<IActionResult> StartSession(CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.StartSessionAsync(learnerId, cancellationToken));

    [HttpPost("session/{sessionId}/close")]
    public async Task<IActionResult> CloseSession(string sessionId, CancellationToken cancellationToken)
    {
        await _aiMentor.CloseSessionAsync(sessionId, cancellationToken);
        return NoContent();
    }

    [HttpPost("welcome")]
    public Task<IActionResult> Welcome([FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.WelcomeLearnerAsync(learnerId, request.SessionId, cancellationToken));

    [HttpPost("formations/{formationId:int}/introduce")]
    public Task<IActionResult> IntroduceFormation(int formationId, [FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.IntroduceFormationAsync(learnerId, formationId, request.SessionId, cancellationToken));

    [HttpPost("modules/{moduleId:int}/introduce")]
    public Task<IActionResult> IntroduceModule(int moduleId, [FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.IntroduceModuleAsync(learnerId, moduleId, request.SessionId, cancellationToken));

    [HttpPost("ask")]
    public Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Task.FromResult<IActionResult>(ValidationProblem(ModelState));

        return WithLearnerAsync(learnerId =>
            _aiMentor.AskAsync(learnerId, request.Question, request.ConversationId, request.SessionId, cancellationToken));
    }

    [HttpPost("quiz-attempts/{quizAttemptId:int}/questions/{questionId:int}/explain")]
    public Task<IActionResult> ExplainQuizMistake(int quizAttemptId, int questionId, [FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.ExplainQuizMistakeAsync(learnerId, quizAttemptId, questionId, request.SessionId, cancellationToken));

    [HttpPost("quiz-attempts/{quizAttemptId:int}/react")]
    public Task<IActionResult> ReactToQuizResult(int quizAttemptId, [FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.ReactToQuizResultAsync(learnerId, quizAttemptId, request.SessionId, cancellationToken));

    [HttpPost("recommend-next")]
    public Task<IActionResult> RecommendNext([FromBody] SessionScopedRequest request, CancellationToken cancellationToken) =>
        WithLearnerAsync(learnerId => _aiMentor.RecommendNextStepAsync(learnerId, request.SessionId, cancellationToken));

    [HttpGet("conversations/{conversationId:int}")]
    public async Task<IActionResult> GetConversation(int conversationId, CancellationToken cancellationToken)
    {
        var conversation = await _aiMentor.GetConversationAsync(conversationId, cancellationToken);
        return conversation is null ? NotFound() : Ok(conversation);
    }

    [HttpGet("history")]
    public Task<IActionResult> GetHistory(CancellationToken cancellationToken) =>
        WithLearnerAsync(async learnerId => (object)await _aiMentor.GetHistoryAsync(learnerId, cancellationToken));

    private async Task<IActionResult> WithLearnerAsync<T>(Func<int, Task<T>> action)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var learnerId = await _learnerProfiles.GetLearnerIdByUserIdAsync(userId);
        if (learnerId is null)
            return Problem("Only apprentices have a Maison profile Aurèle can speak to.", statusCode: StatusCodes.Status403Forbidden);

        try
        {
            var result = await action(learnerId.Value);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error serving Aurèle request for learner {LearnerId}", learnerId);
            return Problem("Aurèle ran into a problem answering that. Please try again.", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
