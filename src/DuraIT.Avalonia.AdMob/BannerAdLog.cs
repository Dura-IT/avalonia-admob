using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
///     Pre-compiled log messages for <see cref="Platforms.BannerAd" /> load outcomes, so a failed or consent-blocked
///     banner surfaces in the consuming app's log instead of silently staying blank.
/// </summary>
internal static class BannerAdLog
{
    private static readonly Action<ILogger, string, Exception?> _loaded =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(300, nameof(Loaded)),
            "AdMob banner loaded (ad unit {AdUnitId})"
        );

    private static readonly Action<ILogger, string, long, string, Exception?> _failedToLoad =
        LoggerMessage.Define<string, long, string>(
            LogLevel.Warning,
            new EventId(301, nameof(FailedToLoad)),
            "AdMob banner failed to load (ad unit {AdUnitId}): [{ErrorCode}] {ErrorMessage}"
        );

    private static readonly Action<ILogger, Exception?> _blockedByConsent = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(302, nameof(BlockedByConsent)),
        "AdMob banner not loaded: user consent was not obtained, so ads cannot be requested"
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
