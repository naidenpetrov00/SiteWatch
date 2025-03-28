using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;

namespace Application.Identity;

public abstract class IdentityResultModel
{
    public required Result Result { get; set; }
}

public class IdentityResultOnly : IdentityResultModel { }

public class IdentityResultWithToken : IdentityResultModel
{
    public required string Token { get; set; }
}

public class IdentityResultWithUser : IdentityResultModel
{
    public required ApplicationUser User { get; set; }
}

public static class IdentityResultErrors
{
    public static string UserNotFound => "User with this Email doesnt exist";
    public static string InvalidCredentials => "Invalid email or password";
}
