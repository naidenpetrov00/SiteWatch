using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class RetailerExtractionAllowedHost : BaseEntity
{
    private RetailerExtractionAllowedHost()
    {
    }

    public Guid ExtractionProfileId { get; private set; }
    public RetailerExtractionProfile ExtractionProfile { get; private set; } = null!;
    public string NormalizedHost { get; private set; } = string.Empty;

    internal static RetailerExtractionAllowedHost Create(
        RetailerExtractionProfile profile,
        string host)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new RetailerExtractionAllowedHost
        {
            Id = Guid.NewGuid(),
            ExtractionProfile = profile,
            ExtractionProfileId = profile.Id,
            NormalizedHost = RetailerExtractionHost.Create(host).Value
        };
    }
}
