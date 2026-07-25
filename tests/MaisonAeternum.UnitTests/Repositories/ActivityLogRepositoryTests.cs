using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Infrastructure.Persistence.Repositories;

namespace MaisonAeternum.UnitTests.Repositories;

public class ActivityLogRepositoryTests
{
    [Fact]
    public async Task GetForDateAsync_ReturnsNull_WhenNoLogExistsForThatDate()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ActivityLogRepository(context);

        var result = await repository.GetForDateAsync(learnerId: 1, DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetForDateAsync_ReturnsTheMatchingLog_AndIgnoresOtherLearnersOrDates()
    {
        using var context = TestDbContextFactory.Create();
        var repository = new ActivityLogRepository(context);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        context.ActivityLogs.AddRange(
            new ActivityLog { LearnerId = 1, ActivityDate = today, MinutesSpent = 30, ModulesCompletedCount = 1, QuizAttemptsCount = 0 },
            new ActivityLog { LearnerId = 1, ActivityDate = today.AddDays(-1), MinutesSpent = 10, ModulesCompletedCount = 1, QuizAttemptsCount = 0 },
            new ActivityLog { LearnerId = 2, ActivityDate = today, MinutesSpent = 99, ModulesCompletedCount = 1, QuizAttemptsCount = 0 });
        await context.SaveChangesAsync();

        var result = await repository.GetForDateAsync(learnerId: 1, today);

        Assert.NotNull(result);
        Assert.Equal(30, result!.MinutesSpent);
    }
}
