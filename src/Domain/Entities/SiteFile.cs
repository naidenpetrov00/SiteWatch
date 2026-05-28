using Domain.SeedWork;

namespace Domain.Entities;

public sealed class SiteFile : BaseAuditableEntity
{
    private SiteFile()
    {
    }

    public SiteFile(Guid siteId, Guid fileId, string fileName, string contentType)
    {
        SiteId = siteId;
        FileId = fileId;
        FileName = fileName;
        ContentType = contentType;
    }

    public Guid SiteId { get; private set; }
    public Guid FileId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;

    public Site Site { get; private set; } = null!;
}
