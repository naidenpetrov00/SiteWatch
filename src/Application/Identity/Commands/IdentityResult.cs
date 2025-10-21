using Application.Identity.Queries.Users;
using Application.SeedWork.Models;

namespace Application.Identity.Commands;

public abstract class IdentityResultModel
{
    public required Result Result { get; set; }
}

public class IdentityResultOnly : IdentityResultModel { }

public class IdentityResultWithToken : IdentityResultModel
{
    public required string Token { get; set; }
}

public class IdentityResultWithTokenEmail : IdentityResultWithToken
{
    public required string Email { get; set; }
}

public class IdentityResultWithUser : IdentityResultModel
{
    public required UserInfoDto User { get; set; }
}

public static class IdentityResultErrors
{
    public static string UserNotFound => "User with this Email doesnt exist";
    public static string InvalidCredentials => "Invalid email or password";
    public static string EmailAlreadyRegistered => "Email already registered";
    public static string TokenNotValid => "Invlaid email verification token";
}
