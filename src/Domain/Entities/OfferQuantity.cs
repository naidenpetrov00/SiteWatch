namespace Domain.Entities;

internal static class OfferQuantity
{
    public static decimal ValidateInput(decimal value, string parameterName)
    {
        if (value <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Quantity must be greater than zero.");
        }

        if (decimal.Round(value, 4) != value)
        {
            throw new ArgumentException(
                "Quantity cannot have more than four decimal places.",
                parameterName);
        }

        return value;
    }

    public static decimal CalculateProportional(
        decimal configuredQuantity,
        decimal requestedMeasurement,
        decimal basisQuantity)
    {
        ValidateInput(requestedMeasurement, nameof(requestedMeasurement));
        ValidateInput(basisQuantity, nameof(basisQuantity));
        return RoundCalculated(configuredQuantity * requestedMeasurement / basisQuantity);
    }

    public static decimal RoundCalculated(decimal value) =>
        decimal.Round(value, 8, MidpointRounding.AwayFromZero);
}
