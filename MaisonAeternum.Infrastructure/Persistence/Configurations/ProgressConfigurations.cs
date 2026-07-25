using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaisonAeternum.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasIndex(e => new { e.LearnerId, e.FormationId }).IsUnique();
        builder.Property(e => e.ProgressPercentage).HasPrecision(5, 2);

        builder.HasOne(e => e.Learner)
            .WithMany(l => l.Enrollments)
            .HasForeignKey(e => e.LearnerId);

        builder.HasOne(e => e.Formation)
            .WithMany(f => f.Enrollments)
            .HasForeignKey(e => e.FormationId);

        builder.HasMany(e => e.ModuleProgresses)
            .WithOne(mp => mp.Enrollment)
            .HasForeignKey(mp => mp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ModuleProgressConfiguration : IEntityTypeConfiguration<ModuleProgress>
{
    public void Configure(EntityTypeBuilder<ModuleProgress> builder)
    {
        builder.HasIndex(mp => new { mp.EnrollmentId, mp.ModuleId }).IsUnique();

        builder.HasOne(mp => mp.Module)
            .WithMany(m => m.ModuleProgresses)
            .HasForeignKey(mp => mp.ModuleId);
    }
}

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasIndex(c => c.VerificationToken).IsUnique();
        builder.HasIndex(c => c.CertificateNumber).IsUnique();
        builder.Property(c => c.CertificateNumber).HasMaxLength(30).IsRequired();

        builder.HasOne(c => c.Learner)
            .WithMany(l => l.Certificates)
            .HasForeignKey(c => c.LearnerId);

        builder.HasOne(c => c.Formation)
            .WithMany(f => f.Certificates)
            .HasForeignKey(c => c.FormationId);

        builder.HasOne(c => c.QuizAttempt)
            .WithOne(a => a.Certificate)
            .HasForeignKey<Certificate>(c => c.QuizAttemptId);

        builder.HasOne(c => c.GuildRank)
            .WithMany(r => r.Certificates)
            .HasForeignKey(c => c.GuildRankId);

        builder.HasOne(c => c.GeneratedPdf)
            .WithMany()
            .HasForeignKey(c => c.GeneratedPdfId);
    }
}

public class LearnerBadgeConfiguration : IEntityTypeConfiguration<LearnerBadge>
{
    public void Configure(EntityTypeBuilder<LearnerBadge> builder)
    {
        builder.HasKey(lb => new { lb.LearnerId, lb.BadgeId });

        builder.HasOne(lb => lb.Learner)
            .WithMany(l => l.LearnerBadges)
            .HasForeignKey(lb => lb.LearnerId);

        builder.HasOne(lb => lb.Badge)
            .WithMany(b => b.LearnerBadges)
            .HasForeignKey(lb => lb.BadgeId);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasIndex(a => new { a.LearnerId, a.ActivityDate }).IsUnique();

        builder.HasOne(a => a.Learner)
            .WithMany(l => l.ActivityLogs)
            .HasForeignKey(a => a.LearnerId);
    }
}

public class FormationFavoriteConfiguration : IEntityTypeConfiguration<FormationFavorite>
{
    public void Configure(EntityTypeBuilder<FormationFavorite> builder)
    {
        builder.HasKey(f => new { f.LearnerId, f.FormationId });

        builder.HasOne(f => f.Learner)
            .WithMany(l => l.Favorites)
            .HasForeignKey(f => f.LearnerId);

        builder.HasOne(f => f.Formation)
            .WithMany(fo => fo.Favorites)
            .HasForeignKey(f => f.FormationId);
    }
}

public class FormationReviewConfiguration : IEntityTypeConfiguration<FormationReview>
{
    public void Configure(EntityTypeBuilder<FormationReview> builder)
    {
        builder.HasIndex(r => new { r.LearnerId, r.FormationId }).IsUnique();

        builder.HasOne(r => r.Learner)
            .WithMany(l => l.Reviews)
            .HasForeignKey(r => r.LearnerId);

        builder.HasOne(r => r.Formation)
            .WithMany(f => f.Reviews)
            .HasForeignKey(r => r.FormationId);
    }
}
