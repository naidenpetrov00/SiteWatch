namespace Application.RetailerExtractionProfiles;

public enum RetailerExtractionTestFailureKind
{
    Security,
    Network,
    Content,
    Timeout
}

public sealed class RetailerExtractionTestException(
    RetailerExtractionTestFailureKind kind,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public RetailerExtractionTestFailureKind Kind { get; } = kind;
}
