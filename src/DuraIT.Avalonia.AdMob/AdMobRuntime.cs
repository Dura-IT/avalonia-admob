using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
///     Process-wide holder for the active <see cref="AdMobOptions" /> and logger factory. AdMob is a
///     singleton native SDK, so a single configuration is set once by
///     <see cref="ServiceCollectionExtensions.AddAdMobBanner" /> and read by <see cref="Platforms.BannerAd" />
///     instances, which the UI creates rather than the container.
/// </summary>
internal static class AdMobRuntime
{
    /// <summary>
    ///     Gets or sets the active options. Defaults to a new instance so reads are always safe.
    /// </summary>
    internal static AdMobOptions Options { get; set; } = new AdMobOptions();

    /// <summary>
    ///     Gets or sets the logger factory <see cref="Platforms.BannerAd" /> uses to report ad-load outcomes.
    ///     Defaults to a no-op factory so logging is always safe even when the consuming app supplies
    ///     none.
    /// </summary>
    internal static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
}
