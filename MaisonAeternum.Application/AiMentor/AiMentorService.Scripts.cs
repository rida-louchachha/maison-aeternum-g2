using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.AiMentor;

/// <summary>
/// Aurèle's spoken-script generation. Deliberately rule/template-based rather than an LLM
/// call: every script is assembled from real data pulled through the repositories (learner
/// name, guild rank, formation/module details, actual quiz explanations), which is what
/// makes the replies genuinely contextual. Swapping this for a call to a hosted LLM (to
/// handle fully free-form questions) is the natural extension point — see AskAsync/
/// BuildAnswerScript below and the "Future Improvements" note in the AI Trainer docs.
/// </summary>
public partial class AiMentorService
{
    private static readonly (string[] Keywords, string Answer)[] KnowledgeBase =
    {
        (new[] { "escapement" }, "The escapement is the heart of a mechanical movement — it releases the mainspring's energy in small, regular impulses, which is what divides continuous motion into the ticking seconds you hear."),
        (new[] { "tourbillon" }, "A tourbillon houses the escapement and balance wheel in a rotating cage, usually completing one turn per minute. It averages out the positional errors gravity causes, which is why it's considered a Grand Complication."),
        (new[] { "mainspring" }, "The mainspring is a coiled strip of metal that stores energy when wound. As it unwinds, it drives the gear train — think of it as the movement's battery."),
        (new[] { "jewel", "jewels" }, "Jewels — usually synthetic ruby — are used at high-friction pivot points to reduce wear and improve precision. A movement's jewel count is often quoted as a mark of its complexity."),
        (new[] { "certificate", "certification" }, "Once you pass a formation's Bench Trial final exam, your certificate is issued automatically and appears on your profile — verifiable by anyone via its QR code."),
        (new[] { "streak", "bench streak" }, "Your Bench Streak counts consecutive days with any recorded activity — a completed module, a quiz attempt, or logged bench time. It resets if a full day passes with none."),
        (new[] { "rank", "guild rank" }, "Guild ranks — Apprentice, Journeyman, Certified Horologer, Master of the Maison — advance as you complete more formations. Each rank unlocks recognition, not new content; all formations stay open to you.")
    };

    private string BuildWelcomeScript(LearnerContextDto learner) =>
        $"Bonjour {learner.FirstName}, welcome back to Maison Aeternum. You're currently ranked {learner.GuildRankName}" +
        (learner.CurrentStreakDays > 0
            ? $", and you're on a {learner.CurrentStreakDays}-day bench streak — beautiful discipline. What shall we work on today?"
            : ". It's a fine day to begin — what shall we work on today?");

    private string BuildFormationIntroScript(LearnerContextDto learner, Formation formation, string? categoryName)
    {
        var categoryPhrase = categoryName is null ? string.Empty : $" from our {categoryName} specialization";
        return $"{learner.FirstName}, let me introduce {formation.Title}{categoryPhrase}. " +
               $"{formation.ShortDescription} This formation is rated for {Humanize(formation.Difficulty)} horologers and runs " +
               $"roughly {formation.EstimatedMinutes} minutes across its modules, finishing with a certifying Bench Trial. Shall we begin?";
    }

    private string BuildModuleIntroScript(LearnerContextDto learner, Module module) =>
        $"{learner.FirstName}, this module — {module.Title} — is part of {module.Formation.Title}. {module.Description} " +
        $"Plan for about {module.EstimatedMinutes} minutes. Take your time with the workshop recording before attempting any exercises.";

    private string BuildAnswerScript(string question, LearnerContextDto learner)
    {
        var normalized = question.ToLowerInvariant();
        var match = KnowledgeBase.FirstOrDefault(entry => entry.Keywords.Any(k => normalized.Contains(k)));

        if (match.Answer is not null)
            return match.Answer;

        return $"That's a fair question, {learner.FirstName} — it's specific enough that I'd rather not guess. " +
               "I've noted it, and your Master Watchmaker can go deeper on it during your next formation. " +
               "In the meantime, try rephrasing around a specific term — an escapement, a complication, a rank — and I'll do my best.";
    }

    private string BuildQuizMistakeScript(LearnerContextDto learner, QuizAttemptAnswer answer)
    {
        var question = answer.Question;
        var selected = answer.SelectedOptions.Select(so => so.AnswerOption.Text).ToList();
        var correct = question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Text).ToList();

        if (answer.IsCorrect)
        {
            return $"Good news, {learner.FirstName} — you actually answered \"{question.Text}\" correctly. {question.Explanation}";
        }

        var selectedText = selected.Count > 0 ? string.Join(", ", selected) : "no answer";
        var correctText = string.Join(", ", correct);

        return $"Let's revisit this one, {learner.FirstName}. For \"{question.Text}\", you answered: {selectedText}. " +
               $"The correct answer was: {correctText}. {question.Explanation}";
    }

    private string BuildRecommendationScript(LearnerContextDto learner, NextStepRecommendation recommendation)
    {
        if (!recommendation.HasRecommendation)
        {
            return $"{learner.FirstName}, you have no formation in progress right now. " +
                   "Browse the catalog and enroll in whichever specialization calls to you — I'll be here to introduce it.";
        }

        return $"{learner.FirstName}, I'd suggest continuing with \"{recommendation.ModuleTitle}\" in {recommendation.FormationTitle} — " +
               "it's exactly where you left off, and keeping the momentum will help it stick.";
    }

    private string BuildCongratulationScript(LearnerContextDto learner, QuizAttempt attempt) =>
        $"Bravo, {learner.FirstName}! You passed {attempt.Quiz.Title} with a score of {attempt.ScorePercentage:0}%. " +
        "That's real progress toward your next guild rank — your certificate has been issued. Well earned.";

    private string BuildEncouragementScript(LearnerContextDto learner, QuizAttempt attempt) =>
        $"{learner.FirstName}, that Bench Trial — {attempt.Quiz.Title} — didn't go your way this time, {attempt.ScorePercentage:0}%. " +
        "Every master watchmaker has redone a trial before. Review the module's workshop recording once more, and " +
        (attempt.Quiz.MaxAttempts is null or > 1 ? "you're welcome to try again when ready." : "speak with your trainer about next steps.");

    private static string Humanize(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Apprentice => "Apprentice",
        DifficultyLevel.Journeyman => "Journeyman",
        DifficultyLevel.CertifiedHorologer => "Certified Horologer",
        DifficultyLevel.MasterOfTheMaison => "Master of the Maison",
        _ => level.ToString()
    };

    private static NextStepRecommendation ResolveNextStepRecommendation(List<Enrollment> enrollments)
    {
        var active = enrollments
            .Where(e => e.Status == EnrollmentStatus.InProgress)
            .OrderByDescending(e => e.LastAccessedAt)
            .FirstOrDefault();

        if (active is null)
            return new NextStepRecommendation(null, null, string.Empty, null, false);

        var completedModuleIds = active.ModuleProgresses.Where(mp => mp.IsCompleted).Select(mp => mp.ModuleId).ToHashSet();
        var nextModule = active.Formation.Modules
            .OrderBy(m => m.DisplayOrder)
            .FirstOrDefault(m => !completedModuleIds.Contains(m.Id));

        return nextModule is null
            ? new NextStepRecommendation(active.FormationId, null, active.Formation.Title, null, false)
            : new NextStepRecommendation(active.FormationId, nextModule.Id, active.Formation.Title, nextModule.Title, true);
    }

    private sealed record NextStepRecommendation(int? FormationId, int? ModuleId, string FormationTitle, string? ModuleTitle, bool HasRecommendation);
}
