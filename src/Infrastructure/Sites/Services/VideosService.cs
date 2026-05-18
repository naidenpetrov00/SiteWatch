using Application.SeedWork.Interfaces;
using Application.Sites.Videos.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Infrastructure.Sites.Services;

public class VideosService(IApplicationDbContext dbContext) : IVideosService
{
    public async Task<Stream> CreateSnapshotAsync(Stream originalStream, CancellationToken cancellationToken = default)
    {
        var inputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mp4");
        var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.jpg");

        try
        {
            if (originalStream.CanSeek)
            {
                originalStream.Position = 0;
            }

            await using (var inputFile = File.Create(inputPath))
            {
                await originalStream.CopyToAsync(inputFile, cancellationToken);
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            processStartInfo.ArgumentList.Add("-y");
            processStartInfo.ArgumentList.Add("-i");
            processStartInfo.ArgumentList.Add(inputPath);
            processStartInfo.ArgumentList.Add("-frames:v");
            processStartInfo.ArgumentList.Add("1");
            processStartInfo.ArgumentList.Add("-q:v");
            processStartInfo.ArgumentList.Add("2");
            processStartInfo.ArgumentList.Add(outputPath);

            using var process = Process.Start(processStartInfo)
                ?? throw new InvalidOperationException("Failed to start ffmpeg.");

            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg failed to create a snapshot: {stderr}");
            }

            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException("ffmpeg did not produce a snapshot file.");
            }

            var output = new MemoryStream();
            await using (var snapshotFile = File.OpenRead(outputPath))
            {
                await snapshotFile.CopyToAsync(output, cancellationToken);
            }

            output.Position = 0;
            return output;
        }
        finally
        {
            if (File.Exists(inputPath))
            {
                File.Delete(inputPath);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public Task<List<SiteVideoIdsDto>> GetVideosIdsBySiteId(Guid siteId) => dbContext.SiteVideos
        .AsNoTracking()
        .Where(siteVideo => siteVideo.SiteId == siteId)
        .Select(siteVideo => new SiteVideoIdsDto(
            siteVideo.VideoId,
            siteVideo.SnapshotId,
            siteVideo.DurationSeconds,
            siteVideo.Category))
        .ToListAsync();

    public async Task AddVideoIdsToSiteAsync(
        Guid requestSiteId,
        Guid resultVideoFileId,
        Guid resultSnapshotFileId,
        int? durationSeconds,
        VideoCategory category,
        CancellationToken cancellationToken = default)
    {
        dbContext.SiteVideos.Add(new SiteVideo(
            requestSiteId,
            resultVideoFileId,
            resultSnapshotFileId,
            durationSeconds,
            category));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid?> DeleteVideoIdFromSiteAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var siteVideo = await dbContext.SiteVideos.FirstOrDefaultAsync(sv => sv.VideoId == videoId, cancellationToken);

        if (siteVideo is null)
        {
            return null;
        }

        var snapshotId = siteVideo.SnapshotId;
        var site = await dbContext.Sites.FirstAsync(s => s.Id == siteVideo.SiteId, cancellationToken);
        site.RemoveVideo(siteVideo);
        dbContext.SiteVideos.Remove(siteVideo);

        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshotId;
    }
}
