using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning.Abstractions;
using MaisonAeternum.Application.Learning.Models;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Learning;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IFormationRepository _formations;
    private readonly IModuleRepository _modules;
    private readonly IQuizRepository _quizzes;
    private readonly IActivityLogRepository _activityLogs;
    private readonly IRepository<LearnerProfile> _learnerProfiles;

    public EnrollmentService(
        IEnrollmentRepository enrollments,
        IFormationRepository formations,
        IModuleRepository modules,
        IQuizRepository quizzes,
        IActivityLogRepository activityLogs,
        IRepository<LearnerProfile> learnerProfiles)
    {
        _enrollments = enrollments;
        _formations = formations;
        _modules = modules;
        _quizzes = quizzes;
        _activityLogs = activityLogs;
        _learnerProfiles = learnerProfiles;
    }

    public Task<List<MyEnrollmentDto>> GetMyEnrollmentsAsync(int learnerId, CancellationToken cancellationToken = default) =>
        _enrollments.GetMyEnrollmentSummariesAsync(learnerId, cancellationToken);

    public async Task<FormationLearnerDetailDto?> GetFormationDetailAsync(int learnerId, int formationId, CancellationToken cancellationToken = default)
    {
        var catalog = await _formations.GetCatalogDetailAsync(formationId, cancellationToken);
        if (catalog is null) return null;

        var enrollment = await _enrollments.GetByLearnerAndFormationAsync(learnerId, formationId, cancellationToken);
        var completedModuleIds = enrollment?.ModuleProgresses.Where(mp => mp.IsCompleted).Select(mp => mp.ModuleId).ToHashSet() ?? new HashSet<int>();

        // The final exam lives on the Formation itself (ModuleId == null), not a specific module —
        // resolve it separately so it isn't confused with a module's own practice quiz.
        var finalExam = await _quizzes.GetFinalExamByFormationIdAsync(formationId, cancellationToken);

        var detail = new FormationLearnerDetailDto
        {
            FormationId = catalog.FormationId,
            Title = catalog.Title,
            Slug = catalog.Slug,
            ShortDescription = catalog.ShortDescription,
            FullDescription = catalog.FullDescription,
            PrerequisitesText = catalog.PrerequisitesText,
            CategoryName = catalog.CategoryName,
            CategoryColorHex = catalog.CategoryColorHex,
            TrainerName = catalog.TrainerName,
            Difficulty = catalog.Difficulty,
            EstimatedMinutes = catalog.EstimatedMinutes,
            AverageRating = catalog.AverageRating,
            HasCertificate = catalog.OffersCertificate,
            IsEnrolled = enrollment is not null,
            Status = enrollment?.Status,
            ProgressPercentage = enrollment?.ProgressPercentage ?? 0,
            Modules = catalog.Modules.Select((m, index) => new ModuleProgressDto
            {
                ModuleId = m.ModuleId,
                ModuleTitle = m.Title,
                ModuleDescription = m.Description,
                DisplayOrder = m.DisplayOrder,
                EstimatedMinutes = m.EstimatedMinutes,
                IsCompleted = completedModuleIds.Contains(m.ModuleId),
                IsUnlocked = enrollment is not null && (index == 0 || completedModuleIds.Contains(catalog.Modules[index - 1].ModuleId)),
                ContentItems = m.ContentItems,
                HasQuiz = m.HasQuiz,
                QuizId = m.QuizId
            }).ToList(),
            HasFinalExam = finalExam is not null,
            FinalExamQuizId = finalExam?.Id
        };

        detail.AllModulesCompleted = detail.Modules.Count > 0 && detail.Modules.All(m => m.IsCompleted);
        return detail;
    }

    public async Task<ModulePlayerDto?> GetModulePlayerAsync(int learnerId, int moduleId, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetWithFormationAsync(moduleId, cancellationToken);
        if (module is null) return null;

        var enrollment = await _enrollments.GetByLearnerAndFormationAsync(learnerId, module.FormationId, cancellationToken);
        var isCompleted = enrollment?.ModuleProgresses.Any(mp => mp.ModuleId == moduleId && mp.IsCompleted) ?? false;
        var moduleQuiz = module.Quizzes.FirstOrDefault();

        return new ModulePlayerDto
        {
            ModuleId = module.Id,
            FormationId = module.FormationId,
            FormationTitle = module.Formation.Title,
            ModuleTitle = module.Title,
            ModuleDescription = module.Description,
            ContentItems = module.ContentItems.OrderBy(c => c.DisplayOrder).Select(c => new Catalog.Models.ContentItemDto
            {
                Id = c.Id,
                ModuleId = c.ModuleId,
                Type = c.Type,
                Title = c.Title,
                ExternalUrl = c.ExternalUrl,
                DurationMinutes = c.DurationMinutes,
                DisplayOrder = c.DisplayOrder
            }).ToList(),
            IsCompleted = isCompleted,
            HasQuiz = moduleQuiz is not null,
            QuizId = moduleQuiz?.Id
        };
    }

    public async Task EnrollAsync(int learnerId, int formationId, CancellationToken cancellationToken = default)
    {
        var existing = await _enrollments.GetByLearnerAndFormationAsync(learnerId, formationId, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("Already enrolled in this formation.");

        var formation = await _formations.GetByIdAsync(formationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Formation {formationId} not found.");

        var modules = await _modules.GetByFormationIdAsync(formationId, cancellationToken);

        var enrollment = new Enrollment
        {
            LearnerId = learnerId,
            FormationId = formationId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.InProgress,
            ProgressPercentage = 0,
            LastAccessedAt = DateTime.UtcNow,
            ModuleProgresses = modules.Select(m => new ModuleProgress
            {
                ModuleId = m.Id,
                IsCompleted = false,
                TimeSpentMinutes = 0
            }).ToList()
        };

        await _enrollments.AddAsync(enrollment, cancellationToken);

        formation.EnrollmentCount += 1;
        _formations.Update(formation);

        await _enrollments.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkModuleCompleteAsync(int learnerId, int moduleId, CancellationToken cancellationToken = default)
    {
        var module = await _modules.GetByIdAsync(moduleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Module {moduleId} not found.");

        var enrollment = await _enrollments.GetByLearnerAndFormationAsync(learnerId, module.FormationId, cancellationToken)
            ?? throw new InvalidOperationException("Not enrolled in this formation.");

        var progress = enrollment.ModuleProgresses.FirstOrDefault(mp => mp.ModuleId == moduleId)
            ?? throw new InvalidOperationException("This module is not part of the learner's enrollment.");

        var wasAlreadyCompleted = progress.IsCompleted;
        progress.IsCompleted = true;
        progress.CompletedAt ??= DateTime.UtcNow;

        var totalModules = enrollment.ModuleProgresses.Count;
        var completedModules = enrollment.ModuleProgresses.Count(mp => mp.IsCompleted);
        enrollment.ProgressPercentage = totalModules == 0 ? 0 : Math.Round(completedModules * 100m / totalModules, 1);
        enrollment.LastAccessedAt = DateTime.UtcNow;

        if (enrollment.ProgressPercentage >= 100)
            enrollment.Status = EnrollmentStatus.Completed;

        _enrollments.Update(enrollment);
        await _enrollments.SaveChangesAsync(cancellationToken);

        if (!wasAlreadyCompleted)
        {
            await RecordActivityAsync(learnerId, module.EstimatedMinutes, cancellationToken);
        }
    }

    private async Task RecordActivityAsync(int learnerId, int minutesSpent, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayLog = await _activityLogs.GetForDateAsync(learnerId, today, cancellationToken);

        var isFirstActivityToday = todayLog is null;

        if (todayLog is null)
        {
            // Newly Add()-ed entities are already tracked for insert — calling Update() on them
            // before they're saved throws (EF can't move a still-temporary-keyed entity to Modified).
            todayLog = new ActivityLog { LearnerId = learnerId, ActivityDate = today, MinutesSpent = minutesSpent, ModulesCompletedCount = 1, QuizAttemptsCount = 0 };
            await _activityLogs.AddAsync(todayLog, cancellationToken);
        }
        else
        {
            todayLog.MinutesSpent += minutesSpent;
            todayLog.ModulesCompletedCount += 1;
            _activityLogs.Update(todayLog);
        }

        if (isFirstActivityToday)
        {
            var learner = await _learnerProfiles.GetByIdAsync(learnerId, cancellationToken);
            if (learner is not null)
            {
                var yesterday = today.AddDays(-1);
                var hadActivityYesterday = await _activityLogs.GetForDateAsync(learnerId, yesterday, cancellationToken) is not null;

                learner.CurrentStreakDays = hadActivityYesterday ? learner.CurrentStreakDays + 1 : 1;
                learner.LongestStreakDays = Math.Max(learner.LongestStreakDays, learner.CurrentStreakDays);
                learner.TotalBenchMinutes += minutesSpent;

                _learnerProfiles.Update(learner);
            }
        }

        await _activityLogs.SaveChangesAsync(cancellationToken);
    }
}
