using Domain.Entities;
using Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.ToTable("Municipalities");
        ConfigureName(builder);
        builder.HasIndex(x => x.NormalizedName).IsUnique();
    }

    internal static void ConfigureName<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : BaseAuditableEntity
    {
        builder.Property("Name").HasMaxLength(Municipality.MaxNameLength).IsRequired();
        builder.Property("NormalizedName").HasMaxLength(Municipality.MaxNameLength).IsRequired();
    }
}

public sealed class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.ToTable("Settlements");
        MunicipalityConfiguration.ConfigureName(builder);
        builder.HasOne(x => x.Municipality).WithMany().HasForeignKey(x => x.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.MunicipalityId, x.NormalizedName }).IsUnique();
    }
}

public sealed class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        MunicipalityConfiguration.ConfigureName(builder);
        builder.HasOne(x => x.Settlement).WithMany().HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SettlementId, x.NormalizedName }).IsUnique();
    }
}

public sealed class QuarterConfiguration : IEntityTypeConfiguration<Quarter>
{
    public void Configure(EntityTypeBuilder<Quarter> builder)
    {
        builder.ToTable("Quarters");
        MunicipalityConfiguration.ConfigureName(builder);
        builder.HasOne(x => x.Settlement).WithMany().HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.SettlementId, x.NormalizedName }).IsUnique();
    }
}
