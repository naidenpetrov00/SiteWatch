using FluentValidation;
using FluentValidation.Results;

namespace Application.Invoices.Commands;

public static class InvoiceFileValidation
{
    public const long MaxFileSize = 20L * 1024L * 1024L;
    public const long MaxRequestSize = MaxFileSize + 1024L * 1024L;

    private static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/heic",
            "image/heif"
        };

    public static async Task<UploadedInvoiceFile> ValidateAndBufferAsync(
        UploadedInvoiceFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            throw InvalidFile("The invoice file cannot be empty.");
        }

        var fileName = SanitizeFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw InvalidFile("The invoice file name is required.");
        }

        if (file.Length > MaxFileSize)
        {
            throw InvalidFile("The invoice file cannot exceed 20 MB.");
        }

        var contentType = file.ContentType.Trim();
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw InvalidFile("Only PDF and supported image files are allowed.");
        }

        var buffer = new MemoryStream((int)file.Length);
        try
        {
            await file.Stream.CopyToAsync(buffer, cancellationToken);
            if (buffer.Length != file.Length || buffer.Length > MaxFileSize)
            {
                throw InvalidFile("The invoice file size is invalid.");
            }

            var detectedContentType = DetectContentType(buffer.GetBuffer(), checked((int)buffer.Length));
            if (!ContentTypesMatch(contentType, detectedContentType))
            {
                throw InvalidFile("The invoice file content does not match its declared type.");
            }

            buffer.Position = 0;
            return new UploadedInvoiceFile(
                buffer,
                fileName,
                detectedContentType,
                buffer.Length);
        }
        catch
        {
            await buffer.DisposeAsync();
            throw;
        }
    }

    private static string DetectContentType(byte[] bytes, int length)
    {
        if (length >= 5
            && bytes[0] == 0x25
            && bytes[1] == 0x50
            && bytes[2] == 0x44
            && bytes[3] == 0x46
            && bytes[4] == 0x2D)
        {
            return "application/pdf";
        }

        if (length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (length >= 8
            && bytes[0] == 0x89
            && bytes[1] == 0x50
            && bytes[2] == 0x4E
            && bytes[3] == 0x47
            && bytes[4] == 0x0D
            && bytes[5] == 0x0A
            && bytes[6] == 0x1A
            && bytes[7] == 0x0A)
        {
            return "image/png";
        }

        if (length >= 12
            && IsAscii(bytes, 0, "RIFF")
            && IsAscii(bytes, 8, "WEBP"))
        {
            return "image/webp";
        }

        if (length >= 6 && (IsAscii(bytes, 0, "GIF87a") || IsAscii(bytes, 0, "GIF89a")))
        {
            return "image/gif";
        }

        if (length >= 12 && IsAscii(bytes, 4, "ftyp"))
        {
            var brand = System.Text.Encoding.ASCII.GetString(bytes, 8, 4);
            if (brand is "heic" or "heix" or "hevc" or "hevx")
            {
                return "image/heic";
            }

            if (brand is "mif1" or "msf1")
            {
                return "image/heif";
            }
        }

        return string.Empty;
    }

    private static bool ContentTypesMatch(string declared, string detected) =>
        string.Equals(declared, detected, StringComparison.OrdinalIgnoreCase)
        || (declared.StartsWith("image/hei", StringComparison.OrdinalIgnoreCase)
            && detected.StartsWith("image/hei", StringComparison.OrdinalIgnoreCase));

    private static bool IsAscii(byte[] bytes, int offset, string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (bytes[offset + index] != value[index])
            {
                return false;
            }
        }

        return true;
    }

    private static string SanitizeFileName(string fileName)
    {
        var leafName = fileName
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault()
            ?.Trim();

        if (string.IsNullOrWhiteSpace(leafName) || leafName is "." or "..")
        {
            return string.Empty;
        }

        var invalidCharacters = Path.GetInvalidFileNameChars()
            .Concat(['<', '>', ':', '"', '/', '\\', '|', '?', '*'])
            .ToHashSet();
        var sanitizedName = new string(leafName
            .Where(character => !char.IsControl(character) && !invalidCharacters.Contains(character))
            .ToArray())
            .Trim();

        return sanitizedName.Length <= 255
            ? sanitizedName
            : sanitizedName[..255];
    }

    private static ValidationException InvalidFile(string message) =>
        new([new ValidationFailure("file", message)]);
}
