using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Platform-facing entry point for AdMob rewarded interstitial ads — a full-screen ad shown at a
    /// natural transition that still grants a reward for watching to completion, without the user opting
    /// in first. One implementation is registered per platform head: a live implementation on Android and
    /// iOS, and an inert placeholder on desktop so shared code can depend on the service unconditionally.
    /// Load an ad ahead of time, then present it at a transition point.
    /// </summary>
    public interface IRewardedInterstitialAdService
    {
        /// <summary>
        /// Gets a value indicating whether rewarded interstitial ads can be shown on the current platform.
        /// Returns <see langword="false" /> on desktop, where ads never render.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets a value indicating whether an ad has finished loading and is ready to present with
        /// <see cref="ShowAsync" />. Always <see langword="false" /> on desktop.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Begins loading a rewarded interstitial ad so it is ready to present later. Consent (UMP) is
        /// resolved first; if the user has not consented, no ad is requested. A no-op on desktop.
        /// </summary>
        /// <param name="adUnitId">
        /// The rewarded interstitial ad unit id to load. When <see langword="null" /> (the default), the id
        /// configured on <see cref="AdMobOptions.RewardedInterstitialAdUnitId" /> for the current platform
        /// is used. Whenever <see cref="AdMobOptions.UseTestAds" /> is enabled, Google's sample test unit is
        /// used regardless.
        /// </param>
        /// <returns>
        /// A task that completes once the load request has been issued.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown on a supported platform (Android or iOS) when no ad unit id is available — none passed
        /// here and none configured on <see cref="AdMobOptions.RewardedInterstitialAdUnitId" /> for the
        /// platform — and test ads are disabled.
        /// </exception>
        Task LoadAsync(string? adUnitId = null);

        /// <summary>
        /// Presents a previously loaded rewarded interstitial ad and waits for it to be dismissed. The ad
        /// is single-use, so load another with <see cref="LoadAsync" /> after it is dismissed.
        /// </summary>
        /// <returns>
        /// The <see cref="AdReward" /> the user earned by watching to completion, or <see langword="null" />
        /// if the user dismissed the ad early, no ad was ready, or the platform does not support rewarded
        /// interstitial ads. Grant the reward only when this is non-<see langword="null" />.
        /// </returns>
        Task<AdReward?> ShowAsync();
    }
}
