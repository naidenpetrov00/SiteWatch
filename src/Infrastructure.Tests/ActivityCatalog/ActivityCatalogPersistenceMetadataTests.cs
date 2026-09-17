using Domain.Entities;
using Domain.SeedWork.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NSubstitute;

namespace Infrastructure.Tests.ActivityCatalog;

public sealed class ActivityCatalogPersistenceMetadataTests 
{
    [Fact]
    public void Catalog_node_mapping_preserves_hierarchy_discriminator_and_sibling_constraints()
    {
        using var dbContext = CreateDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(ActivityCatalogNode))!;
        var foreignKey = Assert.Single(entity.GetForeignKeys().Where(foreignKey =>
            foreignKey.Properties.Single().Name == nameof(ActivityCatalogNode.ParentFolderId)));

        Assert.Equal("ActivityCatalogNodes", entity.GetTableName());
        Assert.Equal(nameof(ActivityCatalogNode.ParentFolderId), foreignKey.Properties.Single().Name);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.NotNull(entity.FindProperty("NodeType"));
        Assert.Equal("folder", dbContext.Model.FindEntityType(typeof(ActivityFolder))!.GetDiscriminatorValue());
        Assert.Equal("activity", dbContext.Model.FindEntityType(typeof(Activity))!.GetDiscriminatorValue());

        var name = entity.FindProperty(nameof(ActivityCatalogNode.Name))!;
        var normalizedName = entity.FindProperty(nameof(ActivityCatalogNode.NormalizedName))!;
        Assert.False(name.IsNullable);
        Assert.Equal(ActivityCatalogNode.MaxNameLength, name.GetMaxLength());
        Assert.False(normalizedName.IsNullable);
        Assert.Equal(ActivityCatalogNode.MaxNameLength, normalizedName.GetMaxLength());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ActivityCatalogNode.ParentFolderId), nameof(ActivityCatalogNode.NormalizedName)]));
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(ActivityCatalogNode.ParentFolderId), nameof(ActivityCatalogNode.SortOrder)]));
    }

    [Fact]
    public void Activity_mapping_preserves_generated_number_status_and_description_contracts()
    {
        using var dbContext = CreateDbContext();
        var entity = dbContext.Model.FindEntityType(typeof(Activity))!;
        var numberId = entity.FindProperty(nameof(Activity.NumberId))!;
        var description = entity.FindProperty(nameof(Activity.Description))!;
        var status = entity.FindProperty(nameof(Activity.Status))!;

        Assert.Equal("NEXT VALUE FOR [dbo].[ActivityNumberIds]", numberId.GetDefaultValueSql());
        Assert.Equal(ValueGenerated.OnAdd, numberId.ValueGenerated);
        Assert.Equal(Activity.MaxDescriptionLength, description.GetMaxLength());
        Assert.Equal(32, status.GetMaxLength());
        Assert.False(status.IsNullable);
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(Activity.NumberId)]));
        Assert.Contains(entity.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(Activity.Status)]));
        Assert.NotNull(dbContext.Model.FindSequence("ActivityNumberIds", "dbo"));
    }

    [Fact]
    public void Requirement_mappings_preserve_storage_relationship_and_uniqueness_contracts()
    {
        using var dbContext = CreateDbContext();
        var section = dbContext.Model.FindEntityType(typeof(ActivityRequirementSection))!;
        var requirement = dbContext.Model.FindEntityType(typeof(ActivityProductRequirement))!;

        Assert.Equal("ActivityRequirementSections", section.GetTableName());
        Assert.Equal(ActivityRequirementSection.MaxNameLength, section.FindProperty(nameof(ActivityRequirementSection.Name))!.GetMaxLength());
        Assert.Equal(18, section.FindProperty(nameof(ActivityRequirementSection.BasisQuantity))!.GetPrecision());
        Assert.Equal(4, section.FindProperty(nameof(ActivityRequirementSection.BasisQuantity))!.GetScale());
        Assert.Equal("m2", section.FindProperty(nameof(ActivityRequirementSection.MeasurementUnit))!.GetValueConverter()!.ConvertToProvider(ActivityMeasurementUnit.SquareMeter));
        Assert.Contains(section.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(Activity) && key.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(section.GetIndexes(), index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(ActivityRequirementSection.ActivityId), nameof(ActivityRequirementSection.SortOrder)]));

        Assert.Equal("ActivityProductRequirements", requirement.GetTableName());
        Assert.Equal(18, requirement.FindProperty(nameof(ActivityProductRequirement.Quantity))!.GetPrecision());
        Assert.Equal(4, requirement.FindProperty(nameof(ActivityProductRequirement.Quantity))!.GetScale());
        Assert.Equal(ActivityProductRequirement.MaxNotesLength, requirement.FindProperty(nameof(ActivityProductRequirement.Notes))!.GetMaxLength());
        Assert.Equal("fixed", requirement.FindProperty(nameof(ActivityProductRequirement.QuantityBehavior))!.GetValueConverter()!.ConvertToProvider(ProductQuantityBehavior.Fixed));
        Assert.Contains(requirement.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(ActivityRequirementSection) && key.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(requirement.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(Product) && key.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(requirement.GetIndexes(), index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual([nameof(ActivityProductRequirement.SectionId), nameof(ActivityProductRequirement.ProductId)]));
        Assert.Contains(requirement.GetIndexes(), index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(ActivityProductRequirement.SectionId), nameof(ActivityProductRequirement.SortOrder)]));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SiteWatchActivityCatalogModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
