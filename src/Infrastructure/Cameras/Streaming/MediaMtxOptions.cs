namespace Infrastructure.Cameras.Streaming;

public sealed class MediaMtxOptions
{
    public const string SectionName = "MediaMtx";

    public string ControlApiBaseUrl { get; set; } = string.Empty;
    public string PublicWhepBaseUrl { get; set; } = string.Empty;

    public static bool IsValidOrigin(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            return false;
        }

        return uri.AbsolutePath is "" or "/";
    }
}
