using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Pre-compiled log messages for full-screen ad (interstitial, rewarded, app open) load and
/// presentation outcomes, so a failed, consent-blocked, or dismissed ad surfaces in the consuming
/// app's log instead of passing silently. The <c>AdFormat</c> field distinguishes formats that share
/// this log.
/// </summary>
internal static class FullScreenAdLog
{
    private static readonly Action<ILogger, string, string, Exception?> _loaded =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(310, nameof(Loaded)),
            "AdMob {AdFormat} loaded (ad unit {AdUnitId})"
        );

    private static readonly Action<
        ILogger,
        string,
        string,
        long,
        string,
        Exception?
    > _failedToLoad = LoggerMessage.Define<string, string, long, string>(
        LogLevel.Warning,
        new EventId(311, nameof(FailedToLoad)),
        "AdMob {AdFormat} failed to load (ad unit {AdUnitId}): [{ErrorCode}] {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> _blockedByConsent =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(312, nameof(BlockedByConsent)),
            "AdMob {AdFormat} not loaded: user consent was not obtained, so ads cannot be requested"
        );

    private static readonly Action<ILogger, string, string, Exception?> _showed =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(313, nameof(Showed)),
            "AdMob {AdFormat} shown (ad unit {AdUnitId})"
        );

    private static readonly Action<
        ILogger,
        string,
        string,
        long,
        string,
        Exception?
    > _failedToShow = LoggerMessage.Define<string, string, long, string>(
        LogLevel.Warning,
        new EventId(314, nameof(FailedToShow)),
        "AdMob {AdFormat} failed to show (ad unit {AdUnitId}): [{ErrorCode}] {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, string, Exception?> _dismissed =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(315, nameof(Dismissed)),
            "AdMob {AdFormat} dismissed (ad unit {AdUnitId})"
        );

    public static void Loaded(ILogger logger, string adFormat, string adUnitId) =>
        _loaded(logger, adFormat, adUnitId, null);

    public static void FailedToLoad(
        ILogger logger,
        string adFormat,
        string adUnitId,
        long errorCode,
        string errorMessage
    ) => _failedToLoad(logger, adFormat, adUnitId, errorCode, errorMessage, null);

    public static void BlockedByConsent(ILogger logger, string adFormat) =>
        _blockedByConsent(logger, adFormat, null);

    public static void Showed(ILogger logger, string adFormat, string adUnitId) =>
        _showed(logger, adFormat, adUnitId, null);

    public static void FailedToShow(
        ILogger logger,
        string adFormat,
        string adUnitId,
        long errorCode,
        string errorMessage
    ) => _failedToShow(logger, adFormat, adUnitId, errorCode, errorMessage, null);

    public static void Dismissed(ILogger logger, string adFormat, string adUnitId) =>
        _dismissed(logger, adFormat, adUnitId, null);
}
