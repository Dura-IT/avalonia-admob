using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Pre-compiled log messages for full-screen ad (interstitial, rewarded, app open) presentation
    /// outcomes, so a shown, failed-to-show, dismissed, or reward-earning ad surfaces in the consuming
    /// app's log instead of passing silently. Load-phase outcomes are shared with every format and live in
    /// <see cref="AdLoadLog" />; the <c>AdFormat</c> field distinguishes formats that share this log.
    /// </summary>
    internal static class FullScreenAdLog
    {
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

        private static readonly Action<
            ILogger,
            string,
            string,
            string,
            decimal,
            Exception?
        > _rewardEarned = LoggerMessage.Define<string, string, string, decimal>(
            LogLevel.Information,
            new EventId(316, nameof(RewardEarned)),
            "AdMob {AdFormat} reward earned (ad unit {AdUnitId}): {RewardAmount} {RewardType}"
        );

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

        public static void RewardEarned(
            ILogger logger,
            string adFormat,
            string adUnitId,
            string rewardType,
            decimal rewardAmount
        ) => _rewardEarned(logger, adFormat, adUnitId, rewardType, rewardAmount, null);
    }
}
