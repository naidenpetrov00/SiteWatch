namespace Infrastructure.Data;

internal sealed class RetailerPriceCollectionWorkerLease
{
    internal const string SingletonId = "retailer-price-collection";

    public string Id { get; set; } = SingletonId;
    public Guid OwnerId { get; set; }
    public DateTimeOffset LeaseExpiresAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
