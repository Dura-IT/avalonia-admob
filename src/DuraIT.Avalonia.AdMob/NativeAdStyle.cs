using Avalonia;
using Avalonia.Media;

namespace DuraIT.Avalonia.AdMob;

/// <summary>
/// An immutable snapshot of <see cref="Platforms.NativeAd" />'s style properties, captured once when
/// the native ad view is created and applied to the platform-native asset views. Kept separate from
/// the control so the resolved defaults are unit-testable without an Avalonia control instance.
/// </summary>
internal sealed record NativeAdStyle(
    bool ShowIcon,
    bool ShowMedia,
    bool ShowBody,
    bool ShowAdvertiser,
    bool ShowStarRating,
    bool ShowPrice,
    bool ShowStore,
    Color? CardBackground,
    Color? HeadlineForeground,
    Color? BodyForeground,
    Color? CallToActionBackground,
    Color? CallToActionForeground,
    double HeadlineFontSize,
    double BodyFontSize,
    double CornerRadius,
    Thickness Padding
)
{
    /// <summary>
    /// The style applied when the consuming app sets none of <see cref="Platforms.NativeAd" />'s style
    /// properties: every optional asset shown, native-default colors (<see langword="null" /> defers to
    /// the platform's own default text/background colors), and modest text sizes with no rounding or
    /// padding.
    /// </summary>
    internal static NativeAdStyle Default { get; } =
        new NativeAdStyle(
            ShowIcon: true,
            ShowMedia: true,
            ShowBody: true,
            ShowAdvertiser: true,
            ShowStarRating: true,
            ShowPrice: true,
            ShowStore: true,
            CardBackground: null,
            HeadlineForeground: null,
            BodyForeground: null,
            CallToActionBackground: null,
            CallToActionForeground: null,
            HeadlineFontSize: 16,
            BodyFontSize: 14,
            CornerRadius: 0,
            Padding: new Thickness(0)
        );
}
