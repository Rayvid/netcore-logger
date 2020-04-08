using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Planet.Library.Logging
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// This is the "normal" way to add provider to the list, some DI magic, but in essence it just registers another instance for ILoggerProvider
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddInMemoryDebug(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, InMemoryDebugProvider>());
            LoggerProviderOptions.RegisterProviderOptions<InMemoryDebugOptions, InMemoryDebugProvider>(builder.Services);
            return builder;
        }

        /// <summary>
        /// Test/debug scenarios mostly
        /// </summary>
        public static ILoggerFactory AddInMemoryDebug(this ILoggerFactory factory)
        {
            factory.AddProvider(new InMemoryDebugProvider());
            return factory;
        }

        public class InMemoryDebugLogger<T> : InMemoryDebugLogger, ILogger<T>
        {
            public InMemoryDebugLogger(InMemoryDebugProvider provider) : base(provider, typeof(T))
            {
            }
        }

        private static ConcurrentDictionary<object, int> _sampling = new ConcurrentDictionary<object, int>(); // Shared state for sampling
        /// <summary>
        /// For scenarios you need to log rarely, but extensively, or it can be sampled at some rate
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="actionToPerformOnLogger">Actual action we want to get sampled</param>
        /// <param name="sampleRateInPercents">What sample rate you want to get (if its > 0, first hit will always be logged, afterwards it will be throttled</param>
        public static void Sampled<T>(this ILogger<T> logger, Action<ILogger<T>> actionToPerformOnLogger, int sampleRateInPercents)
        {
            Sampled(logger, actionToPerformOnLogger, sampleRateInPercents);
        }

        /// <summary>
        /// For scenarios you need to log rarely, but extensively, or it can be sampled at some rate, still full info goes into trace if you need
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="actionToPerformOnLogger">Actual action we want to get sampled</param>
        /// <param name="sampleRateInPercents">What sample rate you want to get (if its > 0, first hit will always be logged, afterwards it will be throttled</param>
        public static void Sampled(this ILogger logger, string message, Action<ILogger, string> actionToPerformOnLogger, int sampleRateInPercents)
        {
            logger.LogTrace(message); // Everything goes to trace level

            if (sampleRateInPercents != 100 && sampleRateInPercents != 0)
            {
                var sample = _sampling.AddOrUpdate(logger, 1, (_caller, old) => old + 1);

                if (sample <= sampleRateInPercents)
                {
                    actionToPerformOnLogger(logger, $"[Sampled at {sampleRateInPercents}%] {message}"); // Add special prefix its sampled
                }

                if (sample == 100)
                {
                    _sampling.AddOrUpdate(logger, 0, (_caller, old) => 0);
                }
            }
        }
    }
}
