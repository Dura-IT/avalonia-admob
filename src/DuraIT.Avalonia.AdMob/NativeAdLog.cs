using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Pre-compiled log messages for <see cref="Platforms.NativeAd" /> load outcomes, so a failed or
/// consent-blocked native ad surfaces in the consuming app's log instead of silently staying blank.
/// </summary>
internal static class NativeAdLog
{
    private static readonly Action<ILogger, string, Exception?> _loaded =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(320, nameof(Loaded)),
            "AdMob native ad loaded (ad unit {AdUnitId})"
        );

    private static readonly Action<ILogger, string, long, string, Exception?> _failedToLoad =
        LoggerMessage.Define<string, long, string>(
            LogLevel.Warning,
            new EventId(321, nameof(FailedToLoad)),
            "AdMob native ad failed to load (ad unit {AdUnitId}): [{ErrorCode}] {ErrorMessage}"
        );

    private static readonly Action<ILogger, Exception?> _blockedByConsent = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(322, nameof(BlockedByConsent)),
        "AdMob native ad not loaded: user consent was not obtained, so ads cannot be requested"
    );

    public static void Loaded(ILogger logger, string adUnitId) => _loaded(logger, adUnitId, null);

    public static void FailedToLoad(
        ILogger logger,
        string adUnitId,
        long errorCode,
        string errorMessage
    ) => _failedToLoad(logger, adUnitId, errorCode, errorMessage, null);

    public static void BlockedByConsent(ILogger logger) => _blockedByConsent(logger, null);
}
