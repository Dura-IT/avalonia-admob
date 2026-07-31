using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Gms.Ads.AppOpen;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Android implementation of <see cref="IAppOpenAdService" />: loads and presents a native AdMob
/// <see cref="AppOpenAd" />, gating on UMP consent through <see cref="AdMobInitializer" /> and
/// delegating lifecycle bookkeeping — including four-hour freshness expiry — to a
/// <see cref="FullScreenAdController" />.
/// </summary>
internal sealed class AppOpenAdService : IAppOpenAdService
{
    private const string Format = "app open";

    // App-open ads must be discarded four hours after loading, per Google's guidance.
    private static readonly TimeSpan Freshness = TimeSpan.FromHours(4);

    private readonly ILogger _logger;
    private readonly FullScreenAdController _controller;
    private AppOpenAd? _ad;
    private TaskCompletionSource<bool>? _showCompletion;

    public AppOpenAdService()
    {
        _logger = AdMobRuntime.LoggerFactory.CreateLogger<AppOpenAdService>();
        _controller = new FullScreenAdController(_logger, Format, Freshness, TimeProvider.System);
    }

    public bool IsSupported => true;

    public bool IsReady => _controller.IsReady;

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdRequest and load callback are consumed by the native AppOpenAd.Load call and their lifetime transfers to the native SDK; disposing them here would break the load."
    )]
    public async Task LoadAsync(string? adUnitId = null)
    {
        var activity = AdMobInitializer.CurrentActivity;
        if (activity is null)
        {
            return;
        }

        var canRequestAds = await AdMobInitializer.EnsureReadyAsync(activity);
        if (!canRequestAds)
        {
            AdLoadLog.BlockedByConsent(_logger, Format);
            return;
        }

        string resolvedAdUnitId = AdUnitResolver.Resolve(adUnitId, AdMobTestIds.AppOpen);
        if (!_controller.TryBeginLoad(resolvedAdUnitId))
        {
            return;
        }

        AppOpenAd.Load(
            activity,
            resolvedAdUnitId,
            new AdRequest.Builder().Build(),
            new AppOpenLoadCallback(this)
        );
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The content callback's lifetime transfers to the native AppOpenAd; disposing it here would break the presentation callbacks."
    )]
    public Task<bool> ShowAsync()
    {
        var activity = AdMobInitializer.CurrentActivity;
        if (activity is null || _ad is null || !_controller.TryBeginShow())
        {
            return Task.FromResult(false);
        }

        _showCompletion = new TaskCompletionSource<bool>();
        _ad.FullScreenContentCallback = new AppOpenContentCallback(this);
        _ad.Show(activity);
        return _showCompletion.Task;
    }

    private void OnLoaded(AppOpenAd ad)
    {
        _ad = ad;
        _controller.MarkLoaded();
    }

    private void OnFailedToLoad(LoadAdError error)
    {
        _ad = null;
        _controller.MarkFailedToLoad(error.Code, error.Message);
    }

    private void OnShown() => _controller.MarkShown();

    private void OnDismissed()
    {
        _ad = null;
        _controller.MarkDismissed();
        _showCompletion?.TrySetResult(true);
    }

    private void OnFailedToShow(AdError error)
    {
        _ad = null;
        _controller.MarkFailedToShow(error.Code, error.Message);
        _showCompletion?.TrySetResult(false);
    }

    // Bridges the native load callbacks into the service. The AppOpenAd.Load call owns this instance
    // until it fires.
    private sealed class AppOpenLoadCallback : AppOpenAd.AppOpenAdLoadCallback
    {
        private readonly AppOpenAdService _service;

        public AppOpenLoadCallback(AppOpenAdService service) => _service = service;

        public override void OnAdLoaded(AppOpenAd p0) => _service.OnLoaded(p0);

        public override void OnAdFailedToLoad(LoadAdError p0) => _service.OnFailedToLoad(p0);
    }

    // Bridges the native full-screen presentation callbacks into the service. The AppOpenAd owns this
    // instance until it is destroyed.
    private sealed class AppOpenContentCallback : FullScreenContentCallback
    {
        private readonly AppOpenAdService _service;

        public AppOpenContentCallback(AppOpenAdService service) => _service = service;

        public override void OnAdShowedFullScreenContent() => _service.OnShown();

        public override void OnAdDismissedFullScreenContent() => _service.OnDismissed();

        public override void OnAdFailedToShowFullScreenContent(AdError p0) =>
            _service.OnFailedToShow(p0);
    }
}
