using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Platform;
using Foundation;
using Microsoft.Extensions.Logging;
using MT.GMA.iOS;
using UIKit;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
///     iOS rendering of <see cref="BannerAd" />: hosts a native AdMob <see cref="GADBannerView" /> inside
///     the Avalonia visual tree.
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
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The GADBannerView and its delegate transfer their lifetime to the returned native control handle and are released by the base DestroyNativeControlCore; the GADRequest is consumed by the native LoadRequest call. Disposing any of them here would break the banner."
    )]
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var logger = AdMobRuntime.LoggerFactory.CreateLogger<BannerAd>();
        string adUnitId = BannerAdUnitResolver.Resolve(AdUnitId);
        var rootViewController = ResolveRootViewController(parent);
        var bannerView = new GADBannerView(GADAdSizes.Banner)
        {
            AdUnitID = adUnitId,
            RootViewController = rootViewController,
            Delegate = new BannerAdDelegate(logger, adUnitId),
        };

        // Consent (UMP) must be resolved before any ad is requested. If we can't resolve a view
        // controller to present a consent form on, fail open rather than leave the banner blank forever.
        if (rootViewController is not null)
        {
            _ = LoadWhenConsentedAsync(bannerView, rootViewController, logger);
        }
        else
        {
            bannerView.LoadRequest(GADRequest.Request());
        }

        return new UIViewControlHandle(bannerView);
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The GADRequest is consumed by the native LoadRequest call. Disposing it here would break the banner."
    )]
    private static async Task LoadWhenConsentedAsync(
        GADBannerView bannerView,
        UIViewController rootViewController,
        ILogger logger
    )
    {
        var canRequestAds = await IosBannerAds.EnsureReadyAsync(rootViewController);
        if (canRequestAds)
        {
            bannerView.LoadRequest(GADRequest.Request());
        }
        else
        {
            BannerAdLog.BlockedByConsent(logger);
        }
    }

    // The banner needs a root view controller to present full-screen content after a tap. Prefer the
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

    // Bridges the native GADBannerView's load callbacks to the library's structured logging so a
    // failed load is no longer silent. The GADBannerView owns this delegate; it is released when the
    // banner view is destroyed.
    private sealed class BannerAdDelegate : GADBannerViewDelegate
    {
        private readonly ILogger _logger;
        private readonly string _adUnitId;

        public BannerAdDelegate(ILogger logger, string adUnitId)
        {
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public override void DidReceiveAd(GADBannerView bannerView) =>
            BannerAdLog.Loaded(_logger, _adUnitId);

        public override void DidFailToReceiveAd(GADBannerView bannerView, NSError error) =>
            BannerAdLog.FailedToLoad(_logger, _adUnitId, error.Code, error.LocalizedDescription);
    }
}
