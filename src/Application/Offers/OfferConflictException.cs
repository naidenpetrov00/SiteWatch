namespace Application.Offers;

public sealed class OfferConflictException : Exception
{
    public OfferConflictException(string message) : base(message)
    {
    }
}
