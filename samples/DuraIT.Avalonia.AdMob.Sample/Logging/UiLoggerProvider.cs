using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Sample.Logging
{
    /// <summary>
    /// An <see cref="ILoggerProvider" /> that forwards every log message into a <see cref="UiLogSink" />
    /// so the sample can show the library's ad-load and presentation outcomes on screen.
    /// </summary>
    public sealed class UiLoggerProvider : ILoggerProvider
    {
        private readonly UiLogSink _sink;

        public UiLoggerProvider(UiLogSink sink) => _sink = sink;

        public ILogger CreateLogger(string categoryName) => new UiLogger(_sink, categoryName);

        public void Dispose()
        {
            // Nothing to release; the sink outlives the provider.
        }

        private sealed class UiLogger : ILogger
        {
            private readonly UiLogSink _sink;
            private readonly string _category;

            public UiLogger(UiLogSink sink, string category)
            {
                _sink = sink;
                _category = category[(category.LastIndexOf('.') + 1)..];
            }

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter
            )
            {
                ArgumentNullException.ThrowIfNull(formatter);
                _sink.Append(
                    string.Create(
                        CultureInfo.InvariantCulture,
                        $"[{logLevel}] {_category}: {formatter(state, exception)}"
                    )
                );
            }
        }
    }
}
