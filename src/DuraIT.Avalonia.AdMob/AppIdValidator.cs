using System;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Evaluates the AdMob app id read from the platform manifest at startup and reports a warning when it
    /// is missing or still Google's sample id. The app id lives in the native manifest (Android
    /// <c>AndroidManifest.xml</c>, iOS <c>Info.plist</c>) — the SDK reads it there, not from
    /// <see cref="AdMobOptions" /> — so the library can only read it back and warn, not set it. A missing
    /// id stops the SDK initializing; shipping the sample id with real ads is a misconfiguration.
    /// </summary>
    internal static class AppIdValidator
    {
        /// <summary>
        /// Google's public sample AdMob app id for Android.
        /// </summary>
        internal const string SampleAndroidAppId = "ca-app-pub-3940256099942544~3347511713";

        /// <summary>
        /// Google's public sample AdMob app id for iOS.
        /// </summary>
        internal const string SampleIOSAppId = "ca-app-pub-3940256099942544~1458002511";

        internal enum AppIdStatus
        {
            Ok,
            Missing,
            Sample,
        }

        /// <summary>
        /// Classifies a manifest app id: <see cref="AppIdStatus.Missing" /> when absent, or
        /// <see cref="AppIdStatus.Sample" /> when it is Google's sample id while test ads are disabled
        /// (the sample id is expected and fine while <paramref name="useTestAds" /> is on).
        /// </summary>
        internal static AppIdStatus Evaluate(string? appId, bool useTestAds)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return AppIdStatus.Missing;
            }

            if (!useTestAds && IsSample(appId))
            {
                return AppIdStatus.Sample;
            }

            return AppIdStatus.Ok;
        }

        /// <summary>
        /// Logs a warning through <paramref name="logger" /> when the app id is misconfigured, and does
        /// nothing when it is fine. Called once per process from the platform initializer.
        /// </summary>
        internal static void Report(ILogger logger, string? appId, bool useTestAds, string platform)
        {
            switch (Evaluate(appId, useTestAds))
            {
                case AppIdStatus.Missing:
                    AdLoadLog.MissingAppId(logger, platform);
                    break;
                case AppIdStatus.Sample:
                    AdLoadLog.SampleAppId(logger, platform);
                    break;
                case AppIdStatus.Ok:
                default:
                    break;
            }
        }

        private static bool IsSample(string appId) =>
            appId.Equals(SampleAndroidAppId, StringComparison.OrdinalIgnoreCase)
            || appId.Equals(SampleIOSAppId, StringComparison.OrdinalIgnoreCase);
    }
}
