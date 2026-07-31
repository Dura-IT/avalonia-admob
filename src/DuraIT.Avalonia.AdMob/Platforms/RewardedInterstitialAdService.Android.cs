using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Gms.Ads.Rewarded;
using Android.Gms.Ads.RewardedInterstitial;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Platforms;

/// <summary>
/// Android implementation of <see cref="IRewardedInterstitialAdService" />: loads and presents a
/// native AdMob <see cref="RewardedInterstitialAd" />, gating on UMP consent through
/// <see cref="AdMobInitializer" /> and delegating lifecycle bookkeeping to a
/// <see cref="FullScreenAdController" />.
/// </summary>
internal sealed class RewardedInterstitialAdService : IRewardedInterstitialAdService
{
    private const string Format = "rewarded interstitial";

    private readonly ILogger _logger;
    private readonly FullScreenAdController _controller;
    private RewardedInterstitialAd? _ad;
    private TaskCompletionSource<AdReward?>? _showCompletion;

    public RewardedInterstitialAdService()
    {
        _logger = AdMobRuntime.LoggerFactory.CreateLogger<RewardedInterstitialAdService>();
        _controller = new FullScreenAdController(_logger, Format, null, TimeProvider.System);
    }

    public bool IsSupported => true;

    public bool IsReady => _controller.IsReady;

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The AdRequest and load callback are consumed by the native RewardedInterstitialAd.Load call and their lifetime transfers to the native SDK; disposing them here would break the load."
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

        string resolvedAdUnitId = AdUnitResolver.Resolve(
            adUnitId,
            AdMobTestIds.RewardedInterstitial
        );
        if (!_controller.TryBeginLoad(resolvedAdUnitId))
        {
            return;
        }

        RewardedInterstitialAd.Load(
            activity,
            resolvedAdUnitId,
            new AdRequest.Builder().Build(),
            new RewardedInterstitialLoadCallback(this)
        );
    }

    /// <inheritdoc />
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The content callback and reward listener lifetimes transfer to the native RewardedInterstitialAd; disposing them here would break the presentation callbacks."
    )]
    public Task<AdReward?> ShowAsync()
    {
        var activity = AdMobInitializer.CurrentActivity;
        if (activity is null || _ad is null || !_controller.TryBeginShow())
        {
            return Task.FromResult<AdReward?>(null);
        }

        _showCompletion = new TaskCompletionSource<AdReward?>();
        _ad.FullScreenContentCallback = new RewardedInterstitialContentCallback(this);
        _ad.Show(activity, new RewardListener(this));
        return _showCompletion.Task;
    }

    private void OnLoaded(RewardedInterstitialAd ad)
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

    private void OnRewardEarned(IRewardItem reward) =>
        _controller.MarkRewardEarned(new AdReward(reward.Type ?? string.Empty, reward.Amount));

    private void OnDismissed()
    {
        _ad = null;
        var reward = _controller.LastReward;
        _controller.MarkDismissed();
        _showCompletion?.TrySetResult(reward);
    }

    private void OnFailedToShow(AdError error)
    {
        _ad = null;
        _controller.MarkFailedToShow(error.Code, error.Message);
        _showCompletion?.TrySetResult(null);
    }

    // Bridges the native load callbacks into the service. The RewardedInterstitialAd.Load call owns
    // this instance until it fires.
    private sealed class RewardedInterstitialLoadCallback : RewardedInterstitialAdLoadCallback
    {
        private readonly RewardedInterstitialAdService _service;

        public RewardedInterstitialLoadCallback(RewardedInterstitialAdService service) =>
            _service = service;

        public override void OnAdLoaded(RewardedInterstitialAd p0) => _service.OnLoaded(p0);

        public override void OnAdFailedToLoad(LoadAdError p0) => _service.OnFailedToLoad(p0);
    }

    // Bridges the native full-screen presentation callbacks into the service. The
    // RewardedInterstitialAd owns this instance until it is destroyed.
    private sealed class RewardedInterstitialContentCallback : FullScreenContentCallback
    {
        private readonly RewardedInterstitialAdService _service;

        public RewardedInterstitialContentCallback(RewardedInterstitialAdService service) =>
            _service = service;

        public override void OnAdShowedFullScreenContent() => _service.OnShown();

        public override void OnAdDismissedFullScreenContent() => _service.OnDismissed();

        public override void OnAdFailedToShowFullScreenContent(AdError p0) =>
            _service.OnFailedToShow(p0);
    }

    // Bridges the reward callback into the service. The RewardedInterstitialAd.Show call owns this
    // instance until presentation completes.
    private sealed class RewardListener : Java.Lang.Object, IOnUserEarnedRewardListener
    {
        private readonly RewardedInterstitialAdService _service;

        public RewardListener(RewardedInterstitialAdService service) => _service = service;

        public void OnUserEarnedReward(IRewardItem p0) => _service.OnRewardEarned(p0);
    }
}
