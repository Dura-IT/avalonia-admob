using System;
using DuraIT.Avalonia.AdMob.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// Dependency-injection helpers for registering the AdMob ad-format services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every AdMob ad-format service (banner and interstitial) and applies the supplied
    /// configuration. Call once during application startup; inject only the format services you use.
    /// Each concrete service is chosen per platform: a live implementation on Android and iOS, an
    /// inert placeholder on desktop.
    /// </summary>
    /// <param name="services">
    /// The service collection to add the registrations to.
    /// </param>
    /// <param name="configure">
    /// An optional callback for adjusting <see cref="AdMobOptions" />, for example enabling test ads.
    /// </param>
    /// <param name="loggerFactory">
    /// An optional logger factory used to report ad-load outcomes. When <see langword="null" />,
    /// ad-load logging is silently discarded.
    /// </param>
    /// <returns>
    /// The same <paramref name="services" /> instance so calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services" /> is <see langword="null" />.
    /// </exception>
    public static IServiceCollection AddAdMob(
        this IServiceCollection services,
        Action<AdMobOptions>? configure = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        AddAdMobCore(services, configure, loggerFactory);
        services.AddSingleton<IBannerAdService>(_ => new BannerAdService());
        services.AddSingleton<IInterstitialAdService>(_ => new InterstitialAdService());

        return services;
    }

    /// <summary>
    /// Registers the AdMob banner service and applies the supplied configuration. Call once during
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

        AddAdMobCore(services, configure, loggerFactory);
        services.AddSingleton<IBannerAdService>(_ => new BannerAdService());

        return services;
    }

    /// <summary>
    /// Registers the AdMob interstitial service and applies the supplied configuration. Call once
    /// during application startup. The concrete <see cref="IInterstitialAdService" /> is chosen per
    /// platform: a live implementation on Android and iOS, an inert placeholder on desktop.
    /// </summary>
    /// <param name="services">
    /// The service collection to add the registrations to.
    /// </param>
    /// <param name="configure">
    /// An optional callback for adjusting <see cref="AdMobOptions" />, for example enabling test ads.
    /// </param>
    /// <param name="loggerFactory">
    /// An optional logger factory used to report interstitial ad-load outcomes. When
    /// <see langword="null" />, ad-load logging is silently discarded.
    /// </param>
    /// <returns>
    /// The same <paramref name="services" /> instance so calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services" /> is <see langword="null" />.
    /// </exception>
    public static IServiceCollection AddAdMobInterstitial(
        this IServiceCollection services,
        Action<AdMobOptions>? configure = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        AddAdMobCore(services, configure, loggerFactory);
        services.AddSingleton<IInterstitialAdService>(_ => new InterstitialAdService());

        return services;
    }

    // Applies the configuration to the process-wide runtime holder and registers the options
    // instance. BannerAd controls and format services read the active options and logger factory from
    // AdMobRuntime rather than through injection, because the UI (not the container) creates them.
    private static void AddAdMobCore(
        IServiceCollection services,
        Action<AdMobOptions>? configure,
        ILoggerFactory? loggerFactory
    )
    {
        var options = new AdMobOptions();
        configure?.Invoke(options);

        AdMobRuntime.Options = options;
        AdMobRuntime.LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

        services.AddSingleton(options);
    }
}
