using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using MT.GMA.iOS;
using UIKit;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// iOS implementation of <see cref="IRewardedInterstitialAdService" />: loads and presents a native
/// AdMob <see cref="GADRewardedInterstitialAd" />, gating on UMP consent through
/// <see cref="AdMobInitializer" /> and delegating lifecycle bookkeeping to a
/// <see cref="FullScreenAdController" />.
/// </summary>
internal sealed class RewardedInterstitialAdService : IRewardedInterstitialAdService
{
    private const string Format = "rewarded interstitial";

    private readonly ILogger _logger;
    private readonly FullScreenAdController _controller;
    private GADRewardedInterstitialAd? _ad;
    private TaskCompletionSource<AdReward?>? _showCompletion;

    public RewardedInterstitialAdService()
    {
        _logger = AdMobRuntime.LoggerFactory.CreateLogger<RewardedInterstitialAdService>();
        _controller = new FullScreenAdController(_logger, Format, null, TimeProvider.System);
    }

    public bool IsSupported => true;

    public bool IsReady => _controller.IsReady;

    /// <inheritdoc />
    public async Task LoadAsync(string? adUnitId = null)
    {
        var viewController = AdMobInitializer.ResolveTopViewController();
        if (viewController is null)
        {
            return;
        }

        var canRequestAds = await AdMobInitializer.EnsureReadyAsync(viewController);
        if (!canRequestAds)
        {
            FullScreenAdLog.BlockedByConsent(_logger, Format);
            return;
        }

        string resolvedAdUnitId = AdUnitResolver.Resolve(
            adUnitId,
            AdMobTestIds.RewardedInterstitial
        );
        if (!_controller.TryBeginLoad(resolvedAdUnitId))
        {
            return;
        }

        GADRewardedInterstitialAd.Load(resolvedAdUnitId, GADRequest.Request(), OnLoadCompleted);
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The content delegate's lifetime transfers to the native GADRewardedInterstitialAd; disposing it here would break the presentation callbacks."
    )]
    public Task<AdReward?> ShowAsync()
    {
        var viewController = AdMobInitializer.ResolveTopViewController();
        if (viewController is null || _ad is null || !_controller.TryBeginShow())
        {
            return Task.FromResult<AdReward?>(null);
        }

        _showCompletion = new TaskCompletionSource<AdReward?>();
        ((IGADFullScreenPresentingAd)_ad).Delegate = new RewardedInterstitialContentDelegate(this);
        _ad.Present(viewController, OnUserDidEarnReward);
        return _showCompletion.Task;
    }

    private void OnLoadCompleted(GADRewardedInterstitialAd? ad, NSError? error)
    {
        if (ad is null || error is not null)
        {
            _ad = null;
            _controller.MarkFailedToLoad(
                error?.Code ?? 0,
                error?.LocalizedDescription ?? string.Empty
            );
            return;
        }

        _ad = ad;
        _controller.MarkLoaded();
    }

    private void OnUserDidEarnReward()
    {
        if (_ad?.AdReward is not { } reward)
        {
            return;
        }

        _controller.MarkRewardEarned(
            new AdReward(reward.Type ?? string.Empty, (decimal)reward.Amount.DoubleValue)
        );
    }

    private void OnShown() => _controller.MarkShown();

    private void OnDismissed()
    {
        _ad = null;
        var reward = _controller.LastReward;
        _controller.MarkDismissed();
        _showCompletion?.TrySetResult(reward);
    }

    private void OnFailedToShow(NSError error)
    {
        _ad = null;
        _controller.MarkFailedToShow(error.Code, error.LocalizedDescription);
        _showCompletion?.TrySetResult(null);
    }

    // Bridges the native full-screen presentation callbacks into the service. The
    // GADRewardedInterstitialAd owns this delegate until it is released.
    private sealed class RewardedInterstitialContentDelegate : GADFullScreenContentDelegate
    {
        private readonly RewardedInterstitialAdService _service;

        public RewardedInterstitialContentDelegate(RewardedInterstitialAdService service) =>
            _service = service;

        public override void AdWillPresentFullScreenContent(GADFullScreenPresentingAd ad) =>
            _service.OnShown();

        public override void AdDidDismissFullScreenContent(GADFullScreenPresentingAd ad) =>
            _service.OnDismissed();

        public override void DidFailToPresentFullScreenContent(
            GADFullScreenPresentingAd ad,
            NSError error
        ) => _service.OnFailedToShow(error);
    }
}
