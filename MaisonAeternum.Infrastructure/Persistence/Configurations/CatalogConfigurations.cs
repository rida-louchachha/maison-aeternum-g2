using MaisonAeternum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaisonAeternum.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
    }
}

public class FormationConfiguration : IEntityTypeConfiguration<Formation>
{
    public void Configure(EntityTypeBuilder<Formation> builder)
    {
        builder.HasIndex(f => f.Slug).IsUnique();
        builder.Property(f => f.Title).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Slug).HasMaxLength(220).IsRequired();
        builder.Property(f => f.AverageRating).HasPrecision(3, 2);

        builder.HasOne(f => f.Category)
            .WithMany(c => c.Formations)
            .HasForeignKey(f => f.CategoryId);

        builder.HasOne(f => f.Trainer)
            .WithMany(t => t.Formations)
            .HasForeignKey(f => f.TrainerId);

        builder.HasOne(f => f.CoverImage)
            .WithMany()
            .HasForeignKey(f => f.CoverImageId);

        builder.HasMany(f => f.Objectives)
            .WithOne(o => o.Formation)
            .HasForeignKey(o => o.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Modules)
            .WithOne(m => m.Formation)
            .HasForeignKey(m => m.FormationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormationPrerequisiteConfiguration : IEntityTypeConfiguration<FormationPrerequisite>
{
    public void Configure(EntityTypeBuilder<FormationPrerequisite> builder)
    {
        builder.HasKey(fp => new { fp.FormationId, fp.RequiredFormationId });

        builder.HasOne(fp => fp.Formation)
            .WithMany(f => f.Prerequisites)
            .HasForeignKey(fp => fp.FormationId);

        builder.HasOne(fp => fp.RequiredFormation)
            .WithMany()
            .HasForeignKey(fp => fp.RequiredFormationId);
    }
}

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();

        builder.HasMany(m => m.ContentItems)
            .WithOne(ci => ci.Module)
            .HasForeignKey(ci => ci.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ModulePrerequisiteConfiguration : IEntityTypeConfiguration<ModulePrerequisite>
{
    public void Configure(EntityTypeBuilder<ModulePrerequisite> builder)
    {
        builder.HasKey(mp => new { mp.ModuleId, mp.RequiredModuleId });

        builder.HasOne(mp => mp.Module)
            .WithMany()
            .HasForeignKey(mp => mp.ModuleId);

        builder.HasOne(mp => mp.RequiredModule)
            .WithMany()
            .HasForeignKey(mp => mp.RequiredModuleId);
    }
}

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.Property(ci => ci.Title).HasMaxLength(200).IsRequired();

        builder.HasOne(ci => ci.MediaFile)
            .WithMany()
            .HasForeignKey(ci => ci.MediaFileId);
    }
}
