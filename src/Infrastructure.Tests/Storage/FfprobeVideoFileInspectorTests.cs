using FluentValidation;
using Infrastructure.Storage;

namespace Infrastructure.Tests.Storage;

public sealed class FfprobeVideoFileInspectorTests
{
    [Theory]
    [InlineData("webm")]
    [InlineData("matroska")]
    public void ReadEbmlDocType_reads_the_declared_container_from_the_ebml_header(string docType)
    {
        var docTypeBytes = System.Text.Encoding.ASCII.GetBytes(docType);
        var header = new byte[7 + docTypeBytes.Length];
        new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x42, 0x82, (byte)(0x80 | docTypeBytes.Length) }
            .CopyTo(header, 0);
        docTypeBytes.CopyTo(header, 7);

        var result = FfprobeVideoFileInspector.ReadEbmlDocType(header);

        Assert.Equal(docType, result);
    }

    [Theory]
    [InlineData("video/mp4", "isom")]
    [InlineData("video/quicktime", "qt  ")]
    [InlineData("video/mp4", "qt  ")]
    [InlineData("video/quicktime", "mp42")]
    public void ParseAndValidate_accepts_mp4_and_mov_as_compatible_iso_base_media(
        string declaredContentType,
        string majorBrand)
    {
        var output = IsoBaseMediaProbeOutput(majorBrand);

        var result = FfprobeVideoFileInspector.ParseAndValidate(output, declaredContentType);

        Assert.Equal(13, result.DurationSeconds);
    }

    [Fact]
    public void ParseAndValidate_accepts_webm_with_a_video_stream()
    {
        const string output = """
            {
              "streams": [{ "codec_type": "video" }],
              "format": { "format_name": "matroska,webm", "duration": "2.4" }
            }
            """;

        var result = FfprobeVideoFileInspector.ParseAndValidate(
            output,
            "video/webm",
            "webm");

        Assert.Equal(2, result.DurationSeconds);
    }

    [Fact]
    public void ParseAndValidate_rejects_matroska_content_declared_as_webm()
    {
        const string output = """
            {
              "streams": [{ "codec_type": "video" }],
              "format": { "format_name": "matroska,webm" }
            }
            """;

        Assert.Throws<ValidationException>(() =>
            FfprobeVideoFileInspector.ParseAndValidate(
                output,
                "video/webm",
                "matroska"));
    }

    [Theory]
    [InlineData("video/webm", "isom")]
    [InlineData("video/mp4", "3gp5")]
    [InlineData("video/quicktime", "mjp2")]
    public void ParseAndValidate_rejects_mismatched_or_unsupported_containers(
        string declaredContentType,
        string majorBrand)
    {
        var output = IsoBaseMediaProbeOutput(majorBrand);

        Assert.Throws<ValidationException>(() =>
            FfprobeVideoFileInspector.ParseAndValidate(output, declaredContentType));
    }

    [Fact]
    public void ParseAndValidate_rejects_files_without_a_video_stream()
    {
        const string output = """
            {
              "streams": [{ "codec_type": "audio" }],
              "format": { "format_name": "mov,mp4", "tags": { "major_brand": "isom" } }
            }
            """;

        Assert.Throws<ValidationException>(() =>
            FfprobeVideoFileInspector.ParseAndValidate(output, "video/mp4"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    public void ParseAndValidate_rejects_malformed_or_incomplete_probe_output(string output)
    {
        Assert.Throws<ValidationException>(() =>
            FfprobeVideoFileInspector.ParseAndValidate(output, "video/mp4"));
    }

    private static string IsoBaseMediaProbeOutput(string majorBrand) => $$"""
        {
          "streams": [{ "codec_type": "video" }],
          "format": {
            "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
            "duration": "12.6",
            "tags": { "major_brand": "{{majorBrand}}" }
          }
        }
        """;
}
