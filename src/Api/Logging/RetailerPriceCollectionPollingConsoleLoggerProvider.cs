using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Api.Logging;

internal static class RetailerPriceCollectionPollingConsoleLoggingExtensions
{
    private const string DatabaseCommandCategory = "Microsoft.EntityFrameworkCore.Database.Command";
    private const int CommandExecutedEventId = 20101;
    private const string LeadershipCheckTag =
        "SiteWatch.RetailerPriceCollection.Polling.LeadershipCheck";
    private const string NextItemProbeTag =
        "SiteWatch.RetailerPriceCollection.Polling.NextItemProbe";

    public static IServiceCollection FilterRetailerPriceCollectionPollingFromConsole(
        this IServiceCollection services)
    {
        var consoleProvider = services.LastOrDefault(descriptor =>
            descriptor.ServiceType == typeof(ILoggerProvider)
            && descriptor.ImplementationType == typeof(ConsoleLoggerProvider));
        if (consoleProvider is null)
        {
            return services;
        }

        services.Remove(consoleProvider);
        services.AddSingleton<ConsoleLoggerProvider>();
        services.AddSingleton<ILoggerProvider, RetailerPriceCollectionPollingConsoleLoggerProvider>();
        return services;
    }

    private sealed class RetailerPriceCollectionPollingConsoleLoggerProvider(
        ConsoleLoggerProvider inner) : ILoggerProvider, ISupportExternalScope
    {
        public ILogger CreateLogger(string categoryName) =>
            new RetailerPriceCollectionPollingConsoleLogger(inner.CreateLogger(categoryName), categoryName);

        public void Dispose() => inner.Dispose();

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
            inner.SetScopeProvider(scopeProvider);

        private sealed class RetailerPriceCollectionPollingConsoleLogger(
            ILogger inner,
            string categoryName) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => inner.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (categoryName == DatabaseCommandCategory
                    && eventId.Id == CommandExecutedEventId)
                {
                    var message = formatter(state, exception);
                    if (message.Contains(LeadershipCheckTag, StringComparison.Ordinal)
                        || message.Contains(NextItemProbeTag, StringComparison.Ordinal))
                    {
                        return;
                    }
                }

                inner.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
