namespace Application.ProductCatalog;

/// <summary>Represents an administrator-managed Category.</summary>
public sealed record CategoryDto(Guid Id, string Name);

/// <summary>Represents an administrator-managed Product Type.</summary>
public sealed record ProductTypeDto(
    Guid Id,
    string Name,
    Guid CategoryId,
    string CategoryName);
