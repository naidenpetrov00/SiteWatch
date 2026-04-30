namespace Application.Sites.Images.Commands;

public record UploadedFile
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
}