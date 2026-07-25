namespace MaisonAeternum.Application.Common.Models;

public class DashboardSnapshotDto
{
    public int TotalLearners { get; set; }
    public int TotalTrainers { get; set; }
    public int TotalFormations { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveFormations { get; set; }
    public int ArchivedFormations { get; set; }
    public int CertificatesIssued { get; set; }
    public decimal CompletionRatePercentage { get; set; }

    public List<RecentRegistrationDto> RecentRegistrations { get; set; } = new();
    public List<RecentQuizAttemptDto> RecentQuizAttempts { get; set; } = new();
    public List<DailyCountDto> EnrollmentTrend { get; set; } = new();
    public List<NamedCountDto> CategoryPopularity { get; set; } = new();
    public List<DailyCountDto> ActivityHeatmap { get; set; } = new();
}

public class RecentRegistrationDto
{
    public string FullName { get; set; } = default!;
    public DateTime JoinedAt { get; set; }
    public string RankName { get; set; } = default!;
}

public class RecentQuizAttemptDto
{
    public string LearnerName { get; set; } = default!;
    public string QuizTitle { get; set; } = default!;
    public decimal ScorePercentage { get; set; }
    public bool Passed { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class DailyCountDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class NamedCountDto
{
    public string Name { get; set; } = default!;
    public int Count { get; set; }
    public string? ColorHex { get; set; }
}
