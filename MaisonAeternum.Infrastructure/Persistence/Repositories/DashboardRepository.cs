using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Common.Models;
using MaisonAeternum.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = new DashboardSnapshotDto
        {
            TotalLearners = await _context.LearnerProfiles.CountAsync(cancellationToken),
            TotalTrainers = await _context.TrainerProfiles.CountAsync(cancellationToken),
            TotalFormations = await _context.Formations.CountAsync(cancellationToken),
            TotalCategories = await _context.Categories.CountAsync(cancellationToken),
            ActiveFormations = await _context.Formations.CountAsync(f => f.Status == FormationStatus.Published, cancellationToken),
            ArchivedFormations = await _context.Formations.CountAsync(f => f.Status == FormationStatus.Archived, cancellationToken),
            CertificatesIssued = await _context.Certificates.CountAsync(c => !c.IsRevoked, cancellationToken)
        };

        var totalEnrollments = await _context.Enrollments.CountAsync(cancellationToken);
        var completedEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed, cancellationToken);
        snapshot.CompletionRatePercentage = totalEnrollments == 0 ? 0 : Math.Round(completedEnrollments * 100m / totalEnrollments, 1);

        snapshot.RecentRegistrations = await (
            from learner in _context.LearnerProfiles.AsNoTracking()
            join user in _context.Users.AsNoTracking() on learner.UserId equals user.Id
            join rank in _context.GuildRanks.AsNoTracking() on learner.GuildRankId equals rank.Id
            orderby learner.MemberSince descending
            select new RecentRegistrationDto
            {
                FullName = user.FirstName + " " + user.LastName,
                JoinedAt = learner.MemberSince,
                RankName = rank.Name
            }).Take(6).ToListAsync(cancellationToken);

        snapshot.RecentQuizAttempts = await (
            from attempt in _context.QuizAttempts.AsNoTracking()
            join quiz in _context.Quizzes.AsNoTracking() on attempt.QuizId equals quiz.Id
            join learner in _context.LearnerProfiles.AsNoTracking() on attempt.LearnerId equals learner.Id
            join user in _context.Users.AsNoTracking() on learner.UserId equals user.Id
            where attempt.SubmittedAt != null
            orderby attempt.SubmittedAt descending
            select new RecentQuizAttemptDto
            {
                LearnerName = user.FirstName + " " + user.LastName,
                QuizTitle = quiz.Title,
                ScorePercentage = attempt.ScorePercentage,
                Passed = attempt.Passed,
                SubmittedAt = attempt.SubmittedAt
            }).Take(6).ToListAsync(cancellationToken);

        var trendStart = DateTime.UtcNow.Date.AddDays(-29);
        var enrollmentsByDay = await _context.Enrollments.AsNoTracking()
            .Where(e => e.EnrolledAt >= trendStart)
            .GroupBy(e => e.EnrolledAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        snapshot.EnrollmentTrend = Enumerable.Range(0, 30)
            .Select(offset => trendStart.AddDays(offset))
            .Select(date => new DailyCountDto
            {
                Date = DateOnly.FromDateTime(date),
                Count = enrollmentsByDay.FirstOrDefault(e => e.Date == date)?.Count ?? 0
            })
            .ToList();

        snapshot.CategoryPopularity = await _context.Categories.AsNoTracking()
            .Select(c => new NamedCountDto
            {
                Name = c.Name,
                Count = c.Formations.Sum(f => f.EnrollmentCount),
                ColorHex = c.ColorHex
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync(cancellationToken);

        var heatmapStart = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-89));
        var activityByDay = await _context.ActivityLogs.AsNoTracking()
            .Where(a => a.ActivityDate >= heatmapStart)
            .GroupBy(a => a.ActivityDate)
            .Select(g => new { Date = g.Key, Minutes = g.Sum(a => a.MinutesSpent) })
            .ToListAsync(cancellationToken);

        snapshot.ActivityHeatmap = Enumerable.Range(0, 90)
            .Select(offset => heatmapStart.AddDays(offset))
            .Select(date => new DailyCountDto
            {
                Date = date,
                Count = activityByDay.FirstOrDefault(a => a.Date == date)?.Minutes ?? 0
            })
            .ToList();

        return snapshot;
    }
}
