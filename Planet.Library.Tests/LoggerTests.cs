using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Xunit;
using System.Linq;
using System.IO;
using Planet.Library.Logging;

namespace Mmr.UnitTests.Logging
{
    public class LoggerTests
    {
        [Fact]
        public void GivenNothing_LoggerFactoryCanBeConfiguredUsingInMemoryDebugProvider_AndLoggerLogs()
        {
            // Ensure other tests logs doesnt interfere
            Thread.Sleep(100);
            var testStartTime = DateTime.UtcNow;
            //

            var logger = LoggerFactory.Create((builder) => { }).AddInMemoryDebug().CreateLogger(typeof(LoggerTests));
            logger.LogError(new IndexOutOfRangeException("Lew"), "Mew");
            Assert.Single(InMemoryDebugProvider.FindRecentLogMessages("Mew", "Lew", testStartTime));
        }

        [Fact]
        public void GivenNothing_LoggerFactoryCanBeConfiguredUsingInMemoryDebugProvider_FindRecentRespectsTimestamp()
        {
            var logger = LoggerFactory.Create((builder) => { }).AddInMemoryDebug().CreateLogger(typeof(LoggerTests));
            logger.LogError(new IndexOutOfRangeException("Lew"), "Mew");
            Assert.Empty(InMemoryDebugProvider.FindRecentLogMessages("Mew", "Lew", DateTime.UtcNow + TimeSpan.FromMilliseconds(1)));
        }

        [Fact]
        public void GivenServicesProvider_DICanBeConfiguredUsingInMemoryDebugProvider_AndLoggerLogs()
        {
            // Ensure other tests logs doesnt interfere
            Thread.Sleep(100);
            var testStartTime = DateTime.UtcNow;
            //

            var internalServiceCollection = new ServiceCollection();
            var serviceProvider = internalServiceCollection
                .AddLogging(_ =>
                {
                    _.AddInMemoryDebug();
                }).BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<LoggerTests>>();
            logger.LogError(new IndexOutOfRangeException("Lew"), "Mew");
            Assert.Single(InMemoryDebugProvider.FindRecentLogMessages("Mew", "Lew", testStartTime));
        }

        [Fact] // TODO make manual because takes long and infra dependant?
        public void GivenServicesProvider_OptionsCanBeChangedForInMemoryDebugProvider_EnsureNewOptionsAreEffective()
        {
            var testStartTime = DateTime.UtcNow;

            File.WriteAllText("dummy.json", @"{""Logging"": { ""InMemoryDebug"": { ""LogMessageLifetime"": 10000 } } }");
            var mutatingConfig = new ConfigurationBuilder()
                .AddJsonFile("dummy.json", optional: false, reloadOnChange: true)
                .Build();
            var config = new ConfigurationBuilder()
                .AddConfiguration(mutatingConfig)
                .Build();

            var internalServiceCollection = new ServiceCollection();
            var serviceProvider = internalServiceCollection
                .AddLogging(_ =>
                {
                    _.AddConfiguration(config.GetSection("Logging"));
                    _.AddInMemoryDebug();
                }).BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<LoggerTests>>();

            // Assert first config doesnt expire message
            logger.LogError(new IndexOutOfRangeException("Lew"), "Mew");
            Thread.Sleep(1000);
            Assert.True(InMemoryDebugProvider.FindRecentLogMessages("Mew", "Lew", testStartTime).Count() > 0);

            // Mutate config
            File.WriteAllText("dummy.json", @"{""Logging"": { ""InMemoryDebug"": { ""LogMessageLifetime"": 500 } } }");
            Thread.Sleep(5000); // Takes time to pickup, bit infrastructure dependant

            // Assert mutated config does expire message
            logger.LogError(new IndexOutOfRangeException("Lew"), "Mew");
            Thread.Sleep(1000);
            Assert.Empty(InMemoryDebugProvider.FindRecentLogMessages("Mew", "Lew", testStartTime));
        }
    }
}
