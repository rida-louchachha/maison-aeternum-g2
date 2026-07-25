using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaisonAeternum.Infrastructure.Persistence.Configurations;

public class GuildRankConfiguration : IEntityTypeConfiguration<GuildRank>
{
    public void Configure(EntityTypeBuilder<GuildRank> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(60).IsRequired();
    }
}

public class LearnerProfileConfiguration : IEntityTypeConfiguration<LearnerProfile>
{
    public void Configure(EntityTypeBuilder<LearnerProfile> builder)
    {
        builder.HasIndex(l => l.UserId).IsUnique();
        builder.HasIndex(l => l.MemberNumber).IsUnique();
        builder.Property(l => l.MemberNumber).HasMaxLength(20).IsRequired();

        builder.HasOne(l => l.GuildRank)
            .WithMany(r => r.LearnerProfiles)
            .HasForeignKey(l => l.GuildRankId);
    }
}

public class TrainerProfileConfiguration : IEntityTypeConfiguration<TrainerProfile>
{
    public void Configure(EntityTypeBuilder<TrainerProfile> builder)
    {
        builder.HasIndex(t => t.UserId).IsUnique();
        builder.Property(t => t.AverageRating).HasPrecision(3, 2);

        builder.HasMany(t => t.SocialLinks)
            .WithOne(s => s.TrainerProfile)
            .HasForeignKey(s => s.TrainerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AIConversationConfiguration : IEntityTypeConfiguration<AIConversation>
{
    public void Configure(EntityTypeBuilder<AIConversation> builder)
    {
        builder.HasOne(c => c.Learner)
            .WithMany(l => l.AiConversations)
            .HasForeignKey(c => c.LearnerId);

        builder.HasOne(c => c.RelatedFormation)
            .WithMany()
            .HasForeignKey(c => c.RelatedFormationId);

        builder.HasOne(c => c.RelatedModule)
            .WithMany()
            .HasForeignKey(c => c.RelatedModuleId);

        builder.HasOne(c => c.RelatedQuizAttempt)
            .WithMany()
            .HasForeignKey(c => c.RelatedQuizAttemptId);

        builder.HasIndex(c => new { c.LearnerId, c.LastMessageAt });

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasIndex(a => new { a.EntityName, a.EntityId, a.Timestamp });
        builder.HasIndex(a => new { a.UserId, a.Timestamp });
    }
}
