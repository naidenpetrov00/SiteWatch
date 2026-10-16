namespace Domain.SeedWork.Enums;

public enum AssetCatalogClassification
{
    Machine,
    Tool
}

public enum AssetReviewStatus
{
    PendingReview,
    Active,
    Rejected
}

public enum AssetCondition
{
    Good,
    Damaged,
    Unsafe
}

public enum AssetOperationalStatus
{
    Available,
    InUse,
    InTransfer,
    Missing,
    UnderRepair,
    Retired
}

public enum AssetEventType
{
    Registered,
    RegistrationActivated,
    RegistrationRejected,
    Sighted,
    Relocated,
    Returned,
    OperationalStatusChanged,
    ConditionChanged,
    MissingReported,
    Retired,
    HandoverInitiated,
    HandoverAccepted,
    HandoverRejected,
    AdministrativeReassignment,
    CatalogAssociationCorrected,
    ManufacturerSerialCorrected,
    QrIdentifierRotated
}
