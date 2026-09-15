using Domain.Entities;
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

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SiteWatchActivityCatalogModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
