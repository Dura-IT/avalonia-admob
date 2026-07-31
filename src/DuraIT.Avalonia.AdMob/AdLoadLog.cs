using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Pre-compiled log messages for ad load outcomes shared by every ad format (banner, native, and the
/// full-screen formats), so a failed or consent-blocked ad surfaces in the consuming app's log instead
/// of silently staying blank. The <c>AdFormat</c> field distinguishes the format that produced the log.
/// </summary>
internal static class AdLoadLog
{
    private static readonly Action<ILogger, string, string, Exception?> _loaded =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(300, nameof(Loaded)),
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
        new EventId(301, nameof(FailedToLoad)),
        "AdMob {AdFormat} failed to load (ad unit {AdUnitId}): [{ErrorCode}] {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> _blockedByConsent =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(302, nameof(BlockedByConsent)),
            "AdMob {AdFormat} not loaded: user consent was not obtained, so ads cannot be requested"
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
}
