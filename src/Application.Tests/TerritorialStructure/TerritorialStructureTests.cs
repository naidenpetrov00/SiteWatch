using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Application.TerritorialStructure;
using Domain.Entities;
using NSubstitute;

namespace Application.Tests.TerritorialStructure;

public sealed class TerritorialStructureTests
{
    [Fact]
    public void Territorial_entries_normalize_names_preserve_hierarchy_and_change_active_state()
    {
        var municipality = Municipality.Create("  Stolichna   Municipality ");
        var settlement = Settlement.Create(municipality, "  Sofia ");
        var district = District.Create(settlement, "  Oborishte ");
        var quarter = Quarter.Create(settlement, "  Geo   Milev ");

        Assert.Equal("Stolichna Municipality", municipality.Name);
        Assert.Equal("STOLICHNA MUNICIPALITY", municipality.NormalizedName);
        Assert.Equal(municipality.Id, settlement.MunicipalityId);
        Assert.Equal(settlement.Id, district.SettlementId);
        Assert.Equal(settlement.Id, quarter.SettlementId);
        Assert.Equal("Geo Milev", quarter.Name);

        district.Deactivate();
        Assert.False(district.IsActive);
        district.Activate();
        Assert.True(district.IsActive);
    }

    [Fact]
    public void Territorial_entries_reject_missing_oversized_names_and_missing_parents()
    {
        var municipality = Municipality.Create("Stolichna");

        Assert.Throws<ArgumentException>(() => Municipality.Create(" "));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Municipality.Create(new string('x', Municipality.MaxNameLength + 1)));
        Assert.Throws<ArgumentNullException>(() => Settlement.Create(null!, "Sofia"));
        Assert.Throws<ArgumentNullException>(() => District.Create(null!, "Oborishte"));
        Assert.Throws<ArgumentNullException>(() => Quarter.Create(null!, "Geo Milev"));

        var settlement = Settlement.Create(municipality, "Sofia");
        settlement.Rename("  Sofia   City ");
        Assert.Equal("Sofia City", settlement.Name);
    }

    [Fact]
    public void Person_addresses_default_to_bulgaria_and_preserve_optional_territorial_links()
    {
        var personId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var municipalityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var settlementId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var districtId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var quarterId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        var address = PersonAddress.Create(
            personId,
            country: null,
            municipalityId: municipalityId,
            settlementId: settlementId,
            districtId: districtId,
            quarterId: quarterId,
            street: "  Knyaz Alexander Dondukov  ");

        Assert.Equal("Bulgaria", address.Country);
        Assert.Equal(municipalityId, address.MunicipalityId);
        Assert.Equal(settlementId, address.SettlementId);
        Assert.Equal(districtId, address.DistrictId);
        Assert.Equal(quarterId, address.QuarterId);
        Assert.Equal("Knyaz Alexander Dondukov", address.Street);
    }

    [Fact]
    public async Task Validators_enforce_the_parent_and_identifier_contracts()
    {
        var parentId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var municipality = await new CreateTerritorialEntryValidator().ValidateAsync(
            new CreateTerritorialEntryCommand(TerritorialEntryKind.Municipality, null, "Stolichna"));
        var municipalityWithParent = await new CreateTerritorialEntryValidator().ValidateAsync(
            new CreateTerritorialEntryCommand(TerritorialEntryKind.Municipality, parentId, "Stolichna"));
        var settlementWithoutParent = await new CreateTerritorialEntryValidator().ValidateAsync(
            new CreateTerritorialEntryCommand(TerritorialEntryKind.Settlement, null, "Sofia"));
        var settlement = await new CreateTerritorialEntryValidator().ValidateAsync(
            new CreateTerritorialEntryCommand(TerritorialEntryKind.Settlement, parentId, "Sofia"));
        var query = await new TerritorialEntriesQueryValidator().ValidateAsync(
            new TerritorialEntriesQuery { Kind = TerritorialEntryKind.Quarter, ParentId = null });
        var update = await new UpdateTerritorialEntryValidator().ValidateAsync(
            new UpdateTerritorialEntryCommand { Id = Guid.Empty, Kind = TerritorialEntryKind.District, Name = "" });
        var delete = await new DeleteTerritorialEntryValidator().ValidateAsync(
            new DeleteTerritorialEntryCommand(TerritorialEntryKind.Quarter, Guid.Empty));

        Assert.True(municipality.IsValid);
        Assert.False(municipalityWithParent.IsValid);
        Assert.False(settlementWithoutParent.IsValid);
        Assert.True(settlement.IsValid);
        Assert.Contains(query.Errors, error => error.PropertyName == nameof(TerritorialEntriesQuery.ParentId));
        Assert.Contains(update.Errors, error => error.PropertyName == nameof(UpdateTerritorialEntryCommand.Id));
        Assert.Contains(update.Errors, error => error.PropertyName == nameof(UpdateTerritorialEntryCommand.Name));
        Assert.Contains(delete.Errors, error => error.PropertyName == nameof(DeleteTerritorialEntryCommand.Id));
    }

    [Fact]
    public async Task Handlers_delegate_typed_territorial_contracts_to_the_service()
    {
        var service = Substitute.For<ITerritorialStructureService>();
        var municipalityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settlementId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var entries = new[]
        {
            new TerritorialEntryDto(settlementId, "Sofia", true, municipalityId)
        };
        service.CreateAsync(TerritorialEntryKind.Settlement, municipalityId, "Sofia", CancellationToken.None)
            .Returns(settlementId);
        service.GetEntriesAsync(Arg.Any<TerritorialEntriesQuery>(), CancellationToken.None)
            .Returns(entries);
        var commandHandler = new TerritorialStructureCommandHandler(service);
        var queryHandler = new TerritorialStructureQueryHandler(service);

        var createdId = await commandHandler.Handle(
            new CreateTerritorialEntryCommand(TerritorialEntryKind.Settlement, municipalityId, "Sofia"),
            CancellationToken.None);
        await commandHandler.Handle(new UpdateTerritorialEntryCommand
        {
            Id = settlementId,
            Kind = TerritorialEntryKind.Settlement,
            Name = "Sofia City",
            IsActive = false
        }, CancellationToken.None);
        await commandHandler.Handle(
            new DeleteTerritorialEntryCommand(TerritorialEntryKind.Settlement, settlementId),
            CancellationToken.None);
        var query = new TerritorialEntriesQuery
        {
            Kind = TerritorialEntryKind.Settlement,
            ParentId = municipalityId,
            SearchTerm = "sof",
            IncludeInactive = true
        };
        var result = await queryHandler.Handle(query, CancellationToken.None);

        Assert.Equal(settlementId, createdId);
        Assert.Same(entries, result);
        await service.Received(1).UpdateAsync(
            TerritorialEntryKind.Settlement, settlementId, "Sofia City", false, CancellationToken.None);
        await service.Received(1).DeleteAsync(
            TerritorialEntryKind.Settlement, settlementId, CancellationToken.None);
        await service.Received(1).GetEntriesAsync(query, CancellationToken.None);
    }

    [Theory]
    [InlineData(typeof(CreateTerritorialEntryCommand))]
    [InlineData(typeof(UpdateTerritorialEntryCommand))]
    [InlineData(typeof(DeleteTerritorialEntryCommand))]
    [InlineData(typeof(TerritorialEntriesQuery))]
    public void Territorial_use_cases_require_administrator_access(Type useCaseType)
    {
        var authorization = Assert.Single(useCaseType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());

        Assert.Equal(UserRoles.Administrator, authorization.Roles);
    }
}
