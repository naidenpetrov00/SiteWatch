namespace Application.SeedWork.Exceptions;

public sealed class OpenRouterInvoiceExtractionException : Exception
{
    public OpenRouterInvoiceExtractionException(
        string message,
        string rawResponse,
        int? statusCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        RawResponse = rawResponse;
        StatusCode = statusCode;
    }

    public string RawResponse { get; }

    public int? StatusCode { get; }
}
