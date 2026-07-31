using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class SiteFile : BaseAuditableEntity
{
    private SiteFile()
    {
    }

    public SiteFile(
        Guid siteId,
        Guid fileId,
        string fileName,
        string contentType,
        FileCategory category,
        FileDocumentType documentType)
    {
        SiteId = siteId;
        FileId = fileId;
        FileName = fileName;
        ContentType = contentType;
        Category = category;
        DocumentType = documentType;
    }

    public Guid SiteId { get; private set; }
    public Guid FileId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public FileCategory Category { get; private set; }
    public FileDocumentType DocumentType { get; private set; }

    public Site Site { get; private set; } = null!;
}
