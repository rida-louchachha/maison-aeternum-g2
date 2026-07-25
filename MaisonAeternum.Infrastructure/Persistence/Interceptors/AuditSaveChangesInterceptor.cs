using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace MaisonAeternum.Infrastructure.Persistence.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditRules(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    auditLogs.Add(BuildLog(entry, AuditAction.Created, userId, now));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    auditLogs.Add(BuildLog(entry, AuditAction.Updated, userId, now));
                    break;

                case EntityState.Deleted:
                    // Soft delete: never let a physical DELETE reach the database for audited entities.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;
                    auditLogs.Add(BuildLog(entry, AuditAction.SoftDeleted, userId, now));
                    break;
            }
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private AuditLog BuildLog(EntityEntry<AuditableEntity> entry, AuditAction action, string? userId, DateTime now)
    {
        string? oldValues = null;
        string? newValues = null;

        if (action != AuditAction.Created)
        {
            var original = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
            oldValues = JsonSerializer.Serialize(original);
        }

        if (action != AuditAction.SoftDeleted)
        {
            var current = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            newValues = JsonSerializer.Serialize(current);
        }

        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = now,
            IpAddress = _currentUser.IpAddress
        };
    }
}
