namespace Application.RetailerListings;

public sealed class RetailerListingConflictException(
    string message,
    Exception? innerException = null) : Exception(message, innerException);
