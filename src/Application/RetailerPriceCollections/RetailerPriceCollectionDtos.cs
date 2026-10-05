namespace Application.RetailerPriceCollections;

/// <summary>Represents one durable company price-collection run.</summary>
public sealed record RetailerPriceCollectionRunSummaryDto(
    Guid Id,
    Guid CompanyPersonId,
    Guid? OfferId,
    string CompanyDisplayName,
    Guid ExtractionProfileId,
    int ExtractionProfileVersion,
    string Status,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    int TotalCount,
    int QueuedCount,
    int RunningCount,
    int ProcessedCount,
    int SucceededCount,
    int FailedCount,
    int SkippedCount);

/// <summary>Represents one persisted listing outcome within a collection run.</summary>
public sealed record RetailerPriceCollectionRunItemDto(
    Guid Id,
    Guid RetailerListingId,
    Guid RetailerId,
    string RetailerDisplayName,
    Guid ProductId,
    int ProductNumberId,
    string ProductTitle,
    string? RetailerProductCode,
    string Status,
    string? CapturedProductUrl,
    string? FinalSourceUrl,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    decimal? Amount,
    string? CurrencyCode,
    string? PriceBasis,
    DateTimeOffset? ObservedAt,
    DateTimeOffset? RecordedAt,
    Guid? MatchedRuleId,
    string? MatchedRuleName,
    int? MatchedRulePriority,
    Guid? RetailerPriceObservationId,
    string? DiagnosticCode,
    string? DiagnosticMessage);

/// <summary>Represents one run and a page of its persisted listing outcomes.</summary>
public sealed record RetailerPriceCollectionRunDetailsDto(
    RetailerPriceCollectionRunSummaryDto Run,
    IReadOnlyList<RetailerPriceCollectionRunItemDto> Items,
    int PageIndex,
    int PageSize,
    int TotalCount);
