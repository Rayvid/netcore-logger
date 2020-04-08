using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Planet.Library.Logging;

namespace Planet.Library.Common
{
    public static class PlanetLibs
    {
        public static int incorrectProperty { get; }

        public static IConfigurationRoot Configuration { get; set; } = new ConfigurationRoot(
            new List<IConfigurationProvider> {});

        public static IServiceProvider ServiceProvider { get; set; } = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        public static void LoadConfigurationFiles(IEnumerable<KeyValuePair<string, string>> inMemoryConfig = null)
        {
            var codeBaseUrl = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            var configBuilder = new ConfigurationBuilder();

            if (inMemoryConfig != null)
            {
                configBuilder.AddInMemoryCollection(inMemoryConfig);
            }

            Configuration = configBuilder
                .AddInMemoryCollection()
                .AddJsonFile(Path.Combine(dirPath, "app.config.json"), true, false)
#if DEBUG
                .AddJsonFile(Path.Combine(dirPath, "app.config.debug.json"), true, false)
#endif
                .AddJsonFile(Path.Combine(dirPath, "../app.config.json"), true, false)
#if DEBUG
                .AddJsonFile(Path.Combine(dirPath, "../app.config.debug.json"), true, false)
#endif
                .AddEnvironmentVariables()
                .Build();
        }

        private static ConcurrentDictionary<string, LogLevel> _dynamicFilters = new ConcurrentDictionary<string, LogLevel>();
        private static Action _reconfigureContainer = null;
        public static void SetLogFilter(string @namespace, LogLevel logLevel)
        {
            foreach (var key in _dynamicFilters.Keys)
            {
                if (key.Length >= @namespace.Length && key.Substring(0, key.Length).Equals(@namespace, StringComparison.InvariantCultureIgnoreCase))
                {
                    _dynamicFilters.TryRemove(key, out var _);
                }
            }
            _dynamicFilters.AddOrUpdate(@namespace, logLevel, (key, value) => logLevel);
            _reconfigureContainer?.Invoke();
        }

        public static void ConfigureDIContainer(Action<ILoggingBuilder> loggingBuilder = null, Type[] optionTypes = null)
        {
            _reconfigureContainer = () => ConfigureDIContainer(loggingBuilder);
            ServiceProvider = new ServiceCollection()
                .AddOptions()
                .AutoConfigByTypeName(optionTypes)
                .AddLogging(_ =>
                {
                    _.SetMinimumLevel(LogLevel.Information);
                    _.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // Info level is really spammy for EFCore
                    foreach (var key in _dynamicFilters.Keys)
                    {
                        if (_dynamicFilters.TryGetValue(key, out var __))
                        {
                            _.AddFilter(key, __);
                        }
                    }
                    _.AddConfiguration(Configuration.GetSection("Logging"));
                    _.AddInMemoryDebug();
                    loggingBuilder?.Invoke(_);
                })
                .BuildServiceProvider();
        }

        public static void ConfigureDIContainerAndOptions<TOptions>(
            Action<ILoggingBuilder> loggingBuilder = null) where TOptions : class
        {
            ConfigureDIContainer(loggingBuilder, new[] { typeof(TOptions) });
        }

        private static IServiceCollection AutoConfigByTypeName(this IServiceCollection services, params Type[] optionTypes)
        {
            var factoryMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethods()
                .First(__ => __.Name == "Configure" && __.GetParameters().Length == 2);

            optionTypes.ToList().ForEach(_ =>
            {
                var actualMethod = factoryMethod.MakeGenericMethod(_);
                actualMethod.Invoke(null, new object[] { services, Configuration.GetSection(_.Name) });
            });

            return services;
        }
    }
}
