using Domain.ValueObjects;
using FluentValidation;

namespace Application.Sites.Commands;

internal static class MediaCategoryValidation
{
    public static void Validate<T>(string[]? values, ValidationContext<T> context)
    {
        if (values is null)
        {
            return;
        }

        var normalizedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            var category = SiteMediaPolicy.NormalizeCategory(value);
            if (category is null)
            {
                continue;
            }

            if (category.Length > SiteMediaPolicy.MaxCategoryLength)
            {
                context.AddFailure(
                    $"Media categories cannot exceed {SiteMediaPolicy.MaxCategoryLength} characters.");
                return;
            }

            if (string.Equals(category, SiteMediaPolicy.AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                context.AddFailure(
                    $"{SiteMediaPolicy.AllFilter} is reserved for the media filter that shows every item.");
                return;
            }

            normalizedCategories.Add(category);
        }

        normalizedCategories.Add(SiteMediaPolicy.OtherCategory);
        if (normalizedCategories.Count > SiteMediaPolicy.MaxCategoryCount)
        {
            context.AddFailure(
                $"A media policy cannot contain more than {SiteMediaPolicy.MaxCategoryCount} categories.");
        }
    }

    public static int GetEffectiveCount(IEnumerable<string> values)
    {
        var normalizedCategories = values
            .Select(SiteMediaPolicy.NormalizeCategory)
            .Where(category => category is not null)
            .Select(category => category!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        normalizedCategories.Add(SiteMediaPolicy.OtherCategory);
        return normalizedCategories.Count;
    }
}
