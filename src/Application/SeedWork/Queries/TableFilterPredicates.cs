using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Application.SeedWork.Queries;

public static class TableFilterPredicates
{
    public static Expression<Func<TEntity, bool>>? TextContains<TEntity>(
        Expression<Func<TEntity, string>> selector,
        string? rawValue
    )
    {
        var normalizedValue = Normalize(rawValue);
        if (normalizedValue.Length == 0)
        {
            return null;
        }

        var body = selector.Body;
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var loweredBody = Expression.Call(body, toLowerMethod);
        var containsBody = Expression.Call(
            loweredBody,
            containsMethod,
            Expression.Constant(normalizedValue)
        );

        return Expression.Lambda<Func<TEntity, bool>>(containsBody, selector.Parameters);
    }

    public static Expression<Func<TEntity, bool>>? BooleanEquals<TEntity>(
        Expression<Func<TEntity, bool>> selector,
        bool? value
    )
    {
        if (!value.HasValue)
        {
            return null;
        }

        var body = Expression.Equal(selector.Body, Expression.Constant(value.Value));

        return Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters);
    }

    public static Expression<Func<TEntity, bool>>? DateTimeOffsetSearch<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> selector,
        string? rawValue
    )
    {
        var normalizedValue = Normalize(rawValue);
        if (normalizedValue.Length == 0)
        {
            return null;
        }

        if (Regex.IsMatch(normalizedValue, @"^\d{4}$", RegexOptions.CultureInvariant))
        {
            return BuildYearPredicate(
                selector,
                int.Parse(normalizedValue, CultureInfo.InvariantCulture)
            );
        }

        if (
            Regex.IsMatch(
                normalizedValue,
                @"^\d{4}[-/]\d{1,2}$",
                RegexOptions.CultureInvariant
            )
            && TryParseYearMonth(normalizedValue, out var year, out var month)
        )
        {
            return BuildYearMonthPredicate(selector, year, month);
        }

        if (
            Regex.IsMatch(
                normalizedValue,
                @"^\d{4}[-/]\d{1,2}[-/]\d{1,2}$",
                RegexOptions.CultureInvariant
            )
            && DateOnly.TryParse(
                normalizedValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var dateOnly
            )
        )
        {
            return BuildDateEqualityPredicate(selector, dateOnly);
        }

        if (
            DateTimeOffset.TryParse(
                normalizedValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out var parsedDateTime
            )
        )
        {
            return BuildDateEqualityPredicate(selector, DateOnly.FromDateTime(parsedDateTime.Date));
        }

        return null;
    }

    private static Expression<Func<TEntity, bool>> BuildYearPredicate<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> selector,
        int year
    )
    {
        var dateTimeOffsetValue = Expression.Property(selector.Body, nameof(Nullable<DateTimeOffset>.Value));
        var yearProperty = Expression.Property(dateTimeOffsetValue, nameof(DateTimeOffset.Year));
        return BuildNullableDatePredicate(
            selector,
            Expression.Equal(yearProperty, Expression.Constant(year))
        );
    }

    private static Expression<Func<TEntity, bool>> BuildYearMonthPredicate<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> selector,
        int year,
        int month
    )
    {
        var dateTimeOffsetValue = Expression.Property(selector.Body, nameof(Nullable<DateTimeOffset>.Value));
        var yearProperty = Expression.Property(dateTimeOffsetValue, nameof(DateTimeOffset.Year));
        var monthProperty = Expression.Property(dateTimeOffsetValue, nameof(DateTimeOffset.Month));

        var yearMatch = Expression.Equal(yearProperty, Expression.Constant(year));
        var monthMatch = Expression.Equal(monthProperty, Expression.Constant(month));

        return BuildNullableDatePredicate(selector, Expression.AndAlso(yearMatch, monthMatch));
    }

    private static Expression<Func<TEntity, bool>> BuildDateEqualityPredicate<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> selector,
        DateOnly dateOnly
    )
    {
        var dateTimeOffsetValue = Expression.Property(selector.Body, nameof(Nullable<DateTimeOffset>.Value));
        var dateProperty = Expression.Property(dateTimeOffsetValue, nameof(DateTimeOffset.Date));
        var dateOnlyExpression = Expression.Constant(dateOnly.ToDateTime(TimeOnly.MinValue));
        var equality = Expression.Equal(dateProperty, dateOnlyExpression);

        return BuildNullableDatePredicate(selector, equality);
    }

    private static Expression<Func<TEntity, bool>> BuildNullableDatePredicate<TEntity>(
        Expression<Func<TEntity, DateTimeOffset?>> selector,
        Expression dateBody
    )
    {
        var hasValue = Expression.Property(selector.Body, nameof(Nullable<DateTimeOffset>.HasValue));
        var body = Expression.AndAlso(hasValue, dateBody);

        return Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters);
    }

    private static bool TryParseYearMonth(string value, out int year, out int month)
    {
        year = 0;
        month = 0;

        var parts = value.Split(['-', '/'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        return int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out year)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out month)
            && month is >= 1 and <= 12;
    }

    private static string Normalize(string? value) => value?.Trim().ToLowerInvariant() ?? string.Empty;
}
