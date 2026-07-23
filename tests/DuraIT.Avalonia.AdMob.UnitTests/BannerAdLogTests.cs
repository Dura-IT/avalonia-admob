using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

[TestFixture]
[TestOf(typeof(BannerAdLog))]
public class BannerAdLogTests
{
    [Test]
    public void Loaded_LogsDebugWithAdUnit()
    {
        var logger = new CapturingLogger();

        BannerAdLog.Loaded(logger, "unit-loaded");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain("unit-loaded");
    }

    [Test]
    public void FailedToLoad_LogsWarningWithAdUnitAndErrorDetail()
    {
        var logger = new CapturingLogger();

        BannerAdLog.FailedToLoad(logger, "unit-failed", 3, "No fill");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
        logger
            .Entries[0]
            .Message.Should()
            .Contain("unit-failed")
            .And.Contain("3")
            .And.Contain("No fill");
    }

    [Test]
    public void BlockedByConsent_LogsInformation()
    {
        var logger = new CapturingLogger();

        BannerAdLog.BlockedByConsent(logger);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
    }
}

// Records what was logged so the log assertions don't need to mock ILogger's generic Log method.
file sealed class CapturingLogger : ILogger
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) => Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
}

file sealed record LogEntry(LogLevel Level, string Message);
