using FluentValidation;

namespace Application.Invoices.Commands;

public class UploadInvoiceCommandValidator : AbstractValidator<UploadInvoiceCommand>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png"
    ];

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    ];

    public UploadInvoiceCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty()
            .WithMessage("Site Id is required.");

        RuleFor(x => x.Stream)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(stream => stream.CanRead)
            .WithMessage("Invoice file stream is not readable.");

        RuleFor(x => x.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(255)
            .Must(BeSafeFileName)
            .WithMessage("File name is invalid.")
            .Must(HaveAllowedExtension)
            .WithMessage("Invoice files must be PDF or image files.");

        RuleFor(x => x.ContentType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeAllowedContentType)
            .WithMessage("Invoice files must be PDF or image files.");

        RuleFor(x => x)
            .Must(HaveMatchingFileType)
            .WithMessage("Invoice file extension and content type do not match.");

        RuleFor(x => x.ContentLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(20 * 1024 * 1024)
            .WithMessage("Invoice file must be between 1 byte and 20 MB.");
    }

    private static bool BeSafeFileName(string fileName)
        => !string.IsNullOrWhiteSpace(fileName)
           && fileName == Path.GetFileName(fileName)
           && !fileName.Contains("..", StringComparison.Ordinal);

    private static bool HaveAllowedExtension(string fileName)
        => !string.IsNullOrWhiteSpace(fileName)
           && AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    private static bool BeAllowedContentType(string contentType)
        => !string.IsNullOrWhiteSpace(contentType)
           && AllowedContentTypes.Contains(contentType.ToLowerInvariant());

    private static bool HaveMatchingFileType(UploadInvoiceCommand model)
    {
        if (string.IsNullOrWhiteSpace(model.FileName) || string.IsNullOrWhiteSpace(model.ContentType))
        {
            return false;
        }

        var extension = Path.GetExtension(model.FileName).ToLowerInvariant();
        var contentType = model.ContentType.ToLowerInvariant();

        return extension switch
        {
            ".pdf" => contentType == "application/pdf",
            ".jpg" or ".jpeg" => contentType == "image/jpeg",
            ".png" => contentType == "image/png",
            _ => false
        };
    }
}
