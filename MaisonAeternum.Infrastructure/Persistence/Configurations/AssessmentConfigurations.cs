using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaisonAeternum.Infrastructure.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(q => q.Title).HasMaxLength(200).IsRequired();
        builder.Property(q => q.PassingScore).HasPrecision(5, 2);

        builder.HasOne(q => q.Formation)
            .WithMany(f => f.Quizzes)
            .HasForeignKey(q => q.FormationId);

        builder.HasOne(q => q.Module)
            .WithMany(m => m.Quizzes)
            .HasForeignKey(q => q.ModuleId);

        builder.HasMany(q => q.Questions)
            .WithOne(qu => qu.Quiz)
            .HasForeignKey(qu => qu.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(q => q.Text).HasMaxLength(500).IsRequired();

        builder.HasMany(q => q.AnswerOptions)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.Property(a => a.ScorePercentage).HasPrecision(5, 2);

        builder.HasOne(a => a.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(a => a.QuizId);

        builder.HasOne(a => a.Learner)
            .WithMany(l => l.QuizAttempts)
            .HasForeignKey(a => a.LearnerId);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.QuizAttempt)
            .HasForeignKey(ans => ans.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuizAttemptAnswerConfiguration : IEntityTypeConfiguration<QuizAttemptAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAttemptAnswer> builder)
    {
        builder.HasOne(a => a.Question)
            .WithMany(q => q.AttemptAnswers)
            .HasForeignKey(a => a.QuestionId);

        builder.HasMany(a => a.SelectedOptions)
            .WithOne(so => so.QuizAttemptAnswer)
            .HasForeignKey(so => so.QuizAttemptAnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuizAttemptSelectedOptionConfiguration : IEntityTypeConfiguration<QuizAttemptSelectedOption>
{
    public void Configure(EntityTypeBuilder<QuizAttemptSelectedOption> builder)
    {
        builder.HasKey(so => new { so.QuizAttemptAnswerId, so.AnswerOptionId });

        builder.HasOne(so => so.AnswerOption)
            .WithMany()
            .HasForeignKey(so => so.AnswerOptionId);
    }
}
