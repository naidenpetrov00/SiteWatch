using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Asset : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    public const int MaxManufacturerSerialNumberLength = 100;

    private readonly List<AssetEvent> _assetEvents = [];

    private Asset()
    {
    }

    public int NumberId { get; private set; }

    public string? AssetCode =>
        NumberId > 0 ? $"AST-{NumberId:D6}" : null;

    public Guid QrIdentifier { get; private set; }
    public Guid CatalogItemId { get; private set; }
    public AssetCatalogItem CatalogItem { get; private set; } = null!;
    public string? ManufacturerSerialNumber { get; private set; }
    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public string? ResponsibleWorkerId { get; private set; }
    public ApplicationUser? ResponsibleWorker { get; private set; }
    public AssetCondition Condition { get; private set; }
    public AssetOperationalStatus OperationalStatus { get; private set; }
    public AssetReviewStatus ReviewStatus { get; private set; }
    public string? PendingHandoverRecipientId { get; private set; }
    public ApplicationUser? PendingHandoverRecipient { get; private set; }
    public AssetOperationalStatus? PreHandoverOperationalStatus { get; private set; }
    public long HistorySequence { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<AssetEvent> AssetEvents => _assetEvents;

    public static Asset RegisterActive(
        AssetCatalogItem catalogItem,
        Site site,
        ApplicationUser? responsibleWorker,
        string? manufacturerSerialNumber,
        AssetCondition condition,
        AssetOperationalStatus operationalStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ArgumentNullException.ThrowIfNull(catalogItem);
        if (catalogItem.ReviewStatus != AssetReviewStatus.Active)
        {
            throw new ArgumentException(
                "An active asset requires an active catalog item.",
                nameof(catalogItem));
        }

        return Register(
            catalogItem,
            site,
            responsibleWorker,
            manufacturerSerialNumber,
            condition,
            operationalStatus,
            AssetReviewStatus.Active,
            actor,
            occurredAt,
            note);
    }

    public static Asset RegisterPendingReview(
        AssetCatalogItem catalogItem,
        Site site,
        ApplicationUser? responsibleWorker,
        string? manufacturerSerialNumber,
        AssetCondition condition,
        AssetOperationalStatus operationalStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ArgumentNullException.ThrowIfNull(catalogItem);
        if (catalogItem.ReviewStatus == AssetReviewStatus.Rejected)
        {
            throw new ArgumentException(
                "A rejected catalog item cannot receive an asset.",
                nameof(catalogItem));
        }

        return Register(
            catalogItem,
            site,
            responsibleWorker,
            manufacturerSerialNumber,
            condition,
            operationalStatus,
            AssetReviewStatus.PendingReview,
            actor,
            occurredAt,
            note);
    }

    public void ActivateRegistration(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsurePendingReview();
        if (CatalogItem.ReviewStatus != AssetReviewStatus.Active)
        {
            throw new InvalidOperationException(
                "The catalog item must be active before the asset can be activated.");
        }

        var previousState = CaptureState();
        ReviewStatus = AssetReviewStatus.Active;
        AppendEvent(AssetEventType.RegistrationActivated, actor, occurredAt, previousState, note);
    }

    public void RejectRegistration(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? reason = null)
    {
        ValidateEventContext(actor, occurredAt, reason);
        EnsurePendingReview();
        var previousState = CaptureState();
        ReviewStatus = AssetReviewStatus.Rejected;
        AppendEvent(AssetEventType.RegistrationRejected, actor, occurredAt, previousState, reason);
    }

    public void RecordSighting(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        var currentState = CaptureState();
        AppendEvent(AssetEventType.Sighted, actor, occurredAt, currentState, note);
    }

    public void Relocate(
        Site site,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        ArgumentNullException.ThrowIfNull(site);
        if (site.Id == SiteId)
        {
            throw new InvalidOperationException("The asset is already at the requested site.");
        }

        var previousState = CaptureState();
        Site = site;
        SiteId = site.Id;
        AppendEvent(AssetEventType.Relocated, actor, occurredAt, previousState, note);
    }

    public void ReturnToSite(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureNoPendingHandover();
        if (ResponsibleWorkerId is null || ResponsibleWorkerId != actor.Id)
        {
            throw new InvalidOperationException(
                "Only the responsible worker can return the asset.");
        }
        if (OperationalStatus == AssetOperationalStatus.Missing)
        {
            throw new InvalidOperationException("A missing asset cannot be returned.");
        }

        var previousState = CaptureState();
        ResponsibleWorker = null;
        ResponsibleWorkerId = null;
        OperationalStatus = AssetOperationalStatus.Available;
        AppendEvent(AssetEventType.Returned, actor, occurredAt, previousState, note);
    }

    public void ChangeOperationalStatus(
        AssetOperationalStatus operationalStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureNoPendingHandover();
        EnsureDefined(operationalStatus, nameof(operationalStatus));
        if (operationalStatus is AssetOperationalStatus.InTransfer
            or AssetOperationalStatus.Missing
            or AssetOperationalStatus.Retired)
        {
            throw new ArgumentException(
                "Transfer, missing, and retired states require their dedicated operations.",
                nameof(operationalStatus));
        }
        if (operationalStatus == OperationalStatus)
        {
            throw new InvalidOperationException("The asset already has the requested status.");
        }

        var previousState = CaptureState();
        OperationalStatus = operationalStatus;
        AppendEvent(
            AssetEventType.OperationalStatusChanged,
            actor,
            occurredAt,
            previousState,
            note);
    }

    public void ChangeCondition(
        AssetCondition condition,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureDefined(condition, nameof(condition));
        if (condition == Condition)
        {
            throw new InvalidOperationException("The asset already has the requested condition.");
        }

        var previousState = CaptureState();
        Condition = condition;
        AppendEvent(AssetEventType.ConditionChanged, actor, occurredAt, previousState, note);
    }

    public void ReportMissing(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureNoPendingHandover();
        if (OperationalStatus == AssetOperationalStatus.Missing)
        {
            throw new InvalidOperationException("The asset is already reported missing.");
        }

        var previousState = CaptureState();
        OperationalStatus = AssetOperationalStatus.Missing;
        AppendEvent(AssetEventType.MissingReported, actor, occurredAt, previousState, note);
    }

    public void Retire(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureNoPendingHandover();
        var previousState = CaptureState();
        OperationalStatus = AssetOperationalStatus.Retired;
        AppendEvent(AssetEventType.Retired, actor, occurredAt, previousState, note);
    }

    public void InitiateHandover(
        ApplicationUser recipient,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsureNoPendingHandover();
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient.Id);
        if (ResponsibleWorkerId is null || ResponsibleWorkerId != actor.Id)
        {
            throw new InvalidOperationException(
                "Only the responsible worker can initiate a handover.");
        }
        if (recipient.Id == ResponsibleWorkerId)
        {
            throw new ArgumentException(
                "The handover recipient must differ from the responsible worker.",
                nameof(recipient));
        }
        if (OperationalStatus == AssetOperationalStatus.Missing)
        {
            throw new InvalidOperationException("A missing asset cannot enter a handover.");
        }

        var previousState = CaptureState();
        PreHandoverOperationalStatus = OperationalStatus;
        PendingHandoverRecipient = recipient;
        PendingHandoverRecipientId = recipient.Id;
        OperationalStatus = AssetOperationalStatus.InTransfer;
        AppendEvent(AssetEventType.HandoverInitiated, actor, occurredAt, previousState, note);
    }

    public void AcceptHandover(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsurePendingHandoverFor(actor);
        var previousState = CaptureState();
        var restoredStatus = PreHandoverOperationalStatus!.Value;
        ResponsibleWorker = actor;
        ResponsibleWorkerId = actor.Id;
        ClearPendingHandover(restoredStatus);
        AppendEvent(AssetEventType.HandoverAccepted, actor, occurredAt, previousState, note);
    }

    public void RejectHandover(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureOperationallyMutable();
        EnsurePendingHandoverFor(actor);
        var previousState = CaptureState();
        ClearPendingHandover(PreHandoverOperationalStatus!.Value);
        AppendEvent(AssetEventType.HandoverRejected, actor, occurredAt, previousState, note);
    }

    public void AdministrativelyReassign(
        ApplicationUser? responsibleWorker,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string reason)
    {
        ValidateEventContext(actor, occurredAt, reason);
        EnsureOperationallyMutable();
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (responsibleWorker is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(responsibleWorker.Id);
        }

        var hasPendingHandover = PendingHandoverRecipientId is not null;
        if (!hasPendingHandover && responsibleWorker?.Id == ResponsibleWorkerId)
        {
            throw new InvalidOperationException("The requested responsibility is already current.");
        }

        var previousState = CaptureState();
        if (hasPendingHandover)
        {
            ClearPendingHandover(PreHandoverOperationalStatus!.Value);
        }
        SetResponsibleWorker(responsibleWorker);
        AppendEvent(
            AssetEventType.AdministrativeReassignment,
            actor,
            occurredAt,
            previousState,
            reason);
    }

    public void CorrectCatalogAssociation(
        AssetCatalogItem catalogItem,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureRegistrationNotRejected();
        ArgumentNullException.ThrowIfNull(catalogItem);
        if (catalogItem.Id == CatalogItemId)
        {
            throw new InvalidOperationException("The requested catalog item is already assigned.");
        }
        if (catalogItem.ReviewStatus == AssetReviewStatus.Rejected
            || (ReviewStatus == AssetReviewStatus.Active
                && catalogItem.ReviewStatus != AssetReviewStatus.Active))
        {
            throw new ArgumentException(
                "The catalog item is not valid for the asset's review state.",
                nameof(catalogItem));
        }

        var previousState = CaptureState();
        CatalogItem = catalogItem;
        CatalogItemId = catalogItem.Id;
        AppendEvent(
            AssetEventType.CatalogAssociationCorrected,
            actor,
            occurredAt,
            previousState,
            note);
    }

    public void CorrectManufacturerSerialNumber(
        string? manufacturerSerialNumber,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureRegistrationNotRejected();
        var normalizedSerialNumber = NormalizeSerialNumber(manufacturerSerialNumber);
        if (string.Equals(
                normalizedSerialNumber,
                ManufacturerSerialNumber,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The requested serial number is already current.");
        }

        var previousState = CaptureState();
        ManufacturerSerialNumber = normalizedSerialNumber;
        AppendEvent(
            AssetEventType.ManufacturerSerialCorrected,
            actor,
            occurredAt,
            previousState,
            note);
    }

    public void RotateQrIdentifier(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureRegistrationNotRejected();
        var previousState = CaptureState();
        var replacement = Guid.NewGuid();
        while (replacement == QrIdentifier)
        {
            replacement = Guid.NewGuid();
        }
        QrIdentifier = replacement;
        AppendEvent(AssetEventType.QrIdentifierRotated, actor, occurredAt, previousState, note);
    }

    private static Asset Register(
        AssetCatalogItem catalogItem,
        Site site,
        ApplicationUser? responsibleWorker,
        string? manufacturerSerialNumber,
        AssetCondition condition,
        AssetOperationalStatus operationalStatus,
        AssetReviewStatus reviewStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note)
    {
        ArgumentNullException.ThrowIfNull(catalogItem);
        ArgumentNullException.ThrowIfNull(site);
        ValidateEventContext(actor, occurredAt, note);
        EnsureDefined(condition, nameof(condition));
        EnsureValidInitialOperationalStatus(operationalStatus);

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            QrIdentifier = Guid.NewGuid(),
            CatalogItem = catalogItem,
            CatalogItemId = catalogItem.Id,
            ManufacturerSerialNumber = NormalizeSerialNumber(manufacturerSerialNumber),
            Site = site,
            SiteId = site.Id,
            Condition = condition,
            OperationalStatus = operationalStatus,
            ReviewStatus = reviewStatus
        };
        asset.SetResponsibleWorker(responsibleWorker);
        asset.AppendEvent(AssetEventType.Registered, actor, occurredAt, null, note);
        return asset;
    }

    private void AppendEvent(
        AssetEventType eventType,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        AssetStateSnapshot? previousState,
        string? note)
    {
        HistorySequence = checked(HistorySequence + 1);
        _assetEvents.Add(
            AssetEvent.Create(
                this,
                HistorySequence,
                eventType,
                actor,
                occurredAt,
                previousState,
                CaptureState(),
                note));
    }

    private AssetStateSnapshot CaptureState() =>
        new(
            CatalogItemId,
            QrIdentifier,
            ManufacturerSerialNumber,
            SiteId,
            ResponsibleWorkerId,
            PendingHandoverRecipientId,
            Condition,
            OperationalStatus,
            PreHandoverOperationalStatus,
            ReviewStatus);

    private void SetResponsibleWorker(ApplicationUser? responsibleWorker)
    {
        if (responsibleWorker is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(responsibleWorker.Id);
        }

        ResponsibleWorker = responsibleWorker;
        ResponsibleWorkerId = responsibleWorker?.Id;
    }

    private void ClearPendingHandover(AssetOperationalStatus restoredStatus)
    {
        PendingHandoverRecipient = null;
        PendingHandoverRecipientId = null;
        PreHandoverOperationalStatus = null;
        OperationalStatus = restoredStatus;
    }

    private void EnsurePendingReview()
    {
        if (ReviewStatus != AssetReviewStatus.PendingReview)
        {
            throw new InvalidOperationException("Only a pending asset registration can be reviewed.");
        }
    }

    private void EnsureRegistrationNotRejected()
    {
        if (ReviewStatus == AssetReviewStatus.Rejected)
        {
            throw new InvalidOperationException("A rejected asset registration cannot be changed.");
        }
    }

    private void EnsureOperationallyMutable()
    {
        if (ReviewStatus != AssetReviewStatus.Active)
        {
            throw new InvalidOperationException("Only an active asset registration can be operated.");
        }
        if (OperationalStatus == AssetOperationalStatus.Retired)
        {
            throw new InvalidOperationException("A retired asset cannot be operated.");
        }
    }

    private void EnsureNoPendingHandover()
    {
        if (PendingHandoverRecipientId is not null)
        {
            throw new InvalidOperationException(
                "The pending handover must be resolved before this change.");
        }
    }

    private void EnsurePendingHandoverFor(ApplicationUser actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (PendingHandoverRecipientId is null
            || PreHandoverOperationalStatus is null
            || OperationalStatus != AssetOperationalStatus.InTransfer)
        {
            throw new InvalidOperationException("The asset has no pending handover.");
        }
        if (actor.Id != PendingHandoverRecipientId)
        {
            throw new InvalidOperationException(
                "Only the intended recipient can resolve the handover.");
        }
    }

    private static void EnsureValidInitialOperationalStatus(AssetOperationalStatus status)
    {
        EnsureDefined(status, nameof(status));
        if (status is AssetOperationalStatus.InTransfer
            or AssetOperationalStatus.Missing
            or AssetOperationalStatus.Retired)
        {
            throw new ArgumentException(
                "An asset cannot be registered as in transfer, missing, or retired.",
                nameof(status));
        }
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported value.");
        }
    }

    private static string? NormalizeSerialNumber(string? manufacturerSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(manufacturerSerialNumber))
        {
            return null;
        }

        var normalized = manufacturerSerialNumber.Trim();
        if (normalized.Length > MaxManufacturerSerialNumberLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(manufacturerSerialNumber),
                normalized.Length,
                $"A manufacturer serial number cannot exceed {MaxManufacturerSerialNumberLength} characters.");
        }

        return normalized;
    }

    private static void ValidateEventContext(
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor.Id);
        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("The event timestamp must be UTC.", nameof(occurredAt));
        }

        var normalizedNote = note?.Trim();
        if (normalizedNote?.Length > AssetEvent.MaxNoteLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(note),
                normalizedNote.Length,
                $"An asset event note cannot exceed {AssetEvent.MaxNoteLength} characters.");
        }
    }
}
