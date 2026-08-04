using System.Collections.Generic;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Configuration for the AdMob integration, supplied by the consuming application at startup through
    /// <see cref="ServiceCollectionExtensions.AddAdMob" /> (or a per-format registration). Besides the
    /// behavioural flags below, it holds the per-platform ad unit id for each format so an app can declare
    /// its ids once here instead of at every call site.
    /// </summary>
    public sealed class AdMobOptions
    {
        /// <summary>
        /// Gets or sets the banner ad unit id used when a <see cref="Platforms.BannerAd" /> does not set
        /// its own <see cref="Platforms.BannerAd.AdUnitId" />. Ignored while <see cref="UseTestAds" /> is
        /// enabled. Defaults to <see langword="null" />; a banner with neither a configured nor an explicit
        /// id logs an error and renders blank rather than requesting an ad.
        /// </summary>
        public AdUnitId? BannerAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets the native ad unit id used when a <see cref="Platforms.NativeAd" /> does not set its
        /// own <see cref="Platforms.NativeAd.AdUnitId" />. Ignored while <see cref="UseTestAds" /> is
        /// enabled. Defaults to <see langword="null" />; a native ad with neither a configured nor an
        /// explicit id logs an error and renders blank rather than requesting an ad.
        /// </summary>
        public AdUnitId? NativeAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets the interstitial ad unit id used when <see cref="IInterstitialAdService.LoadAsync" />
        /// is called without one. Ignored while <see cref="UseTestAds" /> is enabled. Defaults to
        /// <see langword="null" />.
        /// </summary>
        public AdUnitId? InterstitialAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets the rewarded ad unit id used when <see cref="IRewardedAdService.LoadAsync" /> is
        /// called without one. Ignored while <see cref="UseTestAds" /> is enabled. Defaults to
        /// <see langword="null" />.
        /// </summary>
        public AdUnitId? RewardedAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets the rewarded interstitial ad unit id used when
        /// <see cref="IRewardedInterstitialAdService.LoadAsync" /> is called without one. Ignored while
        /// <see cref="UseTestAds" /> is enabled. Defaults to <see langword="null" />.
        /// </summary>
        public AdUnitId? RewardedInterstitialAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets the app-open ad unit id used when <see cref="IAppOpenAdService.LoadAsync" /> is
        /// called without one. Ignored while <see cref="UseTestAds" /> is enabled. Defaults to
        /// <see langword="null" />.
        /// </summary>
        public AdUnitId? AppOpenAdUnitId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether banners render Google's public sample test ad
        /// instead of the configured ad unit. Keep this enabled throughout development so no real
        /// impressions or revenue are generated. Defaults to <see langword="false" />.
        /// </summary>
        public bool UseTestAds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the app is directed at users under the age of consent
        /// (COPPA/GDPR-K). Passed to Google's User Messaging Platform so it applies the correct consent
        /// flow. Defaults to <see langword="false" />; set to <see langword="true" /> if the app is
        /// child-directed or mixed-audience with under-age users.
        /// </summary>
        public bool TagForUnderAgeOfConsent { get; set; }

        /// <summary>
        /// Gets or sets the AdMob-registered test device identifiers that should receive test creatives
        /// when requesting a real (non-<see cref="UseTestAds" />) ad unit — Google's recommended pattern
        /// for validating a production ad unit before shipping without generating invalid traffic.
        /// Defaults to an empty list, meaning no device receives test creatives for real ad units.
        /// </summary>
        public IReadOnlyList<string> TestDeviceIds { get; set; } = [];
    }
}
