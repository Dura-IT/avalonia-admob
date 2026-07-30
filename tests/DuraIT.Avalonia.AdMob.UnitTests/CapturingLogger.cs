using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.UnitTests;

// Records what was logged so log assertions don't need to mock ILogger's generic Log method.
internal sealed class CapturingLogger : ILogger
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

internal sealed record LogEntry(LogLevel Level, string Message);
