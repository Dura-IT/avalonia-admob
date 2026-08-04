using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Gms.Ads.Interstitial;
using Microsoft.Extensions.Logging;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// Android implementation of <see cref="IInterstitialAdService" />: loads and presents a native AdMob
    /// <see cref="InterstitialAd" />, gating on UMP consent through <see cref="AdMobInitializer" /> and
    /// delegating lifecycle bookkeeping to a <see cref="FullScreenAdController" />.
    /// </summary>
    internal sealed class InterstitialAdService : IInterstitialAdService
    {
        private const string Format = "interstitial";

        private readonly ILogger _logger;
        private readonly FullScreenAdController _controller;
        private InterstitialAd? _ad;
        private TaskCompletionSource<bool>? _showCompletion;

        public InterstitialAdService()
        {
            _logger = AdMobRuntime.LoggerFactory.CreateLogger<InterstitialAdService>();
            _controller = new FullScreenAdController(_logger, Format, null, TimeProvider.System);
        }

        public bool IsSupported => true;

        public bool IsReady => _controller.IsReady;

        /// <inheritdoc />
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The AdRequest and load callback are consumed by the native InterstitialAd.Load call and their lifetime transfers to the native SDK; disposing them here would break the load."
        )]
        public async Task LoadAsync(string? adUnitId = null)
        {
            string resolvedAdUnitId = AdUnitResolver.ResolveOrThrow(
                adUnitId,
                AdMobRuntime.Options.InterstitialAdUnitId?.Android,
                AdMobTestIds.Interstitial,
                Format,
                "Android"
            );

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

            if (!_controller.TryBeginLoad(resolvedAdUnitId))
            {
                return;
            }

            InterstitialAd.Load(
                activity,
                resolvedAdUnitId,
                new AdRequest.Builder().Build(),
                new InterstitialLoadCallback(this)
            );
        }

        /// <inheritdoc />
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The content callback's lifetime transfers to the native InterstitialAd; disposing it here would break the presentation callbacks."
        )]
        public Task<bool> ShowAsync()
        {
            var activity = AdMobInitializer.CurrentActivity;
            if (activity is null || _ad is null || !_controller.TryBeginShow())
            {
                return Task.FromResult(false);
            }

            _showCompletion = new TaskCompletionSource<bool>();
            _ad.FullScreenContentCallback = new InterstitialContentCallback(this);
            _ad.Show(activity);
            return _showCompletion.Task;
        }

        private void OnLoaded(InterstitialAd ad)
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

        // Bridges the native load callbacks into the service. The InterstitialAd.Load call owns this
        // instance until it fires.
        private sealed class InterstitialLoadCallback : InterstitialAdLoadCallback
        {
            private readonly InterstitialAdService _service;

            public InterstitialLoadCallback(InterstitialAdService service) => _service = service;

            public override void OnAdLoaded(InterstitialAd p0) => _service.OnLoaded(p0);

            public override void OnAdFailedToLoad(LoadAdError p0) => _service.OnFailedToLoad(p0);
        }

        // Bridges the native full-screen presentation callbacks into the service. The InterstitialAd owns
        // this instance until it is destroyed.
        private sealed class InterstitialContentCallback : FullScreenContentCallback
        {
            private readonly InterstitialAdService _service;

            public InterstitialContentCallback(InterstitialAdService service) => _service = service;

            public override void OnAdShowedFullScreenContent() => _service.OnShown();

            public override void OnAdDismissedFullScreenContent() => _service.OnDismissed();

            public override void OnAdFailedToShowFullScreenContent(AdError p0) =>
                _service.OnFailedToShow(p0);
        }
    }
}
