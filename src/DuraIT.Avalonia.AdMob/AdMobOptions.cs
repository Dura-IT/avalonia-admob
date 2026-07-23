using System.Collections.Generic;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Configuration for the AdMob banner integration, supplied by the consuming application at startup
/// through <see cref="ServiceCollectionExtensions.AddAdMobBanner" />.
/// </summary>
public sealed class AdMobOptions
{
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
