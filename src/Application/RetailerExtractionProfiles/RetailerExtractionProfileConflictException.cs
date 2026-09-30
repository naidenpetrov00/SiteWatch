namespace Application.RetailerExtractionProfiles;

public sealed class RetailerExtractionProfileConflictException : Exception
{
    public RetailerExtractionProfileConflictException(string message) : base(message)
    {
    }

    public RetailerExtractionProfileConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
