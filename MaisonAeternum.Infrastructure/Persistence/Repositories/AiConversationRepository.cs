using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.Infrastructure.Persistence.Repositories;

public class AiConversationRepository : Repository<AIConversation>, IAiConversationRepository
{
    public AiConversationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AIConversation?> FindActiveConversationAsync(
        int learnerId, AiConversationContext context, int? relatedFormationId, int? relatedModuleId, int? relatedQuizAttemptId,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(c => c.LearnerId == learnerId
                        && c.Context == context
                        && c.IsActive
                        && c.RelatedFormationId == relatedFormationId
                        && c.RelatedModuleId == relatedModuleId
                        && c.RelatedQuizAttemptId == relatedQuizAttemptId)
            .OrderByDescending(c => c.LastMessageAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<AIConversation?> GetWithMessagesAsync(int conversationId, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

    public async Task<List<AIConversation>> GetRecentByLearnerAsync(int learnerId, int count, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(c => c.Messages)
            .Where(c => c.LearnerId == learnerId)
            .OrderByDescending(c => c.LastMessageAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
