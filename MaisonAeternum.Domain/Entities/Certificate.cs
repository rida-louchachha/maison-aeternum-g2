using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class Certificate : AuditableEntity
{
    public int LearnerId { get; set; }
    public int FormationId { get; set; }
    public int QuizAttemptId { get; set; }
    public int GuildRankId { get; set; }
    public string CertificateNumber { get; set; } = default!;
    public Guid VerificationToken { get; set; }
    public DateTime IssuedAt { get; set; }
    public int? GeneratedPdfId { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedReason { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Formation Formation { get; set; } = default!;
    public QuizAttempt QuizAttempt { get; set; } = default!;
    public GuildRank GuildRank { get; set; } = default!;
    public MediaFile? GeneratedPdf { get; set; }
}
