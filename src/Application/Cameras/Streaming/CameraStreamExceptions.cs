namespace Application.Cameras.Streaming;

public sealed class CameraStreamConfigurationException : Exception
{
    public CameraStreamConfigurationException()
        : base("The camera does not have a supported live-stream configuration.")
    {
    }
}

public sealed class MediaMtxGatewayUnavailableException : Exception
{
    public MediaMtxGatewayUnavailableException()
        : base("The live-stream gateway is unavailable.")
    {
    }

    public MediaMtxGatewayUnavailableException(Exception innerException)
        : base("The live-stream gateway is unavailable.", innerException)
    {
    }
}
