using System;

namespace DuraIT.Avalonia.AdMob
{
    /// <summary>
    /// Chooses the ad unit id an ad request should use, applying one rule shared by every ad format: test
    /// ads win when enabled, an explicit id (set in XAML or passed to a load call) beats the configured
    /// default, and a missing id is reported rather than silently replaced with Google's sample unit — so a
    /// production app can never quietly serve unpaid test ads.
    /// </summary>
    internal static class AdUnitResolver
    {
        /// <summary>
        /// Resolves the effective ad unit id, or <see langword="null" /> when none is available and test
        /// ads are disabled. Precedence: <see cref="AdMobOptions.UseTestAds" /> forces
        /// <paramref name="testAdUnitId" />; otherwise a non-empty <paramref name="explicitAdUnitId" /> is
        /// used, then <paramref name="configuredAdUnitId" />; if both are empty, the result is
        /// <see langword="null" />.
        /// </summary>
        internal static string? Resolve(
            string? explicitAdUnitId,
            string? configuredAdUnitId,
            string testAdUnitId
        )
        {
            if (AdMobRuntime.Options.UseTestAds)
            {
                return testAdUnitId;
            }

            if (!string.IsNullOrWhiteSpace(explicitAdUnitId))
            {
                return explicitAdUnitId;
            }

            return string.IsNullOrWhiteSpace(configuredAdUnitId) ? null : configuredAdUnitId;
        }

        /// <summary>
        /// Resolves the effective ad unit id the same way as <see cref="Resolve" />, but throws when none is
        /// available instead of returning <see langword="null" />. Used by the full-screen ad services,
        /// whose load calls are a deliberate call site where a configuration error should surface loudly.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no ad unit id is available — none passed to the load call, none configured on
        /// <see cref="AdMobOptions" /> for the platform — and test ads are disabled.
        /// </exception>
        internal static string ResolveOrThrow(
            string? explicitAdUnitId,
            string? configuredAdUnitId,
            string testAdUnitId,
            string adFormat,
            string platform
        ) =>
            Resolve(explicitAdUnitId, configuredAdUnitId, testAdUnitId)
            ?? throw new InvalidOperationException(
                $"No {adFormat} ad unit id is configured for {platform}. Pass one to the load call, set the "
                    + "matching id on AdMobOptions, or enable AdMobOptions.UseTestAds to serve Google's sample ad."
            );
    }
}
