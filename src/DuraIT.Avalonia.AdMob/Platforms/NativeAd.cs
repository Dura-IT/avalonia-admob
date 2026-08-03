using Avalonia;
using Avalonia.Media;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// An AdMob native (in-feed) ad control. It hosts a native asset template (Android <c>NativeAdView</c> /
    /// iOS <c>GADNativeAdView</c>) inside the Avalonia visual tree; on desktop it renders an inert
    /// placeholder. Set <see cref="AdUnitId" /> to your native ad unit, or enable
    /// <see cref="AdMobOptions.UseTestAds" /> to serve Google's sample test ad during development.
    /// </summary>
    /// <remarks>
    /// Unlike a regular Avalonia control, the ad's assets (headline, icon, media, call-to-action, etc.)
    /// cannot be composed in your own XAML template: AdMob only counts impressions and clicks — and stays
    /// policy-compliant — when the asset views are native views registered with the platform SDK's asset
    /// wrapper. This control renders a built-in native template and exposes styling through the properties
    /// below instead. Styles are read once, when the native view is created; changing a style property
    /// after the ad has rendered does not restyle it live.
    /// </remarks>
    public sealed partial class NativeAd
    {
        // Default minimum height (device-independent pixels) for the hosted native ad view. A
        // NativeControlHost has no intrinsic size, so without a floor the control collapses to zero in an
        // auto-sizing panel (e.g. a DockPanel row) and Avalonia never creates its native view — so no ad
        // is ever requested. The mobile partials apply this in their constructor; consumers can override
        // it by setting Height or MinHeight in XAML for a taller ad template.
        internal const double DefaultMinHeight = 280;

        // Upper bound (device-independent pixels) on the media asset's height. The media view is otherwise
        // 16:9, which on a wide card (e.g. a tablet, where the card can be 700+ dip across) would balloon
        // past 400 dip and push the call-to-action and footer outside the ad view's bounds — clipped, and
        // no longer counted as clicks. Capping the media keeps the whole template inside the ad view so the
        // AdMob SDK registers every asset. The mobile partials size the card to its measured content, with
        // this cap keeping that measurement sane across screen widths.
        internal const double MediaMaxHeight = 200;

        /// <summary>
        /// Defines the <see cref="AdUnitId" /> property.
        /// </summary>
        public static readonly StyledProperty<string?> AdUnitIdProperty = AvaloniaProperty.Register<
            NativeAd,
            string?
        >(nameof(AdUnitId));

        /// <summary>
        /// Defines the <see cref="ShowIcon" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowIconProperty = AvaloniaProperty.Register<
            NativeAd,
            bool
        >(nameof(ShowIcon), NativeAdStyle.Default.ShowIcon);

        /// <summary>
        /// Defines the <see cref="ShowMedia" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowMediaProperty = AvaloniaProperty.Register<
            NativeAd,
            bool
        >(nameof(ShowMedia), NativeAdStyle.Default.ShowMedia);

        /// <summary>
        /// Defines the <see cref="ShowBody" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowBodyProperty = AvaloniaProperty.Register<
            NativeAd,
            bool
        >(nameof(ShowBody), NativeAdStyle.Default.ShowBody);

        /// <summary>
        /// Defines the <see cref="ShowAdvertiser" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowAdvertiserProperty =
            AvaloniaProperty.Register<NativeAd, bool>(
                nameof(ShowAdvertiser),
                NativeAdStyle.Default.ShowAdvertiser
            );

        /// <summary>
        /// Defines the <see cref="ShowStarRating" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowStarRatingProperty =
            AvaloniaProperty.Register<NativeAd, bool>(
                nameof(ShowStarRating),
                NativeAdStyle.Default.ShowStarRating
            );

        /// <summary>
        /// Defines the <see cref="ShowPrice" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowPriceProperty = AvaloniaProperty.Register<
            NativeAd,
            bool
        >(nameof(ShowPrice), NativeAdStyle.Default.ShowPrice);

        /// <summary>
        /// Defines the <see cref="ShowStore" /> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowStoreProperty = AvaloniaProperty.Register<
            NativeAd,
            bool
        >(nameof(ShowStore), NativeAdStyle.Default.ShowStore);

        /// <summary>
        /// Defines the <see cref="CardBackground" /> property.
        /// </summary>
        public static readonly StyledProperty<Color?> CardBackgroundProperty =
            AvaloniaProperty.Register<NativeAd, Color?>(
                nameof(CardBackground),
                NativeAdStyle.Default.CardBackground
            );

        /// <summary>
        /// Defines the <see cref="HeadlineForeground" /> property.
        /// </summary>
        public static readonly StyledProperty<Color?> HeadlineForegroundProperty =
            AvaloniaProperty.Register<NativeAd, Color?>(
                nameof(HeadlineForeground),
                NativeAdStyle.Default.HeadlineForeground
            );

        /// <summary>
        /// Defines the <see cref="BodyForeground" /> property.
        /// </summary>
        public static readonly StyledProperty<Color?> BodyForegroundProperty =
            AvaloniaProperty.Register<NativeAd, Color?>(
                nameof(BodyForeground),
                NativeAdStyle.Default.BodyForeground
            );

        /// <summary>
        /// Defines the <see cref="CallToActionBackground" /> property.
        /// </summary>
        public static readonly StyledProperty<Color?> CallToActionBackgroundProperty =
            AvaloniaProperty.Register<NativeAd, Color?>(
                nameof(CallToActionBackground),
                NativeAdStyle.Default.CallToActionBackground
            );

        /// <summary>
        /// Defines the <see cref="CallToActionForeground" /> property.
        /// </summary>
        public static readonly StyledProperty<Color?> CallToActionForegroundProperty =
            AvaloniaProperty.Register<NativeAd, Color?>(
                nameof(CallToActionForeground),
                NativeAdStyle.Default.CallToActionForeground
            );

        /// <summary>
        /// Defines the <see cref="HeadlineFontSize" /> property.
        /// </summary>
        public static readonly StyledProperty<double> HeadlineFontSizeProperty =
            AvaloniaProperty.Register<NativeAd, double>(
                nameof(HeadlineFontSize),
                NativeAdStyle.Default.HeadlineFontSize
            );

        /// <summary>
        /// Defines the <see cref="BodyFontSize" /> property.
        /// </summary>
        public static readonly StyledProperty<double> BodyFontSizeProperty =
            AvaloniaProperty.Register<NativeAd, double>(
                nameof(BodyFontSize),
                NativeAdStyle.Default.BodyFontSize
            );

        /// <summary>
        /// Defines the <see cref="CornerRadius" /> property.
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<NativeAd, double>(
                nameof(CornerRadius),
                NativeAdStyle.Default.CornerRadius
            );

        /// <summary>
        /// Defines the <see cref="Padding" /> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> PaddingProperty =
            AvaloniaProperty.Register<NativeAd, Thickness>(
                nameof(Padding),
                NativeAdStyle.Default.Padding
            );

        /// <summary>
        /// Gets or sets the AdMob native ad unit id to load. Substituted with a sample test unit when
        /// <see cref="AdMobOptions.UseTestAds" /> is enabled.
        /// </summary>
        public string? AdUnitId
        {
            get => GetValue(AdUnitIdProperty);
            set => SetValue(AdUnitIdProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the icon asset is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowIcon
        {
            get => GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the media asset (image or video) is shown. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowMedia
        {
            get => GetValue(ShowMediaProperty);
            set => SetValue(ShowMediaProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the body text asset is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowBody
        {
            get => GetValue(ShowBodyProperty);
            set => SetValue(ShowBodyProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the advertiser name is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowAdvertiser
        {
            get => GetValue(ShowAdvertiserProperty);
            set => SetValue(ShowAdvertiserProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the star rating is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowStarRating
        {
            get => GetValue(ShowStarRatingProperty);
            set => SetValue(ShowStarRatingProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the price is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowPrice
        {
            get => GetValue(ShowPriceProperty);
            set => SetValue(ShowPriceProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the store name is shown, when the ad provides one. Defaults to
        /// <see langword="true" />.
        /// </summary>
        public bool ShowStore
        {
            get => GetValue(ShowStoreProperty);
            set => SetValue(ShowStoreProperty, value);
        }

        /// <summary>
        /// Gets or sets the card's background color. <see langword="null" /> (the default) leaves the
        /// platform's own default background in place.
        /// </summary>
        public Color? CardBackground
        {
            get => GetValue(CardBackgroundProperty);
            set => SetValue(CardBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the headline text color. <see langword="null" /> (the default) leaves the
        /// platform's own default text color in place.
        /// </summary>
        public Color? HeadlineForeground
        {
            get => GetValue(HeadlineForegroundProperty);
            set => SetValue(HeadlineForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the body text color. <see langword="null" /> (the default) leaves the platform's
        /// own default text color in place.
        /// </summary>
        public Color? BodyForeground
        {
            get => GetValue(BodyForegroundProperty);
            set => SetValue(BodyForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the call-to-action button's background color. <see langword="null" /> (the
        /// default) leaves the platform's own default button background in place.
        /// </summary>
        public Color? CallToActionBackground
        {
            get => GetValue(CallToActionBackgroundProperty);
            set => SetValue(CallToActionBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the call-to-action button's text color. <see langword="null" /> (the default)
        /// leaves the platform's own default button text color in place.
        /// </summary>
        public Color? CallToActionForeground
        {
            get => GetValue(CallToActionForegroundProperty);
            set => SetValue(CallToActionForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the headline font size, in device-independent pixels. Defaults to 16.
        /// </summary>
        public double HeadlineFontSize
        {
            get => GetValue(HeadlineFontSizeProperty);
            set => SetValue(HeadlineFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the body text font size, in device-independent pixels. Defaults to 14.
        /// </summary>
        public double BodyFontSize
        {
            get => GetValue(BodyFontSizeProperty);
            set => SetValue(BodyFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the card's corner radius, in device-independent pixels. Defaults to 0.
        /// </summary>
        public double CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding between the card's edge and its asset content. Defaults to none.
        /// </summary>
        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        // Captures the current style property values into an immutable snapshot for the platform
        // implementation to apply to the native asset views at creation time.
        internal NativeAdStyle CaptureStyle() =>
            new NativeAdStyle(
                ShowIcon,
                ShowMedia,
                ShowBody,
                ShowAdvertiser,
                ShowStarRating,
                ShowPrice,
                ShowStore,
                CardBackground,
                HeadlineForeground,
                BodyForeground,
                CallToActionBackground,
                CallToActionForeground,
                HeadlineFontSize,
                BodyFontSize,
                CornerRadius,
                Padding
            );
    }
}
