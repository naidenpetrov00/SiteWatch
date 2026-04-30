namespace Application.SeedWork.Models.Internal;

public sealed record BlobContainerName
{
    private string Value { get; }

    private BlobContainerName(string value) => Value = value;

    public static BlobContainerName Images { get; } = new("images");
    public static BlobContainerName Files { get; } = new("files");

    public override string ToString() => Value;
}