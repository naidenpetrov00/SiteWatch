namespace Domain.SeedWork.Enums;

public enum PriceObservationSource
{
    Manual = 0,
    Automated = 1
}

public static class PriceObservationSourceCodes
{
    public static string ToCode(this PriceObservationSource source) => source switch
    {
        PriceObservationSource.Manual => "manual",
        PriceObservationSource.Automated => "automated",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };
}
