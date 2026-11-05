using Domain.Entities;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NSubstitute;

namespace Infrastructure.Tests.TerritorialStructure;

public sealed class TerritorialStructurePersistenceMetadataTests
{
    [Fact]
    public void Territorial_mappings_preserve_names_unique_sibling_scopes_and_restricted_hierarchy()
    {
        using var dbContext = CreateDbContext();
        var municipality = dbContext.Model.FindEntityType(typeof(Municipality))!;
        var settlement = dbContext.Model.FindEntityType(typeof(Settlement))!;
        var district = dbContext.Model.FindEntityType(typeof(District))!;
        var quarter = dbContext.Model.FindEntityType(typeof(Quarter))!;

        AssertNameContract(municipality, "Municipalities");
        AssertNameContract(settlement, "Settlements");
        AssertNameContract(district, "Districts");
        AssertNameContract(quarter, "Quarters");

        AssertUniqueIndex(municipality, nameof(Municipality.NormalizedName));
        AssertUniqueIndex(settlement, nameof(Settlement.MunicipalityId), nameof(Settlement.NormalizedName));
        AssertUniqueIndex(district, nameof(District.SettlementId), nameof(District.NormalizedName));
        AssertUniqueIndex(quarter, nameof(Quarter.SettlementId), nameof(Quarter.NormalizedName));

        AssertRestrictedForeignKey(settlement, nameof(Settlement.MunicipalityId), typeof(Municipality));
        AssertRestrictedForeignKey(district, nameof(District.SettlementId), typeof(Settlement));
        AssertRestrictedForeignKey(quarter, nameof(Quarter.SettlementId), typeof(Settlement));
    }

    [Fact]
    public void Person_address_mapping_keeps_all_territorial_links_optional_and_restricted()
    {
        using var dbContext = CreateDbContext();
        var address = dbContext.Model.FindEntityType(typeof(PersonAddress))!;

        AssertOptionalRestrictedForeignKey(address, nameof(PersonAddress.MunicipalityId), typeof(Municipality));
        AssertOptionalRestrictedForeignKey(address, nameof(PersonAddress.SettlementId), typeof(Settlement));
        AssertOptionalRestrictedForeignKey(address, nameof(PersonAddress.DistrictId), typeof(District));
        AssertOptionalRestrictedForeignKey(address, nameof(PersonAddress.QuarterId), typeof(Quarter));
    }

    private static void AssertNameContract(IEntityType entity, string tableName)
    {
        Assert.Equal(tableName, entity.GetTableName());
        Assert.Equal(Municipality.MaxNameLength, entity.FindProperty("Name")!.GetMaxLength());
        Assert.False(entity.FindProperty("Name")!.IsNullable);
        Assert.Equal(Municipality.MaxNameLength, entity.FindProperty("NormalizedName")!.GetMaxLength());
        Assert.False(entity.FindProperty("NormalizedName")!.IsNullable);
    }

    private static void AssertUniqueIndex(IEntityType entity, params string[] propertyNames)
    {
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));
    }

    private static void AssertRestrictedForeignKey(IEntityType entity, string propertyName, Type principalType)
    {
        var foreignKey = Assert.Single(entity.GetForeignKeys().Where(key =>
            key.Properties.Single().Name == propertyName));
        Assert.Equal(principalType, foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    private static void AssertOptionalRestrictedForeignKey(
        IEntityType entity,
        string propertyName,
        Type principalType)
    {
        Assert.True(entity.FindProperty(propertyName)!.IsNullable);
        AssertRestrictedForeignKey(entity, propertyName, principalType);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SiteWatchTerritorialModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
