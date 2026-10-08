using Domain.Entities;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class ActivityCatalogSeedData(
    ApplicationDbContext dbContext,
    ILogger<ActivityCatalogSeedData> logger)
{
    private const string SeededBy = "System";

    public async Task SeedAsync()
    {
        var definitions = CreateDefinitions();
        var productsByTitle = await LoadProductsAsync(definitions);
        var nodes = await dbContext.ActivityCatalogNodes.ToListAsync();
        var sections = await dbContext.ActivityRequirementSections
            .Include(section => section.ProductRequirements)
            .ToListAsync();
        var counters = new SeedCounters();
        var seededAt = DateTimeOffset.UtcNow;

        foreach (var areaDefinition in definitions)
        {
            var areaFolder = EnsureFolder(
                nodes,
                areaDefinition.Name,
                parentFolderId: null,
                areaDefinition.SortOrder,
                seededAt,
                counters);
            if (areaFolder is null)
            {
                continue;
            }

            var subfolder = EnsureFolder(
                nodes,
                areaDefinition.SubfolderName,
                areaFolder.Id,
                sortOrder: 0,
                seededAt,
                counters);
            if (subfolder is null)
            {
                continue;
            }

            foreach (var activityDefinition in areaDefinition.Activities)
            {
                var activity = EnsureActivity(
                    nodes,
                    activityDefinition,
                    subfolder.Id,
                    seededAt,
                    counters);
                if (activity is null)
                {
                    continue;
                }

                EnsureActivityStructure(
                    activity,
                    activityDefinition,
                    sections,
                    productsByTitle,
                    seededAt,
                    counters);
            }
        }

        if (counters.TotalCreated > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation(
            "Ensured the renovation activity catalog; created {FolderCount} folders, {ActivityCount} activities, {SectionCount} work steps and {RequirementCount} product requirements.",
            counters.Folders,
            counters.Activities,
            counters.Sections,
            counters.Requirements);
    }

    private async Task<Dictionary<string, Product>> LoadProductsAsync(
        IReadOnlyCollection<AreaDefinition> definitions)
    {
        var requiredTitles = definitions
            .SelectMany(area => area.Activities)
            .SelectMany(activity => activity.Sections)
            .SelectMany(section => section.Requirements)
            .Select(requirement => requirement.ProductTitle)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var products = await dbContext.Products
            .Where(product => requiredTitles.Contains(product.Title))
            .ToListAsync();
        var productsByTitle = new Dictionary<string, Product>(StringComparer.Ordinal);

        foreach (var title in requiredTitles)
        {
            var activeMatches = products
                .Where(product => product.Title == title && product.Status == ProductStatus.Active)
                .ToList();
            if (activeMatches.Count == 1)
            {
                productsByTitle.Add(title, activeMatches[0]);
                continue;
            }

            logger.LogWarning(
                activeMatches.Count == 0
                    ? "Activity catalog product requirement {ProductTitle} was skipped because the active seeded product is missing."
                    : "Activity catalog product requirement {ProductTitle} was skipped because multiple active products have that title.",
                title);
        }

        return productsByTitle;
    }

    private ActivityFolder? EnsureFolder(
        List<ActivityCatalogNode> nodes,
        string name,
        Guid? parentFolderId,
        int sortOrder,
        DateTimeOffset seededAt,
        SeedCounters counters)
    {
        var normalizedName = ActivityCatalogNode.NormalizeNameKey(name);
        var existingNode = nodes.SingleOrDefault(node =>
            node.ParentFolderId == parentFolderId
            && node.NormalizedName == normalizedName);
        if (existingNode is ActivityFolder existingFolder)
        {
            return existingFolder;
        }

        if (existingNode is not null)
        {
            logger.LogWarning(
                "Activity catalog folder {FolderName} was skipped because an activity with the same name already exists at that location.",
                name);
            return null;
        }

        var folder = ActivityFolder.Create(name, parentFolderId, sortOrder);
        ApplyAuditMetadata(folder, seededAt);
        dbContext.ActivityCatalogNodes.Add(folder);
        nodes.Add(folder);
        counters.Folders++;
        return folder;
    }

    private Activity? EnsureActivity(
        List<ActivityCatalogNode> nodes,
        ActivityDefinition definition,
        Guid parentFolderId,
        DateTimeOffset seededAt,
        SeedCounters counters)
    {
        var normalizedName = ActivityCatalogNode.NormalizeNameKey(definition.Name);
        var existingNode = nodes.SingleOrDefault(node =>
            node.ParentFolderId == parentFolderId
            && node.NormalizedName == normalizedName);
        if (existingNode is Activity existingActivity)
        {
            return existingActivity;
        }

        if (existingNode is not null)
        {
            logger.LogWarning(
                "Renovation activity {ActivityName} was skipped because a folder with the same name already exists at that location.",
                definition.Name);
            return null;
        }

        var activity = Activity.Create(
            definition.Name,
            definition.Description,
            parentFolderId,
            definition.SortOrder);
        ApplyAuditMetadata(activity, seededAt);
        dbContext.ActivityCatalogNodes.Add(activity);
        nodes.Add(activity);
        counters.Activities++;
        return activity;
    }

    private void EnsureActivityStructure(
        Activity activity,
        ActivityDefinition definition,
        List<ActivityRequirementSection> sections,
        IReadOnlyDictionary<string, Product> productsByTitle,
        DateTimeOffset seededAt,
        SeedCounters counters)
    {
        if (activity.Status == ActivityStatus.Archived)
        {
            logger.LogWarning(
                "Work steps for renovation activity {ActivityName} were not changed because the activity is archived.",
                activity.Name);
            return;
        }

        foreach (var sectionDefinition in definition.Sections)
        {
            var matchingSections = sections
                .Where(section =>
                    section.ActivityId == activity.Id
                    && NormalizeOptionalNameKey(section.Name)
                        == NormalizeOptionalNameKey(sectionDefinition.Name))
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id)
                .ToList();
            var section = matchingSections.FirstOrDefault();
            if (section is null)
            {
                section = activity.AddRequirementSection(
                    sectionDefinition.Name,
                    sectionDefinition.BasisQuantity,
                    sectionDefinition.MeasurementUnit,
                    sectionDefinition.SortOrder);
                ApplyAuditMetadata(section, seededAt);
                dbContext.ActivityRequirementSections.Add(section);
                sections.Add(section);
                counters.Sections++;
            }
            else if (section.BasisQuantity != sectionDefinition.BasisQuantity
                     || section.MeasurementUnit != sectionDefinition.MeasurementUnit)
            {
                logger.LogWarning(
                    "Existing work step {SectionName} for renovation activity {ActivityName} has a different measurement configuration; the existing step and its requirements were preserved.",
                    sectionDefinition.Name,
                    activity.Name);
                continue;
            }

            if (matchingSections.Count > 1)
            {
                logger.LogWarning(
                    "Renovation activity {ActivityName} has duplicate work steps named {SectionName}; their product requirements were preserved.",
                    activity.Name,
                    sectionDefinition.Name);
                continue;
            }

            foreach (var requirementDefinition in sectionDefinition.Requirements)
            {
                if (!productsByTitle.TryGetValue(
                        requirementDefinition.ProductTitle,
                        out var product)
                    || section.ProductRequirements.Any(requirement =>
                        requirement.ProductId == product.Id))
                {
                    continue;
                }

                var requirement = section.AddProductRequirement(
                    product,
                    requirementDefinition.Quantity,
                    requirementDefinition.IsRequired,
                    requirementDefinition.QuantityBehavior,
                    requirementDefinition.Notes,
                    requirementDefinition.SortOrder);
                ApplyAuditMetadata(requirement, seededAt);
                dbContext.ActivityProductRequirements.Add(requirement);
                counters.Requirements++;
            }
        }
    }

    private static string NormalizeOptionalNameKey(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : ActivityCatalogNode.NormalizeNameKey(name);

    private static void ApplyAuditMetadata(BaseAuditableEntity entity, DateTimeOffset seededAt)
    {
        entity.Created = seededAt;
        entity.CreatedBy = SeededBy;
        entity.LastModified = seededAt;
        entity.LastModifiedBy = SeededBy;
    }

    private static List<AreaDefinition> CreateDefinitions() =>
    [
        new(
            "Floors",
            0,
            "Laminate Flooring",
            [
                new(
                    "Install click laminate flooring",
                    "Install underlay and click laminate for a measured floor area, including a simple material waste allowance.",
                    0,
                    [
                        new(
                            "1. Install underlay",
                            15m,
                            ActivityMeasurementUnit.SquareMeter,
                            0,
                            [
                                new(
                                    "Polyethylene Laminate Underlay, 5 mm, 15 m2",
                                    1m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "One 15 m2 pack covers 15 m2 of measured floor area.",
                                    0)
                            ]),
                        new(
                            "2. Lay click laminate",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            1,
                            [
                                new(
                                    "EGGER Aqua Treviso Oak Laminate Flooring, 1.9948 m2",
                                    0.5514m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows 1.10 m2 of flooring per measured m2, including 10 percent cutting waste.",
                                    0),
                                new(
                                    "Laminate and Parquet Installation Kit",
                                    1m,
                                    false,
                                    ProductQuantityBehavior.Fixed,
                                    "Optional reusable fitting kit; one set is enough for the activity.",
                                    1)
                            ])
                    ])
            ]),
        new(
            "Walls",
            1,
            "Surface Preparation and Painting",
            [
                new(
                    "Prepare and paint interior walls",
                    "Fill, prime and apply two coats of interior paint to a measured wall area.",
                    0,
                    [
                        new(
                            "1. Fill and smooth wall surfaces",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            0,
                            [
                                new(
                                    "Interior Gypsum Filler, 15 kg",
                                    0.0667m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 1 kg of filler per measured m2 for a light smoothing coat.",
                                    0)
                            ]),
                        new(
                            "2. Prime prepared walls",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            1,
                            [
                                new(
                                    "Deep-Penetrating Primer, 5 L",
                                    0.024m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 0.12 L of primer per measured m2.",
                                    0)
                            ]),
                        new(
                            "3. Apply two paint coats",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            2,
                            [
                                new(
                                    "LEKO Interin White Interior Paint, 15 L",
                                    0.014m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 0.21 L of paint per measured m2 for two coats and minor waste.",
                                    0),
                                new(
                                    "Polyester Paint Roller, 12 cm",
                                    1m,
                                    false,
                                    ProductQuantityBehavior.Fixed,
                                    "Optional reusable roller; one piece is enough for the activity.",
                                    1)
                            ])
                    ])
            ]),
        new(
            "Windows",
            2,
            "Perimeter Sealing",
            [
                new(
                    "Seal interior window perimeter joints",
                    "Fill and finish measured window perimeter joints from the interior side.",
                    0,
                    [
                        new(
                            "1. Fill installation gaps",
                            20m,
                            ActivityMeasurementUnit.Meter,
                            0,
                            [
                                new(
                                    "Gun-Grade Polyurethane Foam, 750 ml",
                                    1m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "One can is allowed for 20 linear metres of typical perimeter gap.",
                                    0)
                            ]),
                        new(
                            "2. Finish perimeter seals",
                            12m,
                            ActivityMeasurementUnit.Meter,
                            1,
                            [
                                new(
                                    "Ceresit CS 25 White Sanitary Silicone, 280 ml",
                                    1m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "One cartridge is allowed for 12 linear metres of a small finishing bead.",
                                    0)
                            ])
                    ])
            ]),
        new(
            "Bathroom",
            3,
            "Waterproofing and Tiling",
            [
                new(
                    "Waterproof and tile bathroom wet areas",
                    "Prime, waterproof, tile and finish measured bathroom wet areas, with joint sealing measured separately.",
                    0,
                    [
                        new(
                            "1. Prime wet-area substrates",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            0,
                            [
                                new(
                                    "Deep-Penetrating Primer, 5 L",
                                    0.024m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 0.12 L of primer per measured m2.",
                                    0)
                            ]),
                        new(
                            "2. Apply waterproofing membrane",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            1,
                            [
                                new(
                                    "Liquid Waterproofing Membrane, 5 kg",
                                    0.3m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 1.5 kg of membrane per measured m2 for the complete coating.",
                                    0)
                            ]),
                        new(
                            "3. Fix wall and floor tiles",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            2,
                            [
                                new(
                                    "Flexible Tile Adhesive, 25 kg",
                                    0.16m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 4 kg of adhesive per measured m2 of tiled surface.",
                                    0),
                                new(
                                    "IZIDA Romani Beige Porcelain Tile, 30.3 x 60.6 cm, 1.84 m2",
                                    0.5978m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows 1.10 m2 of tile per measured m2, including 10 percent cutting waste.",
                                    1)
                            ]),
                        new(
                            "4. Grout tile joints",
                            1m,
                            ActivityMeasurementUnit.SquareMeter,
                            3,
                            [
                                new(
                                    "Ceresit CE 40 Jasmine Grout, 5 kg",
                                    0.08m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows about 0.4 kg of grout per measured m2 of tiled surface.",
                                    0)
                            ]),
                        new(
                            "5. Seal perimeter and sanitary joints",
                            12m,
                            ActivityMeasurementUnit.Meter,
                            4,
                            [
                                new(
                                    "Ceresit CS 25 White Sanitary Silicone, 280 ml",
                                    1m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "One cartridge is allowed for 12 linear metres of sanitary joint.",
                                    0)
                            ])
                    ])
            ]),
        new(
            "Doors",
            4,
            "Interior Doors",
            [
                new(
                    "Install an interior door set",
                    "Fit an interior door set and fill the frame joint for each measured doorway.",
                    0,
                    [
                        new(
                            "1. Fit door leaf and frame set",
                            1m,
                            ActivityMeasurementUnit.Piece,
                            0,
                            [
                                new(
                                    "MODULATO MDF Interior Door Set, 80 x 200 x 7 cm, Left",
                                    1m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "One packaged door set is required per measured doorway.",
                                    0)
                            ]),
                        new(
                            "2. Fill frame installation joint",
                            1m,
                            ActivityMeasurementUnit.Piece,
                            1,
                            [
                                new(
                                    "Gun-Grade Polyurethane Foam, 750 ml",
                                    0.5m,
                                    true,
                                    ProductQuantityBehavior.Proportional,
                                    "Allows half a 750 ml can per measured doorway.",
                                    0)
                            ])
                    ])
            ])
    ];

    private sealed record AreaDefinition(
        string Name,
        int SortOrder,
        string SubfolderName,
        IReadOnlyList<ActivityDefinition> Activities);

    private sealed record ActivityDefinition(
        string Name,
        string Description,
        int SortOrder,
        IReadOnlyList<SectionDefinition> Sections);

    private sealed record SectionDefinition(
        string Name,
        decimal BasisQuantity,
        ActivityMeasurementUnit MeasurementUnit,
        int SortOrder,
        IReadOnlyList<RequirementDefinition> Requirements);

    private sealed record RequirementDefinition(
        string ProductTitle,
        decimal Quantity,
        bool IsRequired,
        ProductQuantityBehavior QuantityBehavior,
        string Notes,
        int SortOrder);

    private sealed class SeedCounters
    {
        public int Folders { get; set; }
        public int Activities { get; set; }
        public int Sections { get; set; }
        public int Requirements { get; set; }
        public int TotalCreated => Folders + Activities + Sections + Requirements;
    }
}
