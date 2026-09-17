namespace Application.Retailers;

public sealed class RetailerConflictException(
    string message,
    Exception? innerException = null) : Exception(message, innerException);
