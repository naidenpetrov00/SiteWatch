using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

internal sealed record AssetStateSnapshot(
    AssetClassification Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string? Description,
    Guid QrIdentifier,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string? ResponsibleWorkerId,
    string? PendingHandoverRecipientId,
    AssetCondition Condition,
    AssetOperationalStatus OperationalStatus,
    AssetOperationalStatus? PreHandoverOperationalStatus,
    AssetReviewStatus ReviewStatus);

public sealed class AssetEvent : BaseEntity
{
    public const int MaxNoteLength = 2000;

    private AssetEvent()
    {
    }

    public Guid AssetId { get; private set; }
    public Asset Asset { get; private set; } = null!;
    public long SequenceNumber { get; private set; }
    public AssetEventType EventType { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public ApplicationUser Actor { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Note { get; private set; }

    public AssetClassification? PreviousClassification { get; private set; }
    public string? PreviousName { get; private set; }
    public string? PreviousManufacturer { get; private set; }
    public string? PreviousModel { get; private set; }
    public string? PreviousDescription { get; private set; }
    public Guid? PreviousQrIdentifier { get; private set; }
    public string? PreviousManufacturerSerialNumber { get; private set; }
    public Guid? PreviousSiteId { get; private set; }
    public string? PreviousResponsibleWorkerId { get; private set; }
    public string? PreviousPendingHandoverRecipientId { get; private set; }
    public AssetCondition? PreviousCondition { get; private set; }
    public AssetOperationalStatus? PreviousOperationalStatus { get; private set; }
    public AssetOperationalStatus? PreviousPreHandoverOperationalStatus { get; private set; }
    public AssetReviewStatus? PreviousReviewStatus { get; private set; }

    public AssetClassification ResultingClassification { get; private set; }
    public string ResultingName { get; private set; } = string.Empty;
    public string? ResultingManufacturer { get; private set; }
    public string? ResultingModel { get; private set; }
    public string? ResultingDescription { get; private set; }
    public Guid ResultingQrIdentifier { get; private set; }
    public string? ResultingManufacturerSerialNumber { get; private set; }
    public Guid ResultingSiteId { get; private set; }
    public string? ResultingResponsibleWorkerId { get; private set; }
    public string? ResultingPendingHandoverRecipientId { get; private set; }
    public AssetCondition ResultingCondition { get; private set; }
    public AssetOperationalStatus ResultingOperationalStatus { get; private set; }
    public AssetOperationalStatus? ResultingPreHandoverOperationalStatus { get; private set; }
    public AssetReviewStatus ResultingReviewStatus { get; private set; }

    internal static AssetEvent Create(
        Asset asset,
        long sequenceNumber,
        AssetEventType eventType,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        AssetStateSnapshot? previousState,
        AssetStateSnapshot resultingState,
        string? note)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor.Id);
        ArgumentNullException.ThrowIfNull(resultingState);
        if (sequenceNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber));
        }
        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType));
        }
        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The event timestamp must be UTC.", nameof(occurredAt));
        }
        if ((eventType == AssetEventType.Registered) != (previousState is null))
        {
            throw new ArgumentException(
                "Only a registration event can omit its previous state.",
                nameof(previousState));
        }

        var assetEvent = new AssetEvent
        {
            Id = Guid.NewGuid(),
            Asset = asset,
            AssetId = asset.Id,
            SequenceNumber = sequenceNumber,
            EventType = eventType,
            Actor = actor,
            ActorId = actor.Id,
            OccurredAt = occurredAt,
            Note = NormalizeNote(note)
        };

        if (previousState is not null)
        {
            assetEvent.SetPreviousState(previousState);
        }
        assetEvent.SetResultingState(resultingState);
        return assetEvent;
    }

    private void SetPreviousState(AssetStateSnapshot state)
    {
        PreviousClassification = state.Classification;
        PreviousName = state.Name;
        PreviousManufacturer = state.Manufacturer;
        PreviousModel = state.Model;
        PreviousDescription = state.Description;
        PreviousQrIdentifier = state.QrIdentifier;
        PreviousManufacturerSerialNumber = state.ManufacturerSerialNumber;
        PreviousSiteId = state.SiteId;
        PreviousResponsibleWorkerId = state.ResponsibleWorkerId;
        PreviousPendingHandoverRecipientId = state.PendingHandoverRecipientId;
        PreviousCondition = state.Condition;
        PreviousOperationalStatus = state.OperationalStatus;
        PreviousPreHandoverOperationalStatus = state.PreHandoverOperationalStatus;
        PreviousReviewStatus = state.ReviewStatus;
    }

    private void SetResultingState(AssetStateSnapshot state)
    {
        ResultingClassification = state.Classification;
        ResultingName = state.Name;
        ResultingManufacturer = state.Manufacturer;
        ResultingModel = state.Model;
        ResultingDescription = state.Description;
        ResultingQrIdentifier = state.QrIdentifier;
        ResultingManufacturerSerialNumber = state.ManufacturerSerialNumber;
        ResultingSiteId = state.SiteId;
        ResultingResponsibleWorkerId = state.ResponsibleWorkerId;
        ResultingPendingHandoverRecipientId = state.PendingHandoverRecipientId;
        ResultingCondition = state.Condition;
        ResultingOperationalStatus = state.OperationalStatus;
        ResultingPreHandoverOperationalStatus = state.PreHandoverOperationalStatus;
        ResultingReviewStatus = state.ReviewStatus;
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        var normalized = note.Trim();
        if (normalized.Length > MaxNoteLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(note),
                normalized.Length,
                $"An asset event note cannot exceed {MaxNoteLength} characters.");
        }

        return normalized;
    }
}
