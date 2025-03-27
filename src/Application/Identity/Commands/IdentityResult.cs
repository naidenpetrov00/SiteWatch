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
