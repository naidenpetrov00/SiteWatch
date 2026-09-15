namespace Application.ActivityCatalog;

public sealed class ActivityCatalogConflictException(
    string message,
    Exception? innerException = null) : Exception(message, innerException);
