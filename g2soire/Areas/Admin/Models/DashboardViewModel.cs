using MaisonAeternum.Application.Common.Models;
using System.Text.Json;

namespace MaisonAeternum.Web.Areas.Admin.Models;

public class DashboardViewModel
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

    public string EnrollmentTrendLabelsJson { get; set; } = "[]";
    public string EnrollmentTrendDataJson { get; set; } = "[]";
    public string CategoryLabelsJson { get; set; } = "[]";
    public string CategoryDataJson { get; set; } = "[]";
    public string CategoryColorsJson { get; set; } = "[]";

    public List<List<HeatmapCellViewModel>> HeatmapWeeks { get; set; } = new();

    public static DashboardViewModel FromSnapshot(DashboardSnapshotDto snapshot)
    {
        var vm = new DashboardViewModel
        {
            TotalLearners = snapshot.TotalLearners,
            TotalTrainers = snapshot.TotalTrainers,
            TotalFormations = snapshot.TotalFormations,
            TotalCategories = snapshot.TotalCategories,
            ActiveFormations = snapshot.ActiveFormations,
            ArchivedFormations = snapshot.ArchivedFormations,
            CertificatesIssued = snapshot.CertificatesIssued,
            CompletionRatePercentage = snapshot.CompletionRatePercentage,
            RecentRegistrations = snapshot.RecentRegistrations,
            RecentQuizAttempts = snapshot.RecentQuizAttempts
        };

        vm.EnrollmentTrendLabelsJson = JsonSerializer.Serialize(snapshot.EnrollmentTrend.Select(e => e.Date.ToString("MMM d")));
        vm.EnrollmentTrendDataJson = JsonSerializer.Serialize(snapshot.EnrollmentTrend.Select(e => e.Count));

        vm.CategoryLabelsJson = JsonSerializer.Serialize(snapshot.CategoryPopularity.Select(c => c.Name));
        vm.CategoryDataJson = JsonSerializer.Serialize(snapshot.CategoryPopularity.Select(c => c.Count));
        vm.CategoryColorsJson = JsonSerializer.Serialize(snapshot.CategoryPopularity.Select(c => c.ColorHex));

        vm.HeatmapWeeks = BuildHeatmapWeeks(snapshot.ActivityHeatmap);

        return vm;
    }

    private static List<List<HeatmapCellViewModel>> BuildHeatmapWeeks(List<DailyCountDto> days)
    {
        if (days.Count == 0) return new();

        var maxMinutes = days.Max(d => d.Count);
        var cells = days.Select(d => new HeatmapCellViewModel
        {
            Date = d.Date,
            MinutesSpent = d.Count,
            Level = LevelFor(d.Count, maxMinutes)
        }).ToList();

        // Pad the front so the first column starts on a Sunday, matching a GitHub-style grid.
        var firstDayOfWeek = (int)cells[0].Date!.Value.DayOfWeek;
        var padded = Enumerable.Range(0, firstDayOfWeek)
            .Select(_ => new HeatmapCellViewModel { Date = null, MinutesSpent = 0, Level = 0 })
            .Concat(cells)
            .ToList();

        var weeks = new List<List<HeatmapCellViewModel>>();
        for (var i = 0; i < padded.Count; i += 7)
        {
            weeks.Add(padded.Skip(i).Take(7).ToList());
        }

        return weeks;
    }

    private static int LevelFor(int minutes, int max)
    {
        if (minutes <= 0 || max <= 0) return 0;
        var ratio = (double)minutes / max;
        return ratio switch
        {
            >= 0.75 => 4,
            >= 0.5 => 3,
            >= 0.25 => 2,
            _ => 1
        };
    }
}

public class HeatmapCellViewModel
{
    public DateOnly? Date { get; set; }
    public int MinutesSpent { get; set; }
    public int Level { get; set; }
}
