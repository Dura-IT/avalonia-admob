namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Chooses the ad unit id an ad request should use: the configured id in normal operation, or the
/// supplied Google sample test unit when test ads are enabled or no id was configured. Shared by
/// every ad format so the test-ad substitution rule lives in one place.
/// </summary>
internal static class AdUnitResolver
{
    /// <summary>
    /// Resolves the effective ad unit id for a configured value, substituting
    /// <paramref name="testAdUnitId" /> when <see cref="AdMobOptions.UseTestAds" /> is enabled or no
    /// id was configured.
    /// </summary>
    internal static string Resolve(string? configuredAdUnitId, string testAdUnitId)
    {
        if (AdMobRuntime.Options.UseTestAds || string.IsNullOrWhiteSpace(configuredAdUnitId))
        {
            return testAdUnitId;
        }

        return configuredAdUnitId;
    }
}
