using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Asset : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    public const int MaxNameLength = 200;
    public const int MaxManufacturerLength = 100;
    public const int MaxModelLength = 100;
    public const int MaxDescriptionLength = 2000;
    public const int MaxManufacturerSerialNumberLength = 100;

    private readonly List<AssetEvent> _assetEvents = [];

    private Asset()
    {
    }

    public int NumberId { get; private set; }

    public string? AssetCode =>
        NumberId > 0 ? $"AST-{NumberId:D6}" : null;

    public Guid QrIdentifier { get; private set; }
    public AssetClassification Classification { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? Description { get; private set; }
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
        AssetClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
        Site site,
        ApplicationUser? responsibleWorker,
        string? manufacturerSerialNumber,
        AssetCondition condition,
        AssetOperationalStatus operationalStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        return Register(
            classification,
            name,
            manufacturer,
            model,
            description,
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
        AssetClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
        Site site,
        ApplicationUser? responsibleWorker,
        string? manufacturerSerialNumber,
        AssetCondition condition,
        AssetOperationalStatus operationalStatus,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        return Register(
            classification,
            name,
            manufacturer,
            model,
            description,
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

    public void UpdateDetails(
        AssetClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
        string? manufacturerSerialNumber,
        ApplicationUser actor,
        DateTimeOffset occurredAt,
        string? note = null)
    {
        ValidateEventContext(actor, occurredAt, note);
        EnsureRegistrationNotRejected();
        EnsureDefined(classification, nameof(classification));
        var normalizedName = NormalizeRequired(name, MaxNameLength, nameof(name));
        var normalizedManufacturer = NormalizeOptional(
            manufacturer,
            MaxManufacturerLength,
            nameof(manufacturer));
        var normalizedModel = NormalizeOptional(model, MaxModelLength, nameof(model));
        var normalizedDescription = NormalizeOptional(
            description,
            MaxDescriptionLength,
            nameof(description),
            collapseWhitespace: false);
        var normalizedSerialNumber = NormalizeSerialNumber(manufacturerSerialNumber);
        if (classification == Classification
            && string.Equals(normalizedName, Name, StringComparison.Ordinal)
            && string.Equals(normalizedManufacturer, Manufacturer, StringComparison.Ordinal)
            && string.Equals(normalizedModel, Model, StringComparison.Ordinal)
            && string.Equals(normalizedDescription, Description, StringComparison.Ordinal)
            && string.Equals(
                normalizedSerialNumber,
                ManufacturerSerialNumber,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The requested asset details are already current.");
        }

        var previousState = CaptureState();
        Classification = classification;
        Name = normalizedName;
        Manufacturer = normalizedManufacturer;
        Model = normalizedModel;
        Description = normalizedDescription;
        ManufacturerSerialNumber = normalizedSerialNumber;
        AppendEvent(
            AssetEventType.AssetDetailsUpdated,
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
        AssetClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
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
        ArgumentNullException.ThrowIfNull(site);
        ValidateEventContext(actor, occurredAt, note);
        EnsureDefined(classification, nameof(classification));
        EnsureDefined(condition, nameof(condition));
        EnsureValidInitialOperationalStatus(operationalStatus);

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            QrIdentifier = Guid.NewGuid(),
            Classification = classification,
            Name = NormalizeRequired(name, MaxNameLength, nameof(name)),
            Manufacturer = NormalizeOptional(
                manufacturer,
                MaxManufacturerLength,
                nameof(manufacturer)),
            Model = NormalizeOptional(model, MaxModelLength, nameof(model)),
            Description = NormalizeOptional(
                description,
                MaxDescriptionLength,
                nameof(description),
                collapseWhitespace: false),
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
            Classification,
            Name,
            Manufacturer,
            Model,
            Description,
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

    private static string NormalizeRequired(
        string value,
        int maxLength,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return Normalize(value, maxLength, parameterName, collapseWhitespace: true);
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace = true) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Normalize(value, maxLength, parameterName, collapseWhitespace);

    private static string Normalize(
        string value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace)
    {
        var normalized = collapseWhitespace
            ? string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            : value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                normalized.Length,
                $"The value cannot exceed {maxLength} characters.");
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
