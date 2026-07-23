using System;
using DuraIT.Avalonia.AdMob.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Dependency-injection helpers for registering the AdMob banner integration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the AdMob banner services and applies the supplied configuration. Call once during
    /// application startup. The concrete <see cref="IBannerAdService" /> is chosen per platform: a
    /// live implementation on Android and iOS, an inert placeholder on desktop.
    /// </summary>
    /// <param name="services">
    /// The service collection to add the registrations to.
    /// </param>
    /// <param name="configure">
    /// An optional callback for adjusting <see cref="AdMobOptions" />, for example enabling test ads.
    /// </param>
    /// <param name="loggerFactory">
    /// An optional logger factory used to report banner ad-load outcomes (loaded, failed, or blocked
    /// by consent). When <see langword="null" />, ad-load logging is silently discarded.
    /// </param>
    /// <returns>
    /// The same <paramref name="services" /> instance so calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services" /> is <see langword="null" />.
    /// </exception>
    public static IServiceCollection AddAdMobBanner(
        this IServiceCollection services,
        Action<AdMobOptions>? configure = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new AdMobOptions();
        configure?.Invoke(options);

        // BannerAd controls are created by the UI, not the container, so they read the active
        // options and logger factory from this process-wide holder rather than through injection.
        AdMobRuntime.Options = options;
        AdMobRuntime.LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

        services.AddSingleton(options);
        services.AddSingleton<IBannerAdService>(_ => new BannerAdService());

        return services;
    }
}
