using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Planet.Library.Logging
{
    [ProviderAlias("InMemoryDebug")]
    public class InMemoryDebugProvider : ILoggerProvider, ISupportExternalScope
    {
        private bool _disposed = false;
        private IExternalScopeProvider _scopeProvider = null;
        private static ConcurrentQueue<LogMessage> _loggedMessagesQueue = new ConcurrentQueue<LogMessage>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private InMemoryDebugOptions _options;

        public InMemoryDebugProvider() // Default options
        {
            _options = new InMemoryDebugOptions();

            InitPeriodicCleanup();
        }

        public InMemoryDebugProvider(IOptionsMonitor<InMemoryDebugOptions> providerOptions) // Default options
        {
            _options = providerOptions.CurrentValue;
            providerOptions.OnChange(_ => {
                _options = _;
                InitPeriodicCleanup();
            });

            InitPeriodicCleanup();
        }

        private void InitPeriodicCleanup()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            Action cleanupAction = null;
            cleanupAction = () =>
            {
                var cts = _cts; // It mutates, so preserve original
                var cleanupBefore = DateTime.UtcNow;
                // Wait
                cts.Token.WaitHandle.WaitOne(_options.LogMessageLifetime);
                // If not cancelled
                if (!cts.IsCancellationRequested)
                {
                    // Clean
                    do
                    {
                        LogMessage _;
                        if (!_loggedMessagesQueue.TryPeek(out _) || _.Timestamp >= cleanupBefore)
                        {
                            break;
                        }
                        _loggedMessagesQueue.TryDequeue(out _);
                    } while (true);

                    // Rinse and repeat
                    Task.Run(cleanupAction, cts.Token);
                }
            };

            Task.Run(cleanupAction, _cts.Token);
        }

        public ILogger CreateLogger(string category)
        {
            return new InMemoryDebugLogger(this, category);
        }

        internal void DoActualLogging(LogMessage logMessage)
        {
            _loggedMessagesQueue.Enqueue(logMessage);
        }

        public static IEnumerable<LogMessage> FindRecentLogMessages(IEnumerable<LogLevel> logLevels, string logMessageRegex, string exceptionMessageRegex = null, DateTime? notOlderThan = null)
        {
            var logMessages = _loggedMessagesQueue.ToArray()
                .Where(_ => logLevels == null || logLevels.Any(__ => __ == _.LogLevel))
                .Where(_ => Regex.Match(_.Message, logMessageRegex).Success);
            if (exceptionMessageRegex != null)
            {
                logMessages = logMessages.Where(_ => Regex.Match(_.Exception.GetBaseException().Message, exceptionMessageRegex).Success);
            }
            if (notOlderThan != null)
            {
                logMessages = logMessages.Where(_ => _.Timestamp >= notOlderThan);
            }

            return logMessages.Reverse();
        }

        public static IEnumerable<LogMessage> FindRecentLogMessages(string logMessageRegex, string exceptionMessageRegex = null, DateTime? notOlderThan = null)
        {
            return FindRecentLogMessages(null, logMessageRegex, exceptionMessageRegex, notOlderThan);
        }

        public static IEnumerable<LogMessage> GetAllNonExpiredLogMessages(int lastN = int.MaxValue)
        {
            var logMessages = _loggedMessagesQueue.ToArray();

            if (logMessages.Length > lastN)
            {
                return logMessages.Skip(logMessages.Length - lastN).Take(lastN).Reverse();
            }

            return logMessages.Reverse();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _cts.Cancel();

            if (disposing)
            {
                // Nothing unmanaged
            }

            _disposed = true;
        }

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (_scopeProvider == null)
                    _scopeProvider = new LoggerExternalScopeProvider();
                return _scopeProvider;
            }
        }
    }
}
