namespace Application.SeedWork.Models.Internal;

public sealed record BlobContainerName
{
    private string Value { get; }

    private BlobContainerName(string value) => Value = value;

    public static BlobContainerName Images { get; } = new("images");
    public static BlobContainerName Videos { get; } = new("videos");
    public static BlobContainerName Files { get; } = new("files");
    public static BlobContainerName Invoices { get; } = new("invoices");

    public override string ToString() => Value;
}
