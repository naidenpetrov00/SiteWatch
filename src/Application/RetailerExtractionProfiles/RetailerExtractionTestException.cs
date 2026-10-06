namespace Application.RetailerExtractionProfiles;

public enum RetailerExtractionFailureKind
{
    Security,
    Network,
    Content,
    Timeout
}

public sealed class RetailerExtractionException(
    RetailerExtractionFailureKind kind,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public RetailerExtractionFailureKind Kind { get; } = kind;
}
