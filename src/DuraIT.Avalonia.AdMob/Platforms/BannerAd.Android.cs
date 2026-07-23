using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
///     Android rendering of <see cref="BannerAd" />: hosts a native AdMob <see cref="AdView" /> inside the
///     Avalonia visual tree.
/// </summary>
public partial class BannerAd : NativeControlHost
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BannerAd" /> class.
    /// </summary>
    public BannerAd()
    {
        Height = 50;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no Android context is available to create the native ad view.
    /// </exception>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdView and its listener transfer their lifetime to the returned native control handle and are released in DestroyNativeControlCore; the AdRequest and its builder are consumed by the native LoadAd call. Disposing any of them here would break the banner."
    )]
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var context =
            (parent as AndroidViewControlHandle)?.View?.Context
            ?? global::Android.App.Application.Context
            ?? throw new InvalidOperationException(
                "No Android context is available to create the AdMob banner view."
            );

        var logger = AdMobRuntime.LoggerFactory.CreateLogger<BannerAd>();
        string adUnitId = BannerAdUnitResolver.Resolve(AdUnitId);

        var adView = new AdView(context)
        {
            AdUnitId = adUnitId,
            AdSize = AdSize.Banner,
            AdListener = new BannerAdListener(logger, adUnitId),
        };

        // Consent (UMP) must be resolved before any ad is requested. If we can't resolve an activity
        // to present a consent form on, fail open rather than leave the banner blank forever.
        if (ResolveActivity(context) is { } activity)
        {
            // Remember the host so the DI service can present the privacy options form later.
            AndroidBannerAds.CurrentActivity = activity;
            _ = LoadWhenConsentedAsync(adView, activity, logger);
        }
        else
        {
            adView.LoadAd(new AdRequest.Builder().Build());
        }

        return new AndroidViewControlHandle(adView);
    }

    /// <inheritdoc />
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (control is AndroidViewControlHandle handle && handle.View is AdView adView)
        {
            adView.Destroy();
        }

        base.DestroyNativeControlCore(control);
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdRequest and its builder are consumed by the native LoadAd call. Disposing them here would break the banner."
    )]
    private static async Task LoadWhenConsentedAsync(
        AdView adView,
        Activity activity,
        ILogger logger
    )
    {
        var canRequestAds = await AndroidBannerAds.EnsureReadyAsync(activity);
        if (canRequestAds)
        {
            adView.LoadAd(new AdRequest.Builder().Build());
        }
        else
        {
            BannerAdLog.BlockedByConsent(logger);
        }
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

    // Bridges the native AdView's load callbacks to the library's structured logging so a failed load
    // is no longer silent. The AdView owns this listener; it is released when the AdView is destroyed.
    private sealed class BannerAdListener : AdListener
    {
        private readonly ILogger _logger;
        private readonly string _adUnitId;

        public BannerAdListener(ILogger logger, string adUnitId)
        {
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public override void OnAdLoaded() => BannerAdLog.Loaded(_logger, _adUnitId);

        public override void OnAdFailedToLoad(LoadAdError p0) =>
            BannerAdLog.FailedToLoad(_logger, _adUnitId, p0.Code, p0.Message);
    }
}
