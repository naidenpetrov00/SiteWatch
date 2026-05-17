namespace Application.Sites.Videos.Commands;

public record UploadedFile
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
}
