namespace Domain.SeedWork.Enums;

public enum ProductCategory
{
    BuildingConstruction,
    ToolsEquipment,
    ElectricalLighting,
    PlumbingHvac,
    HardwareFasteners,
    PaintsFinishes,
    SafetySecurity,
    CleaningMaintenance,
    FixturesAppliances,
    OutdoorLandscaping,
    OfficeGeneralSupplies,
    Other
}

public static class ProductCategoryCodes
{
    public static string ToCode(this ProductCategory category) =>
        category switch
        {
            ProductCategory.BuildingConstruction => "building-construction",
            ProductCategory.ToolsEquipment => "tools-equipment",
            ProductCategory.ElectricalLighting => "electrical-lighting",
            ProductCategory.PlumbingHvac => "plumbing-hvac",
            ProductCategory.HardwareFasteners => "hardware-fasteners",
            ProductCategory.PaintsFinishes => "paints-finishes",
            ProductCategory.SafetySecurity => "safety-security",
            ProductCategory.CleaningMaintenance => "cleaning-maintenance",
            ProductCategory.FixturesAppliances => "fixtures-appliances",
            ProductCategory.OutdoorLandscaping => "outdoor-landscaping",
            ProductCategory.OfficeGeneralSupplies => "office-general-supplies",
            ProductCategory.Other => "other",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    public static string ToSearchText(this ProductCategory category) =>
        category switch
        {
            ProductCategory.BuildingConstruction => "Building Construction",
            ProductCategory.ToolsEquipment => "Tools Equipment",
            ProductCategory.ElectricalLighting => "Electrical Lighting",
            ProductCategory.PlumbingHvac => "Plumbing HVAC",
            ProductCategory.HardwareFasteners => "Hardware Fasteners",
            ProductCategory.PaintsFinishes => "Paints Finishes",
            ProductCategory.SafetySecurity => "Safety Security",
            ProductCategory.CleaningMaintenance => "Cleaning Maintenance",
            ProductCategory.FixturesAppliances => "Fixtures Appliances",
            ProductCategory.OutdoorLandscaping => "Outdoor Landscaping",
            ProductCategory.OfficeGeneralSupplies => "Office General Supplies",
            ProductCategory.Other => "Other",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    public static bool TryParse(string? value, out ProductCategory category)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();
        category = normalizedValue switch
        {
            "building-construction" => ProductCategory.BuildingConstruction,
            "tools-equipment" => ProductCategory.ToolsEquipment,
            "electrical-lighting" => ProductCategory.ElectricalLighting,
            "plumbing-hvac" => ProductCategory.PlumbingHvac,
            "hardware-fasteners" => ProductCategory.HardwareFasteners,
            "paints-finishes" => ProductCategory.PaintsFinishes,
            "safety-security" => ProductCategory.SafetySecurity,
            "cleaning-maintenance" => ProductCategory.CleaningMaintenance,
            "fixtures-appliances" => ProductCategory.FixturesAppliances,
            "outdoor-landscaping" => ProductCategory.OutdoorLandscaping,
            "office-general-supplies" => ProductCategory.OfficeGeneralSupplies,
            "other" => ProductCategory.Other,
            _ => default
        };

        return normalizedValue is
            "building-construction" or "tools-equipment" or "electrical-lighting"
            or "plumbing-hvac" or "hardware-fasteners" or "paints-finishes"
            or "safety-security" or "cleaning-maintenance" or "fixtures-appliances"
            or "outdoor-landscaping" or "office-general-supplies" or "other";
    }
}
