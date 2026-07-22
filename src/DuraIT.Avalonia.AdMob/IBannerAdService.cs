using System.Threading.Tasks;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Platform-facing entry point for the AdMob banner integration. One implementation is registered
/// per platform head: a live implementation on Android and iOS, and an inert placeholder on desktop
/// so shared UI can depend on the service unconditionally.
/// </summary>
public interface IBannerAdService
{
    /// <summary>
    /// Gets a value indicating whether banner ads can render on the current platform. Returns
    /// <see langword="false"/> on desktop, where the banner is a placeholder only.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether UMP requires a persistent privacy-options entry point for the
    /// current user, so they can revisit a previously made consent choice. True only where a consent
    /// form applies (for example an EEA user under GDPR); <see langword="false"/> on desktop and
    /// where consent is not regulated. Use it to show or hide a "manage consent" control. The value
    /// is only meaningful once the banner has requested consent, so re-read it when the hosting
    /// screen becomes visible rather than caching it at construction.
    /// </summary>
    bool IsPrivacyOptionsRequired { get; }

    /// <summary>
    /// Re-opens the UMP privacy options (consent) form so the user can change a previously made
    /// choice. A no-op that completes immediately where privacy options do not apply or no host
    /// window is available.
    /// </summary>
    /// <returns>
    /// A task that completes when the form is dismissed.
    /// </returns>
    Task ShowPrivacyOptionsAsync();
}
