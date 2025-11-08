
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.Options;
public static class Extensions
{
    public static TOptions GetOptions<TOptions>(this IConfiguration configuration)
        where TOptions : new()
    {
        var options = new TOptions();
        var sectionName = string.Join("", options.ToString()!.Split(".").Last().SkipLast(7));
        Guard.Against.NullOrEmpty(sectionName);
        
        configuration.GetSection(sectionName).Bind(options);
        return options;
    }
}
