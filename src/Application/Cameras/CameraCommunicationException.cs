namespace Application.Cameras;

public sealed class CameraCommunicationException : Exception
{
    private const string SafeMessage = "Unable to communicate with the camera.";

    public CameraCommunicationException()
        : base(SafeMessage)
    {
    }

    public CameraCommunicationException(Exception innerException)
        : base(SafeMessage, innerException)
    {
    }
}
