using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class LearnerProfile : AuditableEntity
{
    public string UserId { get; set; } = default!;
    public int GuildRankId { get; set; }
    public string MemberNumber { get; set; } = default!;
    public DateTime MemberSince { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LongestStreakDays { get; set; }
    public int TotalBenchMinutes { get; set; }

    public GuildRank GuildRank { get; set; } = default!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<LearnerBadge> LearnerBadges { get; set; } = new List<LearnerBadge>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<FormationFavorite> Favorites { get; set; } = new List<FormationFavorite>();
    public ICollection<ContentBookmark> Bookmarks { get; set; } = new List<ContentBookmark>();
    public ICollection<FormationReview> Reviews { get; set; } = new List<FormationReview>();
    public ICollection<AIConversation> AiConversations { get; set; } = new List<AIConversation>();
}
