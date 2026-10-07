using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable(
            "Proposals",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Proposals_RevisionNumber",
                    "[RevisionNumber] > 0");
                table.HasCheckConstraint(
                    "CK_Proposals_Discounts",
                    "[ActivityDiscountPercentage] >= 0 AND [ActivityDiscountPercentage] <= 100 "
                    + "AND [ProductDiscountPercentage] >= 0 AND [ProductDiscountPercentage] <= 100");
                table.HasCheckConstraint(
                    "CK_Proposals_UnpricedOptionalItemCount",
                    "[UnpricedOptionalItemCount] >= 0");
            });

        builder.Property(proposal => proposal.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[ProposalNumberIds]")
            .IsRequired();
        builder.Property(proposal => proposal.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(proposal => proposal.SiteName).HasMaxLength(200).IsRequired();
        builder.Property(proposal => proposal.SiteAddress).HasMaxLength(200).IsRequired();
        builder.Property(proposal => proposal.RecipientUserId).HasMaxLength(450).IsRequired();
        builder.Property(proposal => proposal.RecipientDisplayName).HasMaxLength(450).IsRequired();
        builder.Property(proposal => proposal.RecipientEmail).HasMaxLength(256).IsRequired();
        builder.Property(proposal => proposal.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(proposal => proposal.PublicNotes)
            .HasMaxLength(Proposal.MaxPublicNotesLength);
        builder.Property(proposal => proposal.PaymentTerms)
            .HasMaxLength(Proposal.MaxPaymentTermsLength);
        builder.Property(proposal => proposal.IssuedBy).HasMaxLength(450);
        builder.Property(proposal => proposal.ActivitySubtotalBeforeDiscount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.ActivityDiscountPercentage).HasPrecision(5, 2);
        builder.Property(proposal => proposal.ActivityDiscountAmount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.ActivityTotalAfterDiscount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.ProductSubtotalBeforeDiscount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.ProductDiscountPercentage).HasPrecision(5, 2);
        builder.Property(proposal => proposal.ProductDiscountAmount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.ProductTotalAfterDiscount).HasPrecision(18, 2);
        builder.Property(proposal => proposal.Total).HasPrecision(18, 2);
        builder.Property(proposal => proposal.RowVersion).IsRowVersion();
        builder.Ignore(proposal => proposal.ExcludesUnpricedOptionalItems);

        builder.HasIndex(proposal => new { proposal.SourceOfferId, proposal.RevisionNumber })
            .IsUnique();
        builder.HasIndex(proposal => new { proposal.NumberId, proposal.RevisionNumber })
            .IsUnique();
        builder.HasIndex(proposal => proposal.SourceOfferId)
            .IsUnique()
            .HasFilter("[Status] = N'Draft'");
        builder.HasIndex(proposal => new { proposal.SiteId, proposal.NumberId });

        builder.HasOne(proposal => proposal.Site)
            .WithMany()
            .HasForeignKey(proposal => proposal.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(proposal => proposal.SourceOffer)
            .WithMany()
            .HasForeignKey(proposal => proposal.SourceOfferId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(proposal => proposal.Document)
            .WithOne(document => document.Proposal)
            .HasForeignKey<ProposalDocument>(document => document.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProposalActivityConfiguration
    : IEntityTypeConfiguration<ProposalActivity>
{
    public void Configure(EntityTypeBuilder<ProposalActivity> builder)
    {
        builder.ToTable("ProposalActivities");
        builder.Property(activity => activity.Name).HasMaxLength(200).IsRequired();
        builder.Property(activity => activity.Description).HasMaxLength(2000);
        builder.HasIndex(activity => new { activity.ProposalId, activity.SortOrder });
        builder.HasOne(activity => activity.Proposal)
            .WithMany(proposal => proposal.Activities)
            .HasForeignKey(activity => activity.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProposalActivitySectionConfiguration
    : IEntityTypeConfiguration<ProposalActivitySection>
{
    public void Configure(EntityTypeBuilder<ProposalActivitySection> builder)
    {
        builder.ToTable("ProposalActivitySections");
        builder.Property(section => section.Name).HasMaxLength(200);
        builder.Property(section => section.BasisQuantity).HasPrecision(18, 4);
        builder.Property(section => section.RequestedMeasurement).HasPrecision(18, 4);
        builder.Property(section => section.PriceAmount).HasPrecision(18, 2);
        builder.Property(section => section.CalculatedTotal).HasPrecision(18, 2);
        builder.Property(section => section.MeasurementUnit)
            .HasConversion(
                unit => unit.ToCode(),
                value => ParseMeasurementUnit(value))
            .HasMaxLength(32);
        builder.Property(section => section.PricingMode)
            .HasConversion(
                mode => mode.ToCode(),
                value => ParsePricingMode(value))
            .HasMaxLength(32);
        builder.HasIndex(section => new { section.ProposalActivityId, section.SortOrder });
        builder.HasOne(section => section.ProposalActivity)
            .WithMany(activity => activity.Sections)
            .HasForeignKey(section => section.ProposalActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static ActivityMeasurementUnit ParseMeasurementUnit(string value) =>
        ActivityMeasurementUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored activity measurement unit '{value}'.");

    private static ActivityPricingMode ParsePricingMode(string value) =>
        ActivityPricingModeCodes.TryParse(value, out var mode)
            ? mode
            : throw new InvalidOperationException(
                $"Unsupported stored activity pricing mode '{value}'.");
}

public sealed class ProposalProductLineConfiguration
    : IEntityTypeConfiguration<ProposalProductLine>
{
    public void Configure(EntityTypeBuilder<ProposalProductLine> builder)
    {
        builder.ToTable("ProposalProductLines");
        builder.Property(line => line.Title).HasMaxLength(200).IsRequired();
        builder.Property(line => line.Brand).HasMaxLength(100);
        builder.Property(line => line.Model).HasMaxLength(100);
        builder.Property(line => line.PackageQuantity).HasPrecision(18, 4);
        builder.Property(line => line.RequiredQuantity).HasPrecision(28, 8);
        builder.Property(line => line.OptionalQuantity).HasPrecision(28, 8);
        builder.Property(line => line.SelectedRetailerDisplayName).HasMaxLength(200);
        builder.Property(line => line.SelectedPriceAmount).HasPrecision(18, 2);
        builder.Property(line => line.SelectedPriceCurrencyCode).HasMaxLength(3);
        builder.Property(line => line.RequiredTotal).HasPrecision(18, 2);
        builder.Property(line => line.OptionalTotal).HasPrecision(18, 2);
        builder.Property(line => line.Category)
            .HasConversion(category => category.ToCode(), value => ParseCategory(value))
            .HasMaxLength(100);
        var packageUnitConverter = new ValueConverter<ProductPackageUnit?, string?>(
            unit => unit.HasValue ? unit.Value.ToCode() : null,
            value => value == null ? null : ParsePackageUnit(value));
        builder.Property(line => line.PackageUnit)
            .HasConversion(packageUnitConverter)
            .HasMaxLength(50);
        var priceBasisConverter = new ValueConverter<PriceBasis?, string?>(
            basis => basis.HasValue ? basis.Value.ToCode() : null,
            value => value == null ? null : ParsePriceBasis(value));
        builder.Property(line => line.SelectedPriceBasis)
            .HasConversion(priceBasisConverter)
            .HasMaxLength(16);
        builder.HasIndex(line => new { line.ProposalId, line.SortOrder });
        builder.HasOne(line => line.Proposal)
            .WithMany(proposal => proposal.ProductLines)
            .HasForeignKey(line => line.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static ProductCategory ParseCategory(string value) =>
        ProductCategoryCodes.TryParse(value, out var category)
            ? category
            : throw new InvalidOperationException(
                $"Unsupported stored product category '{value}'.");

    private static ProductPackageUnit ParsePackageUnit(string value) =>
        ProductPackageUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored package unit '{value}'.");

    private static PriceBasis ParsePriceBasis(string value) =>
        PriceBasisCodes.TryParse(value, out var basis)
            ? basis
            : throw new InvalidOperationException(
                $"Unsupported stored price basis '{value}'.");
}

public sealed class ProposalDocumentConfiguration
    : IEntityTypeConfiguration<ProposalDocument>
{
    public void Configure(EntityTypeBuilder<ProposalDocument> builder)
    {
        builder.ToTable("ProposalDocuments");
        builder.Property(document => document.BlobName).HasMaxLength(500).IsRequired();
        builder.Property(document => document.FileName).HasMaxLength(255).IsRequired();
        builder.Property(document => document.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(document => document.Sha256).HasMaxLength(64).IsRequired();
        builder.HasIndex(document => document.ProposalId).IsUnique();
        builder.HasIndex(document => document.BlobName).IsUnique();
    }
}
