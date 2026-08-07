using Application.Invoices.Commands;
using FluentValidation;

namespace Application.Tests.Invoices;

public sealed class InvoiceFileValidationTests
{
    [Theory]
    [InlineData("application/pdf", "invoice.pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D })]
    [InlineData("image/jpeg", "invoice.jpg", new byte[] { 0xFF, 0xD8, 0xFF })]
    [InlineData("image/png", "invoice.png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    [InlineData("image/webp", "invoice.webp", new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50 })]
    [InlineData("image/gif", "invoice.gif", new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })]
    [InlineData("image/heic", "invoice.heic", new byte[] { 0, 0, 0, 0, 0x66, 0x74, 0x79, 0x70, 0x68, 0x65, 0x69, 0x63 })]
    [InlineData("image/heif", "invoice.heif", new byte[] { 0, 0, 0, 0, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x69, 0x66, 0x31 })]
    public async Task ValidateAndBufferAsync_accepts_supported_content_matching_the_declared_type(
        string contentType, string fileName, byte[] contents)
    {
        await using var original = new MemoryStream(contents);
        await using var file = await InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(original, fileName, contentType, contents.Length), CancellationToken.None);

        Assert.Equal(fileName, file.FileName);
        Assert.Equal(contentType, file.ContentType);
        Assert.Equal(contents.Length, file.Length);
    }

    [Theory]
    [InlineData("application/pdf", "invoice.pdf", new byte[] { 0xFF, 0xD8, 0xFF })]
    [InlineData("text/plain", "invoice.txt", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D })]
    [InlineData("application/pdf", "../../", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D })]
    public async Task ValidateAndBufferAsync_rejects_mismatched_or_unsafe_files(
        string contentType, string fileName, byte[] contents)
    {
        await using var stream = new MemoryStream(contents);

        await Assert.ThrowsAsync<ValidationException>(() => InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(stream, fileName, contentType, contents.Length), CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAndBufferAsync_rejects_empty_files()
    {
        await using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ValidationException>(() => InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(stream, "invoice.pdf", "application/pdf", 0), CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAndBufferAsync_rejects_a_declared_size_that_does_not_match_the_stream()
    {
        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);

        await Assert.ThrowsAsync<ValidationException>(() => InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(stream, "invoice.pdf", "application/pdf", 6), CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAndBufferAsync_rejects_a_declared_file_larger_than_the_limit_before_reading_it()
    {
        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);

        await Assert.ThrowsAsync<ValidationException>(() => InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(stream, "invoice.pdf", "application/pdf", InvoiceFileValidation.MaxFileSize + 1),
            CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAndBufferAsync_sanitizes_a_path_to_its_file_name()
    {
        await using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46, 0x2D]);
        await using var file = await InvoiceFileValidation.ValidateAndBufferAsync(
            new UploadedInvoiceFile(stream, "C:\\uploads\\ invoice.pdf ", "application/pdf", 5), CancellationToken.None);

        Assert.Equal("invoice.pdf", file.FileName);
    }
}
