using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace Infrastructure.Storage;

internal sealed record VideoInspectionResult(int? DurationSeconds);

internal interface IVideoFileInspector
{
    Task<VideoInspectionResult> InspectAsync(
        string inputPath,
        string declaredContentType,
        CancellationToken cancellationToken);
}

internal sealed class FfprobeVideoFileInspector : IVideoFileInspector
{
    public async Task<VideoInspectionResult> InspectAsync(
        string inputPath,
        string declaredContentType,
        CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        processStartInfo.ArgumentList.Add("-v");
        processStartInfo.ArgumentList.Add("error");
        processStartInfo.ArgumentList.Add("-show_entries");
        processStartInfo.ArgumentList.Add("stream=codec_type:format=format_name,duration:format_tags=major_brand");
        processStartInfo.ArgumentList.Add("-of");
        processStartInfo.ArgumentList.Add("json");
        processStartInfo.ArgumentList.Add(inputPath);

        using var process = Process.Start(processStartInfo)
            ?? throw new InvalidOperationException("Failed to start ffprobe.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        _ = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw InvalidVideo();
        }

        var webmDocType = declaredContentType == "video/webm"
            ? await ReadEbmlDocTypeAsync(inputPath, cancellationToken)
            : null;
        return ParseAndValidate(stdout, declaredContentType, webmDocType);
    }

    internal static VideoInspectionResult ParseAndValidate(
        string probeOutput,
        string declaredContentType,
        string? webmDocType = null)
    {
        try
        {
            using var document = JsonDocument.Parse(probeOutput);
            var root = document.RootElement;
            if (!HasVideoStream(root)
                || !root.TryGetProperty("format", out var format)
                || !format.TryGetProperty("format_name", out var formatNameElement))
            {
                throw InvalidVideo();
            }

            var formatNames = (formatNameElement.GetString() ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var majorBrand = GetMajorBrand(format);

            if (!ContentTypeMatchesContainer(
                    declaredContentType,
                    formatNames,
                    majorBrand,
                    webmDocType))
            {
                throw InvalidVideo();
            }

            return new VideoInspectionResult(GetDurationSeconds(format));
        }
        catch (Exception exception) when (exception is JsonException
                                          or InvalidOperationException
                                          or FormatException
                                          or OverflowException)
        {
            throw InvalidVideo();
        }
    }

    private static bool HasVideoStream(JsonElement root) =>
        root.TryGetProperty("streams", out var streams)
        && streams.ValueKind == JsonValueKind.Array
        && streams.EnumerateArray().Any(stream =>
            stream.TryGetProperty("codec_type", out var codecType)
            && string.Equals(codecType.GetString(), "video", StringComparison.OrdinalIgnoreCase));

    private static string? GetMajorBrand(JsonElement format)
    {
        if (!format.TryGetProperty("tags", out var tags)
            || !tags.TryGetProperty("major_brand", out var majorBrand))
        {
            return null;
        }

        return majorBrand.GetString()?.Trim().ToLowerInvariant();
    }

    private static bool ContentTypeMatchesContainer(
        string declaredContentType,
        IReadOnlyCollection<string> formatNames,
        string? majorBrand,
        string? webmDocType)
    {
        if (declaredContentType == "video/webm")
        {
            return formatNames.Contains("webm", StringComparer.OrdinalIgnoreCase)
                   && string.Equals(webmDocType, "webm", StringComparison.OrdinalIgnoreCase);
        }

        if (declaredContentType is not ("video/mp4" or "video/quicktime"))
        {
            return false;
        }

        var isIsoBaseMedia = formatNames.Contains("mov", StringComparer.OrdinalIgnoreCase)
                             || formatNames.Contains("mp4", StringComparer.OrdinalIgnoreCase);
        return isIsoBaseMedia && !IsUnsupportedIsoBaseMediaBrand(majorBrand);
    }

    private static bool IsUnsupportedIsoBaseMediaBrand(string? majorBrand) =>
        majorBrand is not null
        && (majorBrand.StartsWith("3g", StringComparison.OrdinalIgnoreCase)
            || majorBrand.StartsWith("mj2", StringComparison.OrdinalIgnoreCase)
            || majorBrand.StartsWith("mjp2", StringComparison.OrdinalIgnoreCase));

    private static async Task<string?> ReadEbmlDocTypeAsync(
        string inputPath,
        CancellationToken cancellationToken)
    {
        const int headerReadLimit = 4096;
        var buffer = new byte[headerReadLimit];
        await using var stream = File.OpenRead(inputPath);
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        return ReadEbmlDocType(buffer.AsSpan(0, bytesRead));
    }

    internal static string? ReadEbmlDocType(ReadOnlySpan<byte> header)
    {
        if (header.Length < 4
            || header[0] != 0x1A
            || header[1] != 0x45
            || header[2] != 0xDF
            || header[3] != 0xA3)
        {
            return null;
        }

        for (var index = 0; index + 3 < header.Length; index++)
        {
            if (header[index] != 0x42 || header[index + 1] != 0x82)
            {
                continue;
            }

            var sizeOffset = index + 2;
            var sizeLength = GetEbmlVariableIntegerLength(header[sizeOffset]);
            if (sizeLength == 0 || sizeOffset + sizeLength > header.Length)
            {
                return null;
            }

            long size = header[sizeOffset] & (0xFF >> sizeLength);
            for (var offset = 1; offset < sizeLength; offset++)
            {
                size = (size << 8) | header[sizeOffset + offset];
            }

            var valueOffset = sizeOffset + sizeLength;
            if (size <= 0 || size > header.Length - valueOffset)
            {
                return null;
            }

            return Encoding.ASCII.GetString(header.Slice(valueOffset, (int)size));
        }

        return null;
    }

    private static int GetEbmlVariableIntegerLength(byte firstByte)
    {
        for (var length = 1; length <= 8; length++)
        {
            if ((firstByte & (0x80 >> (length - 1))) != 0)
            {
                return length;
            }
        }

        return 0;
    }

    private static int? GetDurationSeconds(JsonElement format)
    {
        if (!format.TryGetProperty("duration", out var durationElement))
        {
            return null;
        }

        var hasDuration = durationElement.ValueKind switch
        {
            JsonValueKind.Number => durationElement.TryGetDouble(out _),
            JsonValueKind.String => double.TryParse(
                durationElement.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out _),
            _ => false,
        };

        if (!hasDuration)
        {
            return null;
        }

        var duration = durationElement.ValueKind == JsonValueKind.Number
            ? durationElement.GetDouble()
            : double.Parse(durationElement.GetString()!, CultureInfo.InvariantCulture);
        if (!double.IsFinite(duration) || duration > int.MaxValue)
        {
            return null;
        }

        return duration <= 0
            ? 0
            : (int)Math.Round(duration, MidpointRounding.AwayFromZero);
    }

    private static ValidationException InvalidVideo() =>
        new([new ValidationFailure(
            "file",
            "The video content is invalid or does not match its declared type.")]);
}
