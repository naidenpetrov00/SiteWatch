namespace Application.RetailerPriceCollections;

public sealed class RetailerPriceCollectionConflictException(
    string message,
    Exception? innerException = null) : Exception(message, innerException);
