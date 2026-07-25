using MaisonAeternum.Application.Catalog.Abstractions;
using MaisonAeternum.Application.Catalog.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Catalog;

public class TrainerProfileService : ITrainerProfileService
{
    private readonly ITrainerRepository _trainers;

    public TrainerProfileService(ITrainerRepository trainers)
    {
        _trainers = trainers;
    }

    public Task<List<TrainerAdminDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _trainers.GetAllForAdminAsync(cancellationToken);

    public async Task<TrainerProfileFormDto?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _trainers.GetWithSocialLinksAsync(id, cancellationToken);
        if (trainer is null) return null;

        return new TrainerProfileFormDto
        {
            Id = trainer.Id,
            Biography = trainer.Biography,
            AtelierAffiliation = trainer.AtelierAffiliation,
            YearsOfExperience = trainer.YearsOfExperience,
            IsFeatured = trainer.IsFeatured,
            SocialLinks = EnsureEditableSlots(trainer.SocialLinks
                .Select(s => new TrainerSocialLinkFormDto { Platform = s.Platform, Url = s.Url })
                .ToList())
        };
    }

    /// <summary>The edit form always shows an Instagram and a LinkedIn row, even if unset, so the form layout is stable.</summary>
    private static List<TrainerSocialLinkFormDto> EnsureEditableSlots(List<TrainerSocialLinkFormDto> existing)
    {
        foreach (var platform in new[] { SocialPlatform.Instagram, SocialPlatform.LinkedIn })
        {
            if (!existing.Any(s => s.Platform == platform))
                existing.Add(new TrainerSocialLinkFormDto { Platform = platform, Url = string.Empty });
        }

        return existing.OrderBy(s => s.Platform).ToList();
    }

    public async Task<int> CreateAsync(string userId, TrainerProfileFormDto form, CancellationToken cancellationToken = default)
    {
        var trainer = new TrainerProfile
        {
            UserId = userId,
            Biography = form.Biography,
            AtelierAffiliation = form.AtelierAffiliation,
            YearsOfExperience = form.YearsOfExperience,
            IsFeatured = form.IsFeatured,
            AverageRating = 0,
            SocialLinks = form.SocialLinks
                .Where(s => !string.IsNullOrWhiteSpace(s.Url))
                .Select(s => new TrainerSocialLink { Platform = s.Platform, Url = s.Url })
                .ToList()
        };

        await _trainers.AddAsync(trainer, cancellationToken);
        await _trainers.SaveChangesAsync(cancellationToken);
        return trainer.Id;
    }

    public async Task UpdateAsync(TrainerProfileFormDto form, CancellationToken cancellationToken = default)
    {
        var trainer = await _trainers.GetWithSocialLinksAsync(form.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Trainer profile {form.Id} not found.");

        trainer.Biography = form.Biography;
        trainer.AtelierAffiliation = form.AtelierAffiliation;
        trainer.YearsOfExperience = form.YearsOfExperience;
        trainer.IsFeatured = form.IsFeatured;

        trainer.SocialLinks.Clear();
        foreach (var link in form.SocialLinks.Where(s => !string.IsNullOrWhiteSpace(s.Url)))
        {
            trainer.SocialLinks.Add(new TrainerSocialLink { Platform = link.Platform, Url = link.Url });
        }

        _trainers.Update(trainer);
        await _trainers.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await _trainers.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Trainer profile {id} not found.");

        _trainers.Remove(trainer);
        await _trainers.SaveChangesAsync(cancellationToken);
    }
}
