namespace Application.Assets;

public sealed class AssetInvalidTransitionException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class AssetConcurrencyConflictException(string message)
    : Exception(message);
