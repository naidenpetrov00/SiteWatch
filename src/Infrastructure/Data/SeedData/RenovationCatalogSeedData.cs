using Domain.Entities;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class RenovationCatalogSeedData(
    ApplicationDbContext dbContext,
    ILogger<RenovationCatalogSeedData> logger)
{
    private const string SeededBy = "System";
    private const decimal HistoricalPriceFactor = 0.80m;
    private static readonly DateTimeOffset BaselineObservedAt =
        new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset BaselineRecordedAt =
        BaselineObservedAt.AddMinutes(5);

    public async Task SeedAsync()
    {
        var definitions = CreateDefinitions();
        var retailerNameKeys = definitions
            .Select(definition => Retailer.NormalizeNameKey(definition.RetailerName))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var retailers = await dbContext.Retailers
            .Where(retailer => retailerNameKeys.Contains(retailer.NormalizedName))
            .ToListAsync();
        var retailersByName = retailers.ToDictionary(
            retailer => retailer.NormalizedName,
            StringComparer.Ordinal);

        var createdProductCount = 0;
        var createdListingCount = 0;
        var createdObservationCount = 0;

        foreach (var definition in definitions)
        {
            var retailerNameKey = Retailer.NormalizeNameKey(definition.RetailerName);
            if (!retailersByName.TryGetValue(retailerNameKey, out var retailer))
            {
                logger.LogWarning(
                    "Renovation catalog record {RetailerProductCode} was skipped because retailer {RetailerName} is missing.",
                    definition.RetailerProductCode,
                    definition.RetailerName);
                continue;
            }

            var listing = await dbContext.RetailerListings
                .FirstOrDefaultAsync(candidate =>
                    candidate.RetailerId == retailer.Id
                    && candidate.RetailerProductCode == definition.RetailerProductCode);
            if (listing is null)
            {
                var product = dbContext.Products.Local.FirstOrDefault(candidate =>
                        candidate.Title == definition.Title
                        && candidate.Brand == definition.Brand
                        && candidate.Model == definition.Model)
                    ?? await dbContext.Products.FirstOrDefaultAsync(candidate =>
                        candidate.Title == definition.Title
                        && candidate.Brand == definition.Brand
                        && candidate.Model == definition.Model);
                if (product is null)
                {
                    product = Product.Create(
                        definition.Title,
                        definition.Description,
                        definition.Brand,
                        definition.Model,
                        definition.PackageQuantity,
                        definition.PackageUnit,
                        definition.Category,
                        ProductStatus.Active,
                        definition.SearchConfiguration);
                    ApplyAuditMetadata(product);
                    dbContext.Products.Add(product);
                    createdProductCount++;
                }

                listing = RetailerListing.Create(
                    product,
                    retailer,
                    definition.ProductUrl,
                    definition.RetailerProductCode);
                ApplyAuditMetadata(listing);
                dbContext.RetailerListings.Add(listing);
                createdListingCount++;
            }

            var hasBaselineObservation = await dbContext.RetailerPriceObservations.AnyAsync(
                observation =>
                    observation.RetailerListingId == listing.Id
                    && observation.Source == PriceObservationSource.Manual
                    && observation.ObservedAt == BaselineObservedAt
                    && observation.SourceReference == definition.ProductUrl);
            if (hasBaselineObservation)
            {
                continue;
            }
            if (!listing.IsActive)
            {
                logger.LogWarning(
                    "Historical price seeding for retailer product {RetailerProductCode} was skipped because its listing is inactive.",
                    definition.RetailerProductCode);
                continue;
            }

            var observation = listing.RecordPrice(
                definition.HistoricalPrice,
                PriceBasis.Package,
                BaselineObservedAt,
                BaselineRecordedAt,
                PriceObservationSource.Manual,
                definition.ProductUrl,
                SeededBy);
            dbContext.RetailerPriceObservations.Add(observation);
            createdObservationCount++;
        }

        if (createdProductCount > 0
            || createdListingCount > 0
            || createdObservationCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation(
            "Ensured {CatalogRecordCount} renovation catalog records; created {ProductCount} products, {ListingCount} listings and {ObservationCount} historical price observations.",
            definitions.Count,
            createdProductCount,
            createdListingCount,
            createdObservationCount);
    }

    private static List<CatalogDefinition> CreateDefinitions() =>
    [
        new(
            "Praktiker",
            "Laminate and Parquet Installation Kit",
            "Reusable fitting set for positioning and joining laminate and parquet flooring.",
            "KWB",
            null,
            1m,
            ProductPackageUnit.Set,
            ProductCategory.ToolsEquipment,
            ProductSearchConfiguration.Create(
                "KWB комплект за монтаж на паркет и ламинат",
                ["KWB laminate parquet installation kit"],
                ["KWB", "ламинат"],
                ["резервна част"]),
            "https://praktiker.bg/bg/Drugi-rachni-instrumenti/KOMPLEKT-ZA-MONTAZh-NA-PARKET-I-LAMINAT-KWB/p/488351",
            "488351",
            12.78m),
        new(
            "Praktiker",
            "EGGER Aqua Treviso Oak Laminate Flooring, 1.9948 m2",
            "Water-resistant 8 mm AC4 click laminate flooring supplied as a 1.9948 m2 pack.",
            "EGGER",
            "Aqua Treviso Oak EL2191",
            1.9948m,
            ProductPackageUnit.SquareMeter,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "EGGER Aqua дъб Тревисо EL2191 ламинат 8 мм 1.9948 m2",
                ["EGGER Aqua Treviso Oak EL2191 laminate pack"],
                ["EGGER", "EL2191", "1.9948"],
                ["мостра", "перваз"]),
            "https://praktiker.bg/bg/Parket/LAMINIRAN-PARKET-EGGER-AQUA-DAB-TREVISO/p/235123",
            "235123",
            29.90m),
        new(
            "Praktiker",
            "Polyethylene Laminate Underlay, 5 mm, 15 m2",
            "Five-millimetre polyethylene underlay supplied as one 15 m2 pack for laminate flooring.",
            null,
            "5 mm / 15 m2",
            15m,
            ProductPackageUnit.SquareMeter,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "полиетиленова подложка за ламинат 5 мм 15 m2 пакет",
                ["laminate underlay 5 mm 15 m2 pack"],
                ["5 мм", "15 m2", "полиетилен"],
                ["XPS", "5 m2"]),
            "https://praktiker.bg/bg/Podlozhki-za-laminat/PODLOZhKA-ZA-LAMINAT-5-MM--15-M2-V-PAKET/p/116849",
            "116849",
            17.25m),
        new(
            "Praktiker",
            "Polyester Paint Roller, 12 cm",
            "Reusable 12 cm polyester roller with 12 mm pile for emulsion and facade paint.",
            "COLOR EXPERT",
            null,
            1m,
            ProductPackageUnit.Piece,
            ProductCategory.ToolsEquipment,
            ProductSearchConfiguration.Create(
                "Color Expert валяк полиестер 12 см 12 мм Ф30 мм",
                ["Color Expert polyester paint roller 12 cm"],
                ["Color Expert", "12 см"],
                ["ролка без дръжка"]),
            "https://praktiker.bg/bg/Valyaci/VALYaK-POLIESTER-12-CM-12-MM-F30-MM-COLOR-EXPERT/p/225952",
            "225952",
            2.14m),
        new(
            "Maxxmart",
            "Interior Gypsum Filler, 15 kg",
            "Gypsum filler for smoothing interior walls and ceilings and filling fibreboard joints.",
            "GLOBUS",
            "G1 41",
            15m,
            ProductPackageUnit.Kilogram,
            ProductCategory.PaintsFinishes,
            ProductSearchConfiguration.Create(
                "GLOBUS G1 41 гипсова шпакловка 15 кг",
                ["GLOBUS G1 41 gypsum filler 15 kg"],
                ["GLOBUS", "G1 41", "15 кг"],
                ["G1 63", "5 кг"]),
            "https://www.maxxmart.eu/product/gipsova-shpaklovka-globus-g1-41",
            "0114020003",
            8.52m),
        new(
            "Praktiker",
            "Liquid Waterproofing Membrane, 5 kg",
            "Ready-to-apply elastic liquid membrane for waterproofing bathrooms before tiling.",
            "TKK",
            "HydroBlocker WRC Coating",
            5m,
            ProductPackageUnit.Kilogram,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "TKK HydroBlocker WRC Coating хидроизолация 5 kg",
                ["TKK HydroBlocker WRC 5 kg waterproofing"],
                ["TKK", "HydroBlocker", "5 kg"],
                ["лента", "грунд"]),
            "https://praktiker.bg/bg/Hidroizolaciya-za-bani-i-terasi/TEChNA-HIDROIZOLATzIYa-TKK-HYDROBLOCKER-WRC/p/231601",
            "231601",
            20.45m),
        new(
            "Praktiker",
            "Ceresit CE 40 Jasmine Grout, 5 kg",
            "Flexible water-repellent grout for 1-8 mm joints in interior and exterior tile coverings.",
            "Ceresit",
            "CE 40 Jasmine 40",
            5m,
            ProductPackageUnit.Kilogram,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "Ceresit CE 40 жасмин 40 фугираща смес 5 кг",
                ["Ceresit CE 40 Jasmine grout 5 kg"],
                ["Ceresit", "CE 40", "жасмин", "5 кг"],
                ["CE 43", "2 кг"]),
            "https://praktiker.bg/bg/Fugi/FUGIRAShtA-SMES-CERESIT-CE-40--ZhASMIN-40-5KG/p/488472",
            "488472",
            17.63m),
        new(
            "Praktiker",
            "MODULATO MDF Interior Door Set, 80 x 200 x 7 cm, Left",
            "Left-opening interior MDF door supplied with a 7 cm frame and hinges, ready for painting.",
            "MODULATO",
            "80 x 200 x 7 cm Left",
            1m,
            ProductPackageUnit.Set,
            ProductCategory.FixturesAppliances,
            ProductSearchConfiguration.Create(
                "MODULATO MDF врата с каса 80x200x7 см лява",
                ["MODULATO MDF interior door set 80 x 200 x 7 cm left"],
                ["MODULATO", "80x200x7", "лява"],
                ["дясна", "16 см"]),
            "https://praktiker.bg/bg/Vrati/MDF-VRATA-S-KASA-MODULATO-80H200H7-SM--LYaVA/p/460549",
            "460549",
            89.99m),
        new(
            "Maxxmart",
            "Deep-Penetrating Primer, 5 L",
            "Concentrated primer for stabilizing absorbent walls and floors before paint, tiles or parquet.",
            "GLOBUS",
            "G4 25 GRUND",
            5m,
            ProductPackageUnit.Liter,
            ProductCategory.PaintsFinishes,
            ProductSearchConfiguration.Create(
                "GLOBUS G4 25 GRUND дълбокопроникващ грунд 5 л",
                ["GLOBUS G4 25 primer 5 L"],
                ["GLOBUS", "G4 25", "5 л"],
                ["1 л", "контактен грунд"]),
            "https://www.maxxmart.eu/product/dlbokopronikvashch-grund-globus-g4-25-grund-5-l",
            "0102010020",
            10.68m),
        new(
            "Maxxmart",
            "LEKO Interin White Interior Paint, 15 L",
            "Matt white water-based paint for interior walls and ceilings, supplied in a 15 L package.",
            "Леко",
            "Интерин бял",
            15m,
            ProductPackageUnit.Liter,
            ProductCategory.PaintsFinishes,
            ProductSearchConfiguration.Create(
                "Леко Интерин бял латекс 15 л",
                ["LEKO Interin white interior paint 15 L"],
                ["Леко", "Интерин", "15 л"],
                ["5 л", "оцветен"]),
            "https://www.maxxmart.eu/product/lateks-interin-byal-15-l-leko",
            "6050700598",
            57.17m),
        new(
            "Maxxmart",
            "Gun-Grade Polyurethane Foam, 750 ml",
            "Gun-applied polyurethane mounting foam for filling and insulating door and window joints.",
            "GLOBUS",
            "G5 53",
            750m,
            ProductPackageUnit.Milliliter,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "GLOBUS G5 53 пистолетна полиуретанова пяна 750 мл",
                ["GLOBUS G5 53 gun foam 750 ml"],
                ["GLOBUS", "G5 53", "750 мл"],
                ["ръчна пяна", "чистител"]),
            "https://www.maxxmart.eu/product/pistoletna-poliuretanova-pyana-globus-g5-53",
            "0502010007",
            5.20m),
        new(
            "Maxxmart",
            "Flexible Tile Adhesive, 25 kg",
            "C2 TE flexible cement adhesive for interior and exterior ceramic, stone and porcelain tiles.",
            "GLOBUS",
            "G1 56 FLEX",
            25m,
            ProductPackageUnit.Kilogram,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "GLOBUS G1 56 FLEX гъвкаво лепило за фаянс и керамика 25 кг",
                ["GLOBUS FLEX tile adhesive 25 kg"],
                ["GLOBUS", "FLEX", "25 кг"],
                ["фугираща смес", "5 кг"]),
            "https://www.maxxmart.eu/product/gvkavo-lepilo-za-fayans-i-keramika-globus-g1-56-flex",
            "1131080",
            10.18m),
        new(
            "Praktiker",
            "Ceresit CM 11 Plus Tile Adhesive, 25 kg",
            "C1 T cement adhesive for ceramic and porcelain tiles on stable interior and exterior mineral bases.",
            "Ceresit",
            "CM 11 Plus Gres",
            25m,
            ProductPackageUnit.Kilogram,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "Ceresit CM 11 Plus Gres лепило за плочки 25 кг",
                ["Ceresit CM 11 Plus tile adhesive 25 kg"],
                ["Ceresit", "CM 11", "25 кг"],
                ["5 кг", "CM 12"]),
            "https://praktiker.bg/bg/Lepila-za-plochki/LEPILO-ZA-PLOChKI-CERESIT-CM-11-SIV/p/432837",
            "432837",
            6.99m),
        new(
            "Maxxmart",
            "Ceresit CM 11 Plus Tile Adhesive, 25 kg",
            "C1 T cement adhesive for ceramic and porcelain tiles on stable interior and exterior mineral bases.",
            "Ceresit",
            "CM 11 Plus Gres",
            25m,
            ProductPackageUnit.Kilogram,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "Ceresit CM 11 Plus Gres лепило за плочки 25 кг",
                ["Ceresit CM 11 Plus tile adhesive 25 kg"],
                ["Ceresit", "CM 11", "25 кг"],
                ["5 кг", "CM 12"]),
            "https://www.maxxmart.eu/product/lepilo-za-granitogres-i-keramichni-plochki-ceresit-cm-11-plus-gres-25",
            "0110020017",
            6.69m),
        new(
            "Praktiker",
            "Ceresit CS 25 White Sanitary Silicone, 280 ml",
            "White mould-resistant sanitary silicone for elastic sealing around tiles, sanitary fittings, doors and windows.",
            "Ceresit",
            "CS 25 White",
            280m,
            ProductPackageUnit.Milliliter,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "Ceresit CS 25 бял санитарен силикон 280 мл",
                ["Ceresit CS 25 white sanitary silicone 280 ml"],
                ["Ceresit", "CS 25", "бял", "280 мл"],
                ["300 мл", "прозрачен"]),
            "https://praktiker.bg/bg/Silikoni-i-akrili/SANITAREN-SILIKON-BYaL-CERESIT-CS-25/p/433906",
            "433906",
            3.99m),
        new(
            "Maxxmart",
            "Ceresit CS 25 White Sanitary Silicone, 280 ml",
            "White mould-resistant sanitary silicone for elastic sealing around tiles, sanitary fittings, doors and windows.",
            "Ceresit",
            "CS 25 White",
            280m,
            ProductPackageUnit.Milliliter,
            ProductCategory.BuildingConstruction,
            ProductSearchConfiguration.Create(
                "Ceresit CS 25 бял санитарен силикон 280 мл",
                ["Ceresit CS 25 white sanitary silicone 280 ml"],
                ["Ceresit", "CS 25", "бял", "280 мл"],
                ["300 мл", "прозрачен"]),
            "https://www.maxxmart.eu/product/sanitaren-silikon-cs-25-ceresit-byal",
            "0100190004",
            4.80m),
    ];

    private static void ApplyAuditMetadata(BaseAuditableEntity entity)
    {
        var now = DateTimeOffset.UtcNow;
        entity.Created = now;
        entity.CreatedBy = SeededBy;
        entity.LastModified = now;
        entity.LastModifiedBy = SeededBy;
    }

    private sealed record CatalogDefinition(
        string RetailerName,
        string Title,
        string Description,
        string? Brand,
        string? Model,
        decimal PackageQuantity,
        ProductPackageUnit PackageUnit,
        ProductCategory Category,
        ProductSearchConfiguration SearchConfiguration,
        string ProductUrl,
        string RetailerProductCode,
        decimal ReferencePriceAtResearch)
    {
        // The research reference is never persisted as an observation. Only the
        // deliberately older demo baseline calculated from it enters price history.
        public decimal HistoricalPrice => decimal.Round(
            ReferencePriceAtResearch * HistoricalPriceFactor,
            2,
            MidpointRounding.AwayFromZero);
    }
}
