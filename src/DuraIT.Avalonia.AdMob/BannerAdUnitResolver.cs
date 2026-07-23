namespace DuraIT.Avalonia.AdMob;

/// <summary>
///     Chooses the ad unit id a banner should load: the configured id in normal operation, or Google's
///     sample test unit when test ads are enabled or no id was configured.
/// </summary>
internal static class BannerAdUnitResolver
{
    /// <summary>
    ///     Resolves the effective banner ad unit id for the given configured value.
    /// </summary>
    internal static string Resolve(string? configuredAdUnitId)
    {
        if (AdMobRuntime.Options.UseTestAds || string.IsNullOrWhiteSpace(configuredAdUnitId))
        {
            return AdMobTestIds.Banner;
        }

        return configuredAdUnitId;
    }
}
