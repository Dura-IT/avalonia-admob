using Avalonia;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// An AdMob banner ad control. On mobile it hosts the native banner view (Android <c>AdView</c> /
/// iOS <c>GADBannerView</c>) inside the Avalonia visual tree; on desktop it renders an inert
/// placeholder. Set <see cref="AdUnitId" /> to your banner ad unit, or enable
/// <see cref="AdMobOptions.UseTestAds" /> to serve Google's sample test ad during development.
/// </summary>
public partial class BannerAd
{
    /// <summary>
    /// Defines the <see cref="AdUnitId" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> AdUnitIdProperty = AvaloniaProperty.Register<
        BannerAd,
        string?
    >(nameof(AdUnitId));

    /// <summary>
    /// Gets or sets the AdMob banner ad unit id to load. Substituted with a sample test unit when
    /// <see cref="AdMobOptions.UseTestAds" /> is enabled.
    /// </summary>
    public string? AdUnitId
    {
        get => GetValue(AdUnitIdProperty);
        set => SetValue(AdUnitIdProperty, value);
    }
}
