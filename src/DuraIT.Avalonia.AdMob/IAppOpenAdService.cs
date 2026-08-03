using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Platform-facing entry point for AdMob app-open ads — the full-screen ad shown while the app is
    /// loading or returning to the foreground. One implementation is registered per platform head: a live
    /// implementation on Android and iOS, and an inert placeholder on desktop so shared code can depend on
    /// the service unconditionally. Load an ad ahead of time, then present it when the app next comes to
    /// the foreground.
    /// </summary>
    /// <remarks>
    /// A loaded app-open ad goes stale after four hours; call <see cref="LoadAsync" /> again if
    /// <see cref="IsReady" /> has since turned <see langword="false" />. This service presents on demand
    /// only — it does not subscribe to platform foreground events, so the app decides when to call
    /// <see cref="ShowAsync" /> (typically from its own lifecycle hook).
    /// </remarks>
    public interface IAppOpenAdService
    {
        /// <summary>
        /// Gets a value indicating whether app-open ads can be shown on the current platform. Returns
        /// <see langword="false" /> on desktop, where ads never render.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets a value indicating whether an ad has finished loading, has not yet expired, and is ready
        /// to present with <see cref="ShowAsync" />. Always <see langword="false" /> on desktop.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Begins loading an app-open ad so it is ready to present later. Consent (UMP) is resolved first;
        /// if the user has not consented, no ad is requested. A no-op on desktop.
        /// </summary>
        /// <param name="adUnitId">
        /// The app-open ad unit id to load, or <see langword="null" /> to use Google's sample test unit.
        /// Substituted with the sample unit whenever <see cref="AdMobOptions.UseTestAds" /> is enabled.
        /// </param>
        /// <returns>
        /// A task that completes once the load request has been issued.
        /// </returns>
        Task LoadAsync(string? adUnitId = null);

        /// <summary>
        /// Presents a previously loaded app-open ad. An app-open ad is single-use, so load another with
        /// <see cref="LoadAsync" /> after it is dismissed.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if a loaded ad was presented; <see langword="false" /> if none was
        /// ready (including when the loaded ad has expired) or the platform does not support app-open ads.
        /// </returns>
        Task<bool> ShowAsync();
    }
}
