namespace Application.Persons;

public sealed class PersonConflictException(
    string message,
    Exception? innerException = null) : Exception(message, innerException);
