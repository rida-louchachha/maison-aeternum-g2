using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Application.Learning;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Moq;
using DomainModule = MaisonAeternum.Domain.Entities.Module;

namespace MaisonAeternum.UnitTests.Services;

public class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<IFormationRepository> _formations = new();
    private readonly Mock<IModuleRepository> _modules = new();
    private readonly Mock<IQuizRepository> _quizzes = new();
    private readonly Mock<IActivityLogRepository> _activityLogs = new();
    private readonly Mock<IRepository<LearnerProfile>> _learnerProfiles = new();
    private readonly EnrollmentService _sut;

    public EnrollmentServiceTests()
    {
        _sut = new EnrollmentService(
            _enrollments.Object, _formations.Object, _modules.Object, _quizzes.Object, _activityLogs.Object, _learnerProfiles.Object);
    }

    [Fact]
    public async Task EnrollAsync_Throws_WhenLearnerAlreadyEnrolled()
    {
        _enrollments.Setup(r => r.GetByLearnerAndFormationAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Enrollment { LearnerId = 1, FormationId = 10 });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.EnrollAsync(1, 10));
    }

    [Fact]
    public async Task EnrollAsync_CreatesOneModuleProgressRowPerModule()
    {
        _enrollments.Setup(r => r.GetByLearnerAndFormationAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);
        _formations.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Formation { Id = 10, EnrollmentCount = 0 });
        _modules.Setup(r => r.GetByFormationIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainModule> { new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 } });

        Enrollment? captured = null;
        _enrollments.Setup(r => r.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        await _sut.EnrollAsync(1, 10);

        Assert.NotNull(captured);
        Assert.Equal(3, captured!.ModuleProgresses.Count);
        Assert.Equal(EnrollmentStatus.InProgress, captured.Status);
    }

    [Fact]
    public async Task MarkModuleCompleteAsync_FirstCompletionToday_DoesNotThrowAndPersistsActivity()
    {
        // Regression test: RecordActivityAsync used to call Update() on an ActivityLog it had
        // just Add()-ed in the same call, which EF Core rejects (temporary key). That bug made
        // every "mark module complete" request crash with a 500 the first time a learner was
        // active on a given day.
        var module = new DomainModule { Id = 7, FormationId = 10, EstimatedMinutes = 25 };
        var progress = new ModuleProgress { ModuleId = 7, IsCompleted = false };
        var enrollment = new Enrollment
        {
            LearnerId = 1,
            FormationId = 10,
            Status = EnrollmentStatus.InProgress,
            ModuleProgresses = new List<ModuleProgress> { progress }
        };

        _modules.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(module);
        _enrollments.Setup(r => r.GetByLearnerAndFormationAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _activityLogs.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((ActivityLog?)null);
        _learnerProfiles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LearnerProfile { Id = 1, CurrentStreakDays = 0, LongestStreakDays = 0, TotalBenchMinutes = 0 });

        var exception = await Record.ExceptionAsync(() => _sut.MarkModuleCompleteAsync(1, 7));

        Assert.Null(exception);
        Assert.True(progress.IsCompleted);
        _activityLogs.Verify(r => r.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _activityLogs.Verify(r => r.Update(It.IsAny<ActivityLog>()), Times.Never); // never Update() the just-Added row
    }

    [Fact]
    public async Task MarkModuleCompleteAsync_AllModulesDone_MarksEnrollmentCompleted()
    {
        var module = new DomainModule { Id = 2, FormationId = 10, EstimatedMinutes = 15 };
        var enrollment = new Enrollment
        {
            LearnerId = 1,
            FormationId = 10,
            Status = EnrollmentStatus.InProgress,
            ModuleProgresses = new List<ModuleProgress>
            {
                new() { ModuleId = 1, IsCompleted = true },
                new() { ModuleId = 2, IsCompleted = false }
            }
        };

        _modules.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(module);
        _enrollments.Setup(r => r.GetByLearnerAndFormationAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _activityLogs.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((ActivityLog?)null);
        _learnerProfiles.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new LearnerProfile { Id = 1 });

        await _sut.MarkModuleCompleteAsync(1, 2);

        Assert.Equal(100m, enrollment.ProgressPercentage);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
    }

    [Fact]
    public async Task MarkModuleCompleteAsync_SecondActivityOnSameDay_UpdatesExistingLogInsteadOfAddingNew()
    {
        var module = new DomainModule { Id = 4, FormationId = 10, EstimatedMinutes = 20 };
        var enrollment = new Enrollment
        {
            LearnerId = 1,
            FormationId = 10,
            ModuleProgresses = new List<ModuleProgress> { new() { ModuleId = 4, IsCompleted = false } }
        };
        var existingLog = new ActivityLog { Id = 99, LearnerId = 1, MinutesSpent = 10, ModulesCompletedCount = 1 };

        _modules.Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(module);
        _enrollments.Setup(r => r.GetByLearnerAndFormationAsync(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(enrollment);
        _activityLogs.Setup(r => r.GetForDateAsync(1, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingLog);

        await _sut.MarkModuleCompleteAsync(1, 4);

        Assert.Equal(30, existingLog.MinutesSpent);
        Assert.Equal(2, existingLog.ModulesCompletedCount);
        _activityLogs.Verify(r => r.Update(existingLog), Times.Once);
        _activityLogs.Verify(r => r.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()), Times.Never);
        // Same-day re-activity must not re-touch the streak — GetByIdAsync on the learner is the streak path.
        _learnerProfiles.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
