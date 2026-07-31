using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Platform;
using Avalonia.Threading;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using MT.GMA.iOS;
using UIKit;
using AvaloniaColor = Avalonia.Media.Color;
using IosNativeAd = MT.GMA.iOS.NativeAd;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// iOS rendering of <see cref="NativeAd" />: hosts a native AdMob <see cref="GADNativeAdView" /> inside
/// the Avalonia visual tree, built from a fixed asset template styled by the control's style
/// properties.
/// </summary>
public partial class NativeAd : NativeControlHost, IDisposable
{
    private IosNativeAd? _ad;
    private AdLoader? _adLoader;
    private NativeAdDelegate? _delegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeAd" /> class.
    /// </summary>
    public NativeAd()
    {
        MinHeight = DefaultMinHeight;
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdLoader, its delegate, and the GADNativeAdView's asset views transfer their lifetime to the returned native control handle and are released by the base DestroyNativeControlCore; disposing them here would break the ad."
    )]
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var logger = AdMobRuntime.LoggerFactory.CreateLogger<NativeAd>();
        string adUnitId = AdUnitResolver.Resolve(AdUnitId, AdMobTestIds.Native);
        var style = CaptureStyle();
        var rootViewController = ResolveRootViewController(parent);

        var adView = new TemplateNativeAdView();
        adView.SetLayoutCallback(UpdateHostHeight);
        var views = BuildTemplate(adView, style);

        // Consent (UMP) must be resolved before any ad is requested. If we can't resolve a view
        // controller to present a consent form on, fail open rather than leave the ad blank forever.
        if (rootViewController is not null)
        {
            _ = LoadWhenConsentedAsync(rootViewController, adUnitId, adView, views, style, logger);
        }
        else
        {
            LoadAd(null, adUnitId, adView, views, style, logger);
        }

        return new UIViewControlHandle(adView);
    }

    /// <inheritdoc />
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        ReleaseAdResources();
        base.DestroyNativeControlCore(control);
    }

    /// <summary>
    /// Releases the ad loader, its delegate, and the loaded native ad. Avalonia calls this through
    /// <see cref="DestroyNativeControlCore" /> when the control leaves the visual tree; exposing it as
    /// <see cref="IDisposable.Dispose" /> lets the SDK's ad loader — which must be held strongly for
    /// the duration of the request — be released deterministically instead of at the garbage
    /// collector's discretion.
    /// </summary>
    public void Dispose()
    {
        ReleaseAdResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseAdResources()
    {
        _adLoader?.Dispose();
        _adLoader = null;
        _delegate?.Dispose();
        _delegate = null;
        _ad?.Dispose();
        _ad = null;
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdLoader and its delegate are consumed by the native load call. Disposing them here would break the ad."
    )]
    private async Task LoadWhenConsentedAsync(
        UIViewController rootViewController,
        string adUnitId,
        GADNativeAdView adView,
        TemplateViews views,
        NativeAdStyle style,
        ILogger logger
    )
    {
        var canRequestAds = await AdMobInitializer.EnsureReadyAsync(rootViewController);
        if (canRequestAds)
        {
            LoadAd(rootViewController, adUnitId, adView, views, style, logger);
        }
        else
        {
            AdLoadLog.BlockedByConsent(logger, "native ad");
        }
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdLoader and its delegate are consumed by the native load call and their lifetime transfers to the native SDK. Disposing them here would break the ad."
    )]
    private void LoadAd(
        UIViewController? rootViewController,
        string adUnitId,
        GADNativeAdView adView,
        TemplateViews views,
        NativeAdStyle style,
        ILogger logger
    )
    {
        _delegate = new NativeAdDelegate(this, adView, views, style, logger, adUnitId);
        _adLoader = new AdLoader(
            adUnitId,
            rootViewController,
            new NSString[] { AdLoadAdTypeConstants.Native },
            null
        )
        {
            Delegate = _delegate,
        };
        _adLoader.LoadRequest(GADRequest.Request());
    }

    private void OnLoaded(
        IosNativeAd ad,
        GADNativeAdView adView,
        TemplateViews views,
        NativeAdStyle style,
        ILogger logger,
        string adUnitId
    )
    {
        _ad = ad;
        PopulateTemplate(ad, views, style);
        adView.NativeAd = ad;

        // The populated template is usually taller than the placeholder floor; size the host to it now
        // so no asset view is clipped outside the ad view (a clipped view is not counted as a click).
        UpdateHostHeight(adView);
        AdLoadLog.Loaded(logger, "native ad", adUnitId);
    }

    private static void OnFailedToLoad(NSError error, ILogger logger, string adUnitId) =>
        AdLoadLog.FailedToLoad(
            logger,
            "native ad",
            adUnitId,
            error.Code,
            error.LocalizedDescription
        );

    // Sizes the Avalonia host to the native template's natural height so every registered asset view
    // stays inside the ad view's bounds. Avalonia's NativeControlHost has no insight into native
    // content size, so the control keeps its placeholder MinHeight unless we measure and push the real
    // height back. Called after the ad is populated and on every native width change (see
    // TemplateNativeAdView), and marshals the property set onto the UI thread.
    private void UpdateHostHeight(GADNativeAdView adView)
    {
        nfloat width = adView.Frame.Width;
        if (width <= 0)
        {
            return;
        }

        var fitting = adView.SystemLayoutSizeFittingSize(
            new CGSize(width, 0),
            (float)(int)UILayoutPriority.Required,
            (float)(int)UILayoutPriority.FittingSizeLevel
        );
        double target = Math.Max(DefaultMinHeight, (double)fitting.Height);

        Dispatcher.UIThread.Post(() =>
        {
            if (double.IsNaN(Height) || Math.Abs(Height - target) > 0.5)
            {
                Height = target;
            }
        });
    }

    // Builds the fixed asset template as a subview of the GADNativeAdView and registers each asset
    // view with the SDK — this registration, not the content set later in PopulateTemplate, is what
    // makes impressions and clicks count.
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Every view and constraint created here is added into the GADNativeAdView's visual tree; their lifetime transfers to that tree and is released when the GADNativeAdView is destroyed, same as the rest of the native control hosting pattern."
    )]
    private static TemplateViews BuildTemplate(GADNativeAdView adView, NativeAdStyle style)
    {
        ApplyContainerStyle(adView, style);

        var root = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Vertical,
            Spacing = 4,
            TranslatesAutoresizingMaskIntoConstraints = false,
        };

        var headerRow = new UIStackView
        {
            Axis = UILayoutConstraintAxis.Horizontal,
            Spacing = 8,
            Alignment = UIStackViewAlignment.Center,
        };

        UIImageView? icon = null;
        if (style.ShowIcon)
        {
            icon = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleAspectFit,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            icon.WidthAnchor.ConstraintEqualTo(40).Active = true;
            icon.HeightAnchor.ConstraintEqualTo(40).Active = true;
            headerRow.AddArrangedSubview(icon);
        }

        var textBlock = new UIStackView { Axis = UILayoutConstraintAxis.Vertical };
        var headline = new UILabel
        {
            Font = UIFont.BoldSystemFontOfSize((nfloat)style.HeadlineFontSize),
        };
        ApplyTextColor(headline, style.HeadlineForeground);
        textBlock.AddArrangedSubview(headline);

        UILabel? advertiser = null;
        if (style.ShowAdvertiser)
        {
            advertiser = new UILabel { Font = UIFont.SystemFontOfSize((nfloat)style.BodyFontSize) };
            ApplyTextColor(advertiser, style.BodyForeground);
            textBlock.AddArrangedSubview(advertiser);
        }

        headerRow.AddArrangedSubview(textBlock);
        root.AddArrangedSubview(headerRow);

        UILabel? body = null;
        if (style.ShowBody)
        {
            body = new UILabel
            {
                Font = UIFont.SystemFontOfSize((nfloat)style.BodyFontSize),
                Lines = 0,
            };
            ApplyTextColor(body, style.BodyForeground);
            root.AddArrangedSubview(body);
        }

        GADMediaView? media = null;
        if (style.ShowMedia)
        {
            media = new GADMediaView { TranslatesAutoresizingMaskIntoConstraints = false };

            // Prefer a 16:9 media view, but never taller than the cap: on a wide card the pure aspect
            // ratio would push the rest of the template outside the ad view's bounds. High (not required)
            // priority lets the cap win when the two conflict.
            var aspect = media.HeightAnchor.ConstraintEqualTo(media.WidthAnchor, 9.0f / 16.0f);
            aspect.Priority = (float)UILayoutPriority.DefaultHigh;
            aspect.Active = true;
            media.HeightAnchor.ConstraintLessThanOrEqualTo((nfloat)MediaMaxHeight).Active = true;
            root.AddArrangedSubview(media);
        }

        var footerRow = new UIStackView { Axis = UILayoutConstraintAxis.Horizontal, Spacing = 8 };
        UILabel? starRating = null;
        if (style.ShowStarRating)
        {
            starRating = new UILabel { Font = UIFont.SystemFontOfSize((nfloat)style.BodyFontSize) };
            ApplyTextColor(starRating, style.BodyForeground);
            footerRow.AddArrangedSubview(starRating);
        }

        UILabel? price = null;
        if (style.ShowPrice)
        {
            price = new UILabel { Font = UIFont.SystemFontOfSize((nfloat)style.BodyFontSize) };
            ApplyTextColor(price, style.BodyForeground);
            footerRow.AddArrangedSubview(price);
        }

        UILabel? store = null;
        if (style.ShowStore)
        {
            store = new UILabel { Font = UIFont.SystemFontOfSize((nfloat)style.BodyFontSize) };
            ApplyTextColor(store, style.BodyForeground);
            footerRow.AddArrangedSubview(store);
        }

        if (footerRow.ArrangedSubviews.Length > 0)
        {
            root.AddArrangedSubview(footerRow);
        }

        var callToAction = new UIButton(UIButtonType.System);
        if (style.CallToActionForeground is { } ctaForeground)
        {
            callToAction.SetTitleColor(ToUIColor(ctaForeground), UIControlState.Normal);
        }

        if (style.CallToActionBackground is { } ctaBackground)
        {
            callToAction.BackgroundColor = ToUIColor(ctaBackground);
        }

        root.AddArrangedSubview(callToAction);

        adView.AddSubview(root);
        NSLayoutConstraint.ActivateConstraints(
            new[]
            {
                root.LeadingAnchor.ConstraintEqualTo(
                    adView.LeadingAnchor,
                    (nfloat)style.Padding.Left
                ),
                root.TrailingAnchor.ConstraintEqualTo(
                    adView.TrailingAnchor,
                    -(nfloat)style.Padding.Right
                ),
                root.TopAnchor.ConstraintEqualTo(adView.TopAnchor, (nfloat)style.Padding.Top),
                root.BottomAnchor.ConstraintEqualTo(
                    adView.BottomAnchor,
                    -(nfloat)style.Padding.Bottom
                ),
            }
        );

        adView.HeadlineView = headline;
        adView.BodyView = body;
        adView.IconView = icon;
        adView.MediaView = media;
        adView.CallToActionView = callToAction;
        adView.AdvertiserView = advertiser;
        adView.StarRatingView = starRating;
        adView.PriceView = price;
        adView.StoreView = store;

        return new TemplateViews(
            headline,
            body,
            icon,
            media,
            callToAction,
            advertiser,
            starRating,
            price,
            store
        );
    }

    // Fills the registered asset views with the loaded ad's content. The SDK requires the developer
    // to populate the views themselves — registering them (above) only wires up tracking.
    private static void PopulateTemplate(IosNativeAd ad, TemplateViews views, NativeAdStyle style)
    {
        views.Headline.Text = ad.Headline;
        views.CallToAction.SetTitle(ad.CallToAction, UIControlState.Normal);
        views.CallToAction.Hidden = string.IsNullOrEmpty(ad.CallToAction);

        SetOptionalText(views.Body, ad.Body);
        SetOptionalText(views.Advertiser, ad.Advertiser);
        SetOptionalText(views.Price, ad.Price);
        SetOptionalText(views.Store, ad.Store);

        if (views.StarRating is { } starRatingView)
        {
            double starRating = ad.StarRating?.DoubleValue ?? 0;
            starRatingView.Hidden = starRating <= 0;
            starRatingView.Text = $"★ {starRating:0.0}";
        }

        if (views.Icon is { } iconView)
        {
            var icon = ad.Icon;
            iconView.Hidden = icon is null;
            if (icon?.Image is { } image)
            {
                iconView.Image = image;
            }
        }

        if (views.Media is { } mediaView && style.ShowMedia)
        {
            mediaView.MediaContent = ad.MediaContent;
        }
    }

    private static void SetOptionalText(UILabel? label, string? text)
    {
        if (label is null)
        {
            return;
        }

        label.Text = text;
        label.Hidden = string.IsNullOrEmpty(text);
    }

    private static void ApplyTextColor(UILabel label, AvaloniaColor? color)
    {
        if (color is { } c)
        {
            label.TextColor = ToUIColor(c);
        }
    }

    private static void ApplyContainerStyle(GADNativeAdView adView, NativeAdStyle style)
    {
        if (style.CardBackground is { } background)
        {
            adView.BackgroundColor = ToUIColor(background);
        }

        if (style.CornerRadius > 0)
        {
            adView.Layer.CornerRadius = (nfloat)style.CornerRadius;
            adView.ClipsToBounds = true;
        }
    }

    private static UIColor ToUIColor(AvaloniaColor color) =>
        UIColor.FromRGBA(color.R, color.G, color.B, color.A);

    // The ad needs a root view controller to resolve consent and load correctly. Prefer the
    // controller hosting the Avalonia view; fall back to the active scene's key window.
    private static UIViewController? ResolveRootViewController(IPlatformHandle parent)
    {
        var fromParent = (parent as UIViewControlHandle)?.View?.Window?.RootViewController;
        if (fromParent is not null)
        {
            return fromParent;
        }

        return UIApplication
            .SharedApplication.ConnectedScenes.OfType<UIWindowScene>()
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(window => window.IsKeyWindow)
            ?.RootViewController;
    }

    // Holds the asset views created for one GADNativeAdView so the loaded-ad callback can populate
    // them without re-walking the view tree.
    private sealed record TemplateViews(
        UILabel Headline,
        UILabel? Body,
        UIImageView? Icon,
        GADMediaView? Media,
        UIButton CallToAction,
        UILabel? Advertiser,
        UILabel? StarRating,
        UILabel? Price,
        UILabel? Store
    );

    // A GADNativeAdView that reports each native layout pass so the host can size itself to the
    // template's natural height. Re-measuring is gated on a width change: a height change is the result
    // of our own measurement, and re-measuring on it would loop. This covers the initial layout (when
    // the frame width is first known) and device rotation; the content-driven measure happens directly
    // from OnLoaded.
    private sealed class TemplateNativeAdView : GADNativeAdView
    {
        private Action<GADNativeAdView>? _onLayout;
        private nfloat _lastWidth = -1;

        public void SetLayoutCallback(Action<GADNativeAdView> onLayout) => _onLayout = onLayout;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Math.Abs((double)(Frame.Width - _lastWidth)) <= 0.5)
            {
                return;
            }

            _lastWidth = Frame.Width;
            _onLayout?.Invoke(this);
        }
    }

    // Bridges the native load callbacks into the service. The AdLoader owns this instance until it
    // fires.
    private sealed class NativeAdDelegate : NativeAdLoaderDelegate
    {
        private readonly NativeAd _control;
        private readonly GADNativeAdView _adView;
        private readonly TemplateViews _views;
        private readonly NativeAdStyle _style;
        private readonly ILogger _logger;
        private readonly string _adUnitId;

        public NativeAdDelegate(
            NativeAd control,
            GADNativeAdView adView,
            TemplateViews views,
            NativeAdStyle style,
            ILogger logger,
            string adUnitId
        )
        {
            _control = control;
            _adView = adView;
            _views = views;
            _style = style;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public override void DidReceiveNativeAd(AdLoader adLoader, IosNativeAd nativeAd) =>
            _control.OnLoaded(nativeAd, _adView, _views, _style, _logger, _adUnitId);

        public override void DidFailToReceiveAd(AdLoader adLoader, NSError error) =>
            OnFailedToLoad(error, _logger, _adUnitId);
    }
}
