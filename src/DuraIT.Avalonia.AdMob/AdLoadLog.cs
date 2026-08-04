using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Pre-compiled log messages for ad load and configuration outcomes shared by every ad format (banner,
    /// native, and the full-screen formats), so a failed, consent-blocked, or misconfigured ad surfaces in
    /// the consuming app's log instead of silently staying blank. The <c>AdFormat</c> field distinguishes
    /// the format that produced the log.
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

        private static readonly Action<ILogger, string, string, Exception?> _missingAdUnitId =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(303, nameof(MissingAdUnitId)),
                "AdMob {AdFormat} not loaded: no ad unit id is configured for {Platform} and test ads are disabled, so no ad was requested"
            );

        private static readonly Action<ILogger, string, Exception?> _missingAppId =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(304, nameof(MissingAppId)),
                "AdMob app id is missing from the {Platform} app manifest; the Google Mobile Ads SDK cannot serve ads until it is set"
            );

        private static readonly Action<ILogger, string, Exception?> _sampleAppId =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(305, nameof(SampleAppId)),
                "AdMob is using Google's sample app id on {Platform} while test ads are disabled; set your own app id in the manifest before shipping real ads"
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

        public static void MissingAdUnitId(ILogger logger, string adFormat, string platform) =>
            _missingAdUnitId(logger, adFormat, platform, null);

        public static void MissingAppId(ILogger logger, string platform) =>
            _missingAppId(logger, platform, null);

        public static void SampleAppId(ILogger logger, string platform) =>
            _sampleAppId(logger, platform, null);
    }
}
