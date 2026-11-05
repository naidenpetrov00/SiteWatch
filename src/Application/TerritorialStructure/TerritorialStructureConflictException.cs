namespace Application.TerritorialStructure;

public sealed class TerritorialStructureConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
