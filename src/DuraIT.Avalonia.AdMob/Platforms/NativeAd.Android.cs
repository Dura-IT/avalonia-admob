using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using AndroidButton = Android.Widget.Button;
using AndroidNativeAd = Android.Gms.Ads.NativeAd.NativeAd;
using AndroidNativeAdView = Android.Gms.Ads.NativeAd.NativeAdView;
using AvaloniaColor = Avalonia.Media.Color;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// Android rendering of <see cref="NativeAd" />: hosts a native AdMob
    /// <see cref="AndroidNativeAdView" /> inside the Avalonia visual tree, built from a fixed asset
    /// template styled by the control's style properties.
    /// </summary>
    [SuppressMessage(
        "Naming",
        "CA1724:Type names should not match namespaces",
        Justification = "The conflict is with Google's own Android.Gms.Ads.NativeAd namespace, referenced only in this Android-specific partial file; the AndroidNativeAd/AndroidNativeAdView aliases already disambiguate every use, so there is no real ambiguity to fix."
    )]
    public partial class NativeAd : NativeControlHost
    {
        private AndroidNativeAd? _ad;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeAd" /> class.
        /// </summary>
        public NativeAd()
        {
            MinHeight = DefaultMinHeight;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when no Android context is available to create the native ad view.
        /// </exception>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The AdLoader, its listeners, and the AdRequest are consumed by the native load call and their lifetime transfers to the native SDK and the returned NativeAdView; disposing them here would break the ad."
        )]
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            var context =
                (parent as AndroidViewControlHandle)?.View?.Context
                ?? global::Android.App.Application.Context
                ?? throw new InvalidOperationException(
                    "No Android context is available to create the AdMob native ad view."
                );

            var logger = AdMobRuntime.LoggerFactory.CreateLogger<NativeAd>();
            string adUnitId = AdUnitResolver.Resolve(AdUnitId, AdMobTestIds.Native);
            var style = CaptureStyle();

            var adView = new AndroidNativeAdView(context);
            var views = BuildTemplate(context, adView, style);

            // NativeAdView is sealed, so we can't override its layout; a layout-change listener reports each
            // width change instead, letting the host re-measure to the template's natural height on the
            // initial layout and on rotation.
            adView.AddOnLayoutChangeListener(
                new LayoutChangeListener(widthPx => UpdateHostHeight(adView, widthPx))
            );

            // Consent (UMP) must be resolved before any ad is requested. If we can't resolve an activity
            // to present a consent form on, fail open rather than leave the ad blank forever.
            if (ResolveActivity(context) is { } activity)
            {
                AdMobInitializer.CurrentActivity = activity;
                _ = LoadWhenConsentedAsync(activity, adUnitId, adView, views, style, logger);
            }
            else
            {
                LoadAd(context, adUnitId, adView, views, style, logger);
            }

            return new AndroidViewControlHandle(adView);
        }

        /// <inheritdoc />
        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (control is AndroidViewControlHandle handle && handle.View is AndroidNativeAdView)
            {
                _ad?.Destroy();
                _ad = null;
            }

            base.DestroyNativeControlCore(control);
        }

        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The AdRequest and its builder are consumed by the native LoadAd call. Disposing them here would break the native ad."
        )]
        private async Task LoadWhenConsentedAsync(
            Activity activity,
            string adUnitId,
            AndroidNativeAdView adView,
            TemplateViews views,
            NativeAdStyle style,
            ILogger logger
        )
        {
            var canRequestAds = await AdMobInitializer.EnsureReadyAsync(activity);
            if (canRequestAds)
            {
                LoadAd(activity, adUnitId, adView, views, style, logger);
            }
            else
            {
                AdLoadLog.BlockedByConsent(logger, "native ad");
            }
        }

        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The AdLoader, its listeners, and the AdRequest are consumed by the native load call. Disposing them here would break the native ad."
        )]
        private void LoadAd(
            Context context,
            string adUnitId,
            AndroidNativeAdView adView,
            TemplateViews views,
            NativeAdStyle style,
            ILogger logger
        )
        {
            var adLoader = new AdLoader.Builder(context, adUnitId)
                .ForNativeAd(
                    new NativeAdLoadedListener(this, adView, views, style, logger, adUnitId)
                )
                .WithAdListener(new NativeAdListener(logger, adUnitId))
                .Build();

            adLoader.LoadAd(new AdRequest.Builder().Build());
        }

        private void OnLoaded(
            AndroidNativeAd ad,
            AndroidNativeAdView adView,
            TemplateViews views,
            NativeAdStyle style,
            ILogger logger,
            string adUnitId
        )
        {
            _ad = ad;
            PopulateTemplate(ad, views, style);
            adView.SetNativeAd(ad);

            // The populated template is usually taller than the placeholder floor; size the host to it now
            // so no asset view is clipped outside the ad view (a clipped view is not counted as a click).
            UpdateHostHeight(adView, adView.Width);
            AdLoadLog.Loaded(logger, "native ad", adUnitId);
        }

        private static void OnFailedToLoad(LoadAdError error, ILogger logger, string adUnitId) =>
            AdLoadLog.FailedToLoad(logger, "native ad", adUnitId, error.Code, error.Message);

        // Sizes the Avalonia host to the native template's natural height so every registered asset view
        // stays inside the ad view's bounds. Avalonia's NativeControlHost has no insight into native
        // content size, so the control keeps its placeholder MinHeight unless we measure and push the real
        // height back. Called after the ad is populated and on every native width change (see
        // TemplateNativeAdView), and marshals the property set onto the UI thread.
        private void UpdateHostHeight(View adView, int widthPx)
        {
            if (widthPx <= 0 || adView.Context is not { } context)
            {
                return;
            }

            int widthSpec = View.MeasureSpec.MakeMeasureSpec(widthPx, MeasureSpecMode.Exactly);
            int heightSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            adView.Measure(widthSpec, heightSpec);

            double density = context.Resources?.DisplayMetrics?.Density ?? 1.0;
            double target = Math.Max(DefaultMinHeight, adView.MeasuredHeight / density);

            Dispatcher.UIThread.Post(() =>
            {
                if (double.IsNaN(Height) || Math.Abs(Height - target) > 0.5)
                {
                    Height = target;
                }
            });
        }

        // Builds the fixed asset template as a child of the NativeAdView and registers each asset view
        // with the SDK — this registration, not the content set later in PopulateTemplate, is what makes
        // impressions and clicks count.
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Every view and layout-params instance created here is added into the NativeAdView's visual tree; their lifetime transfers to that tree and is released when the NativeAdView is destroyed, same as the rest of the native control hosting pattern."
        )]
        private static TemplateViews BuildTemplate(
            Context context,
            AndroidNativeAdView adView,
            NativeAdStyle style
        )
        {
            double density = context.Resources?.DisplayMetrics?.Density ?? 1.0;
            int ToPx(double dp) => (int)(dp * density);

            var root = new LinearLayout(context) { Orientation = Orientation.Vertical };
            root.SetPadding(
                ToPx(style.Padding.Left),
                ToPx(style.Padding.Top),
                ToPx(style.Padding.Right),
                ToPx(style.Padding.Bottom)
            );
            ApplyContainerStyle(root, style, ToPx(style.CornerRadius));

            var headerRow = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            ImageView? icon = null;
            if (style.ShowIcon)
            {
                icon = new ImageView(context);
                var iconParams = new LinearLayout.LayoutParams(ToPx(40), ToPx(40));
                iconParams.SetMargins(0, 0, ToPx(8), 0);
                headerRow.AddView(icon, iconParams);
            }

            var textBlock = new LinearLayout(context) { Orientation = Orientation.Vertical };
            var headline = new TextView(context) { TextSize = (float)style.HeadlineFontSize };
            ApplyTextColor(headline, style.HeadlineForeground);
            textBlock.AddView(headline);

            TextView? advertiser = null;
            if (style.ShowAdvertiser)
            {
                advertiser = new TextView(context) { TextSize = (float)style.BodyFontSize };
                ApplyTextColor(advertiser, style.BodyForeground);
                textBlock.AddView(advertiser);
            }

            headerRow.AddView(textBlock);
            root.AddView(headerRow);

            TextView? body = null;
            if (style.ShowBody)
            {
                body = new TextView(context) { TextSize = (float)style.BodyFontSize };
                ApplyTextColor(body, style.BodyForeground);
                root.AddView(body);
            }

            global::Android.Gms.Ads.NativeAd.MediaView? media = null;
            if (style.ShowMedia)
            {
                media = new global::Android.Gms.Ads.NativeAd.MediaView(context);

                // A fixed, capped media band (rather than a weight that fills the card) keeps the template's
                // height driven by its content: the card is measured and sized to fit — see UpdateHostHeight
                // — so a tall media view can't push the call-to-action and footer outside the ad view's
                // bounds, where a clipped view would no longer count as a click.
                var mediaParams = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ToPx(MediaMaxHeight)
                );
                root.AddView(media, mediaParams);
            }

            var footerRow = new LinearLayout(context) { Orientation = Orientation.Horizontal };
            TextView? starRating = null;
            if (style.ShowStarRating)
            {
                starRating = new TextView(context) { TextSize = (float)style.BodyFontSize };
                ApplyTextColor(starRating, style.BodyForeground);
                footerRow.AddView(starRating);
            }

            TextView? price = null;
            if (style.ShowPrice)
            {
                price = new TextView(context) { TextSize = (float)style.BodyFontSize };
                ApplyTextColor(price, style.BodyForeground);
                footerRow.AddView(price);
            }

            TextView? store = null;
            if (style.ShowStore)
            {
                store = new TextView(context) { TextSize = (float)style.BodyFontSize };
                ApplyTextColor(store, style.BodyForeground);
                footerRow.AddView(store);
            }

            if (footerRow.ChildCount > 0)
            {
                root.AddView(footerRow);
            }

            var callToAction = new AndroidButton(context);
            if (style.CallToActionForeground is { } ctaForeground)
            {
                callToAction.SetTextColor(
                    Color.Argb(ctaForeground.A, ctaForeground.R, ctaForeground.G, ctaForeground.B)
                );
            }

            if (style.CallToActionBackground is { } ctaBackground)
            {
                callToAction.SetBackgroundColor(
                    Color.Argb(ctaBackground.A, ctaBackground.R, ctaBackground.G, ctaBackground.B)
                );
            }

            root.AddView(callToAction);

            adView.AddView(
                root,
                new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent
                )
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
        private static void PopulateTemplate(
            AndroidNativeAd ad,
            TemplateViews views,
            NativeAdStyle style
        )
        {
            views.Headline.Text = ad.Headline;
            views.CallToAction.Text = ad.CallToAction;
            views.CallToAction.Visibility = string.IsNullOrEmpty(ad.CallToAction)
                ? ViewStates.Invisible
                : ViewStates.Visible;

            SetOptionalText(views.Body, ad.Body);
            SetOptionalText(views.Advertiser, ad.Advertiser);
            SetOptionalText(views.Price, ad.Price);
            SetOptionalText(views.Store, ad.Store);

            if (views.StarRating is { } starRatingView)
            {
                double? starRating = ad.StarRating?.DoubleValue();
                starRatingView.Visibility = starRating > 0 ? ViewStates.Visible : ViewStates.Gone;
                starRatingView.Text = $"★ {starRating:0.0}";
            }

            if (views.Icon is { } iconView)
            {
                var icon = ad.Icon;
                iconView.Visibility = icon is null ? ViewStates.Gone : ViewStates.Visible;
                if (icon?.Drawable is { } drawable)
                {
                    iconView.SetImageDrawable(drawable);
                }
            }

            if (views.Media is { } mediaView && style.ShowMedia)
            {
                mediaView.MediaContent = ad.MediaContent;
            }
        }

        private static void SetOptionalText(TextView? view, string? text)
        {
            if (view is null)
            {
                return;
            }

            view.Text = text;
            view.Visibility = string.IsNullOrEmpty(text) ? ViewStates.Gone : ViewStates.Visible;
        }

        private static void ApplyTextColor(TextView view, AvaloniaColor? color)
        {
            if (color is { } c)
            {
                view.SetTextColor(Color.Argb(c.A, c.R, c.G, c.B));
            }
        }

        private static void ApplyContainerStyle(
            LinearLayout root,
            NativeAdStyle style,
            int cornerRadiusPx
        )
        {
            if (style.CardBackground is null && cornerRadiusPx == 0)
            {
                return;
            }

            var drawable = new GradientDrawable();
            drawable.SetCornerRadius(cornerRadiusPx);
            if (style.CardBackground is { } background)
            {
                drawable.SetColor(
                    Color.Argb(background.A, background.R, background.G, background.B)
                );
            }

            root.Background = drawable;
        }

        // The consent flow needs the hosting Activity, not just a Context. Avalonia hands us the Context
        // of the view it created, which is either the Activity itself or a ContextWrapper around it.
        private static Activity? ResolveActivity(Context? context)
        {
            while (context is not null)
            {
                if (context is Activity activity)
                {
                    return activity;
                }

                context = (context as ContextWrapper)?.BaseContext;
            }

            return null;
        }

        // Holds the asset views created for one NativeAdView so the loaded-ad callback can populate them
        // without re-walking the view tree.
        private sealed record TemplateViews(
            TextView Headline,
            TextView? Body,
            ImageView? Icon,
            global::Android.Gms.Ads.NativeAd.MediaView? Media,
            AndroidButton CallToAction,
            TextView? Advertiser,
            TextView? StarRating,
            TextView? Price,
            TextView? Store
        );

        // Reports each layout pass of the (sealed, non-overridable) NativeAdView so the host can size
        // itself to the template's natural height. Re-measuring is gated on a width change: a height change
        // is the result of our own measurement, and re-measuring on it would loop. This covers the initial
        // layout (when the width is first known) and device rotation; the content-driven measure happens
        // directly from OnLoaded.
        private sealed class LayoutChangeListener : Java.Lang.Object, View.IOnLayoutChangeListener
        {
            private readonly Action<int> _onWidth;
            private int _lastWidth = -1;

            public LayoutChangeListener(Action<int> onWidth) => _onWidth = onWidth;

            public void OnLayoutChange(
                View? v,
                int left,
                int top,
                int right,
                int bottom,
                int oldLeft,
                int oldTop,
                int oldRight,
                int oldBottom
            )
            {
                int width = right - left;
                if (width <= 0 || width == _lastWidth)
                {
                    return;
                }

                _lastWidth = width;
                _onWidth(width);
            }
        }

        // Bridges the native load callback into the service. The AdLoader owns this instance until it
        // fires.
        private sealed class NativeAdLoadedListener
            : Java.Lang.Object,
                AndroidNativeAd.IOnNativeAdLoadedListener
        {
            private readonly NativeAd _control;
            private readonly AndroidNativeAdView _adView;
            private readonly TemplateViews _views;
            private readonly NativeAdStyle _style;
            private readonly ILogger _logger;
            private readonly string _adUnitId;

            public NativeAdLoadedListener(
                NativeAd control,
                AndroidNativeAdView adView,
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

            public void OnNativeAdLoaded(AndroidNativeAd p0) =>
                _control.OnLoaded(p0, _adView, _views, _style, _logger, _adUnitId);
        }

        // Bridges the native load-failure callback into the service. The AdLoader owns this instance
        // until it fires.
        private sealed class NativeAdListener : AdListener
        {
            private readonly ILogger _logger;
            private readonly string _adUnitId;

            public NativeAdListener(ILogger logger, string adUnitId)
            {
                _logger = logger;
                _adUnitId = adUnitId;
            }

            public override void OnAdFailedToLoad(LoadAdError p0) =>
                OnFailedToLoad(p0, _logger, _adUnitId);
        }
    }
}
