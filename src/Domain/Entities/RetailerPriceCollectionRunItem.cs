using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class RetailerPriceCollectionRunItem : BaseEntity
{
    public const int MaxDiagnosticCodeLength = 64;
    public const int MaxDiagnosticMessageLength = 1000;

    private RetailerPriceCollectionRunItem()
    {
    }

    public Guid RunId { get; private set; }
    public RetailerPriceCollectionRun Run { get; private set; } = null!;
    public Guid RetailerListingId { get; private set; }
    public RetailerListing RetailerListing { get; private set; } = null!;
    public RetailerPriceCollectionRunItemStatus Status { get; private set; }
    public string? CapturedProductUrl { get; private set; }
    public string? FinalSourceUrl { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public Guid? RetailerPriceObservationId { get; private set; }
    public RetailerPriceObservation? RetailerPriceObservation { get; private set; }
    public string? DiagnosticCode { get; private set; }
    public string? DiagnosticMessage { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    internal static RetailerPriceCollectionRunItem Create(
        RetailerPriceCollectionRun run,
        RetailerListing listing,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(listing);
        var item = new RetailerPriceCollectionRunItem
        {
            Id = Guid.NewGuid(),
            Run = run,
            RunId = run.Id,
            RetailerListing = listing,
            RetailerListingId = listing.Id,
            CapturedProductUrl = listing.ProductUrl,
            Status = RetailerPriceCollectionRunItemStatus.Queued
        };

        if (!listing.Retailer.IsActive)
        {
            item.Skip(createdAt, "retailerInactive", "The retailer location was inactive when the run was created.");
        }
        else if (!listing.IsActive)
        {
            item.Skip(createdAt, "listingInactive", "The retailer listing was inactive when the run was created.");
        }
        else if (string.IsNullOrWhiteSpace(listing.ProductUrl))
        {
            item.Skip(createdAt, "missingProductUrl", "The retailer listing did not have a saved product URL.");
        }

        return item;
    }

    public void Claim(Guid leaseToken, DateTimeOffset claimedAt, DateTimeOffset leaseExpiresAt)
    {
        if (Status != RetailerPriceCollectionRunItemStatus.Queued)
        {
            throw new InvalidOperationException("Only a queued collection item can be claimed.");
        }
        SetLease(leaseToken, claimedAt, leaseExpiresAt);
        Status = RetailerPriceCollectionRunItemStatus.Running;
        StartedAt = claimedAt;
    }

    public void Reclaim(Guid leaseToken, DateTimeOffset claimedAt, DateTimeOffset leaseExpiresAt)
    {
        if (Status != RetailerPriceCollectionRunItemStatus.Running
            || !LeaseExpiresAt.HasValue
            || LeaseExpiresAt.Value > claimedAt)
        {
            throw new InvalidOperationException("Only an expired running collection item can be reclaimed.");
        }
        SetLease(leaseToken, claimedAt, leaseExpiresAt);
    }

    public void Succeed(
        Guid leaseToken,
        RetailerPriceObservation observation,
        string finalSourceUrl,
        DateTimeOffset completedAt)
    {
        EnsureOwnedRunningLease(leaseToken);
        ArgumentNullException.ThrowIfNull(observation);
        if (observation.RetailerListingId != RetailerListingId)
        {
            throw new ArgumentException("The observation belongs to another retailer listing.");
        }
        if (string.IsNullOrWhiteSpace(finalSourceUrl)
            || finalSourceUrl.Length > RetailerListing.MaxProductUrlLength)
        {
            throw new ArgumentException("The final source URL is invalid.", nameof(finalSourceUrl));
        }

        Status = RetailerPriceCollectionRunItemStatus.Succeeded;
        RetailerPriceObservation = observation;
        RetailerPriceObservationId = observation.Id;
        FinalSourceUrl = finalSourceUrl;
        CompletedAt = completedAt;
        ClearLease();
    }

    public void Fail(Guid leaseToken, DateTimeOffset completedAt, string code, string message)
    {
        EnsureOwnedRunningLease(leaseToken);
        Status = RetailerPriceCollectionRunItemStatus.Failed;
        CompletedAt = completedAt;
        SetDiagnostic(code, message);
        ClearLease();
    }

    public void SkipRunning(Guid leaseToken, DateTimeOffset completedAt, string code, string message)
    {
        EnsureOwnedRunningLease(leaseToken);
        Status = RetailerPriceCollectionRunItemStatus.Skipped;
        CompletedAt = completedAt;
        SetDiagnostic(code, message);
        ClearLease();
    }

    private void Skip(DateTimeOffset completedAt, string code, string message)
    {
        Status = RetailerPriceCollectionRunItemStatus.Skipped;
        CompletedAt = completedAt;
        SetDiagnostic(code, message);
    }

    private void SetLease(Guid leaseToken, DateTimeOffset claimedAt, DateTimeOffset leaseExpiresAt)
    {
        if (leaseToken == Guid.Empty || leaseExpiresAt <= claimedAt)
        {
            throw new ArgumentException("The collection item lease is invalid.");
        }
        LeaseToken = leaseToken;
        LeaseExpiresAt = leaseExpiresAt;
    }

    private void EnsureOwnedRunningLease(Guid leaseToken)
    {
        if (Status != RetailerPriceCollectionRunItemStatus.Running
            || LeaseToken != leaseToken)
        {
            throw new InvalidOperationException("The collection item lease is no longer owned.");
        }
    }

    private void SetDiagnostic(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        DiagnosticCode = code.Trim().Length <= MaxDiagnosticCodeLength
            ? code.Trim()
            : code.Trim()[..MaxDiagnosticCodeLength];
        DiagnosticMessage = message.Trim().Length <= MaxDiagnosticMessageLength
            ? message.Trim()
            : message.Trim()[..MaxDiagnosticMessageLength];
    }

    private void ClearLease()
    {
        LeaseToken = null;
        LeaseExpiresAt = null;
    }
}
