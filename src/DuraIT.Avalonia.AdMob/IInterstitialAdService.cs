using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Platform-facing entry point for AdMob interstitial ads — the full-screen ad shown between natural
    /// app transitions. One implementation is registered per platform head: a live implementation on
    /// Android and iOS, and an inert placeholder on desktop so shared code can depend on the service
    /// unconditionally. Load an ad ahead of time, then present it at a transition point.
    /// </summary>
    public interface IInterstitialAdService
    {
        /// <summary>
        /// Gets a value indicating whether interstitial ads can be shown on the current platform. Returns
        /// <see langword="false" /> on desktop, where ads never render.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets a value indicating whether an ad has finished loading and is ready to present with
        /// <see cref="ShowAsync" />. Always <see langword="false" /> on desktop.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Begins loading an interstitial ad so it is ready to present later. Consent (UMP) is resolved
        /// first; if the user has not consented, no ad is requested. A no-op on desktop.
        /// </summary>
        /// <param name="adUnitId">
        /// The interstitial ad unit id to load, or <see langword="null" /> to use Google's sample test
        /// unit. Substituted with the sample unit whenever <see cref="AdMobOptions.UseTestAds" /> is
        /// enabled.
        /// </param>
        /// <returns>
        /// A task that completes once the load request has been issued.
        /// </returns>
        Task LoadAsync(string? adUnitId = null);

        /// <summary>
        /// Presents a previously loaded interstitial ad. An interstitial is single-use, so load another
        /// with <see cref="LoadAsync" /> after it is dismissed.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if a loaded ad was presented; <see langword="false" /> if none was
        /// ready or the platform does not support interstitials.
        /// </returns>
        Task<bool> ShowAsync();
    }
}
