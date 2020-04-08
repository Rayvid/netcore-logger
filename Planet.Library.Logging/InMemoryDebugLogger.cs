using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Planet.Library.Logging
{
    public class InMemoryDebugOptions
    {
        public int LogMessageLifetime { get; set; } = 15 * 60 * 1000; // 15 mins expressed in milliseconds
    }

    public class InMemoryDebugLogger : ILogger
    {
        public InMemoryDebugProvider _provider { get; private set; }
        public string _category { get; private set; }

        public InMemoryDebugLogger(InMemoryDebugProvider provider, string category)
        {
            _provider = provider;
            _category = category;
        }

        public InMemoryDebugLogger(InMemoryDebugProvider provider, Type category)
        {
            _provider = provider;
            _category = category.FullName;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return _provider.ScopeProvider.Push(state);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true; // Framework doesnt really use this and we relay on framework to do the filtering
        }

        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var scopes = new List<object>();
            _provider.ScopeProvider.ForEachScope((_, __) => scopes.Add(_), state);
            var logMessage = new LogMessage(_category, logLevel, state.ToString(), exception, scopes);

            _provider.DoActualLogging(logMessage);
        }
    }
}
