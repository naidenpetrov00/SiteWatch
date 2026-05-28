using Application.SeedWork.Interfaces;
using Application.Sites.Files.Queries;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public class FilesService(IApplicationDbContext dbContext) : IFilesService
{
    public Task<List<SiteFileIdsDto>> GetFilesIdsBySiteId(Guid siteId) => dbContext.SiteFiles
        .AsNoTracking()
        .Where(siteFile => siteFile.SiteId == siteId)
        .Select(siteFile => new SiteFileIdsDto(
            siteFile.FileId,
            siteFile.FileName,
            siteFile.ContentType))
        .ToListAsync();

    public async Task AddFileIdsToSiteAsync(
        Guid requestSiteId,
        Guid resultFileId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        dbContext.SiteFiles.Add(new SiteFile(requestSiteId, resultFileId, fileName, contentType));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteFileIdFromSiteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var siteFile = await dbContext.SiteFiles.FirstOrDefaultAsync(sf => sf.FileId == fileId, cancellationToken);

        if (siteFile is null)
        {
            return;
        }

        var site = await dbContext.Sites.FirstAsync(s => s.Id == siteFile.SiteId, cancellationToken);
        site.RemoveFile(siteFile);
        dbContext.SiteFiles.Remove(siteFile);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
