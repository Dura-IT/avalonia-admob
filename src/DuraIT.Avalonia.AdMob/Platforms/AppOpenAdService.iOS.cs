using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.Logging;
using MT.GMA.iOS;
using UIKit;

namespace DuraIT.Avalonia.AdMob.Platforms
{
    /// <summary>
    /// iOS implementation of <see cref="IAppOpenAdService" />: loads and presents a native AdMob
    /// <see cref="GADAppOpenAd" />, gating on UMP consent through <see cref="AdMobInitializer" /> and
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
        private GADAppOpenAd? _ad;
        private TaskCompletionSource<bool>? _showCompletion;

        public AppOpenAdService()
        {
            _logger = AdMobRuntime.LoggerFactory.CreateLogger<AppOpenAdService>();
            _controller = new FullScreenAdController(
                _logger,
                Format,
                Freshness,
                TimeProvider.System
            );
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
                AdLoadLog.BlockedByConsent(_logger, Format);
                return;
            }

            string resolvedAdUnitId = AdUnitResolver.Resolve(adUnitId, AdMobTestIds.AppOpen);
            if (!_controller.TryBeginLoad(resolvedAdUnitId))
            {
                return;
            }

            GADAppOpenAd.Load(resolvedAdUnitId, GADRequest.Request(), OnLoadCompleted);
        }

        /// <inheritdoc />
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The content delegate's lifetime transfers to the native GADAppOpenAd; disposing it here would break the presentation callbacks."
        )]
        public Task<bool> ShowAsync()
        {
            var viewController = AdMobInitializer.ResolveTopViewController();
            if (viewController is null || _ad is null || !_controller.TryBeginShow())
            {
                return Task.FromResult(false);
            }

            _showCompletion = new TaskCompletionSource<bool>();
            ((IGADFullScreenPresentingAd)_ad).Delegate = new AppOpenContentDelegate(this);
            _ad.Present(viewController);
            return _showCompletion.Task;
        }

        private void OnLoadCompleted(GADAppOpenAd? ad, NSError? error)
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

        private void OnShown() => _controller.MarkShown();

        private void OnDismissed()
        {
            _ad = null;
            _controller.MarkDismissed();
            _showCompletion?.TrySetResult(true);
        }

        private void OnFailedToShow(NSError error)
        {
            _ad = null;
            _controller.MarkFailedToShow(error.Code, error.LocalizedDescription);
            _showCompletion?.TrySetResult(false);
        }

        // Bridges the native full-screen presentation callbacks into the service. The GADAppOpenAd owns
        // this delegate until it is released.
        private sealed class AppOpenContentDelegate : GADFullScreenContentDelegate
        {
            private readonly AppOpenAdService _service;

            public AppOpenContentDelegate(AppOpenAdService service) => _service = service;

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
}
