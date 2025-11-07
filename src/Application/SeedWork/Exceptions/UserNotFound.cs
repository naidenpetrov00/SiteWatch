namespace Application.SeedWork.Exceptions;

class UserNotFound : Exception
{
    public UserNotFound(string message)
        : base(message) { }
}
