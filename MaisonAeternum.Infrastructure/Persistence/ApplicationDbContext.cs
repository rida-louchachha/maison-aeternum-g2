using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MaisonAeternum.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<GuildRank> GuildRanks => Set<GuildRank>();
    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();
    public DbSet<TrainerProfile> TrainerProfiles => Set<TrainerProfile>();
    public DbSet<TrainerSocialLink> TrainerSocialLinks => Set<TrainerSocialLink>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Formation> Formations => Set<Formation>();
    public DbSet<FormationObjective> FormationObjectives => Set<FormationObjective>();
    public DbSet<FormationPrerequisite> FormationPrerequisites => Set<FormationPrerequisite>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<ModulePrerequisite> ModulePrerequisites => Set<ModulePrerequisite>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();
    public DbSet<QuizAttemptSelectedOption> QuizAttemptSelectedOptions => Set<QuizAttemptSelectedOption>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<ModuleProgress> ModuleProgresses => Set<ModuleProgress>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<LearnerBadge> LearnerBadges => Set<LearnerBadge>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<FormationFavorite> FormationFavorites => Set<FormationFavorite>();
    public DbSet<ContentBookmark> ContentBookmarks => Set<ContentBookmark>();
    public DbSet<FormationReview> FormationReviews => Set<FormationReview>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LiveSession> LiveSessions => Set<LiveSession>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<AIMessage> AIMessages => Set<AIMessage>();
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Default every FK to Restrict so SQL Server never rejects the model for
        // "multiple cascade paths" — true parent-owns-child chains opt back into
        // Cascade explicitly inside their IEntityTypeConfiguration.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Global soft-delete filter for every AuditableEntity, applied once via reflection
        // instead of repeating `HasQueryFilter(e => !e.IsDeleted)` on 25+ configurations.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
            var notDeleted = Expression.Not(property);
            var lambda = Expression.Lambda(notDeleted, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
