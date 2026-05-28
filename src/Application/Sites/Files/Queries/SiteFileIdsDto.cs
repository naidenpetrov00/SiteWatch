namespace Application.Sites.Files.Queries;

public sealed record SiteFileIdsDto(
    Guid FileId,
    string FileName,
    string ContentType);
