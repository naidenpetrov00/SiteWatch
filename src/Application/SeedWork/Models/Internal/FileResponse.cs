namespace Application.SeedWork.Models.Internal;

public record FileResponse(Stream Stream, string ContentType);

public sealed record InvoiceFileResponse(
    Stream Stream,
    string FileName,
    string ContentType,
    long ContentLength);
