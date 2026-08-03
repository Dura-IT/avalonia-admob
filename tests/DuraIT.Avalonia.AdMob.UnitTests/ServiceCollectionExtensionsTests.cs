using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(ServiceCollectionExtensions))]
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddAdMobBanner_WhenCalled_RegistersResolvableBannerAdService()
        {
            var services = new ServiceCollection();

            services.AddAdMobBanner();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IBannerAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobBanner_OnDesktop_ServiceReportsUnsupported()
        {
            var services = new ServiceCollection();
            services.AddAdMobBanner();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IBannerAdService>();

            service.IsSupported.Should().BeFalse();
        }

        [Test]
        public void AddAdMobBanner_OnDesktop_PrivacyOptionsNotRequired()
        {
            var services = new ServiceCollection();
            services.AddAdMobBanner();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IBannerAdService>();

            service.IsPrivacyOptionsRequired.Should().BeFalse();
        }

        [Test]
        public async Task AddAdMobBanner_OnDesktop_ShowPrivacyOptionsCompletesWithoutThrowing()
        {
            var services = new ServiceCollection();
            services.AddAdMobBanner();
            await using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IBannerAdService>();

            var act = async () => await service.ShowPrivacyOptionsAsync();

            await act.Should().NotThrowAsync();
        }

        [Test]
        public void AddAdMobBanner_WithConfiguration_AppliesItToOptions()
        {
            var services = new ServiceCollection();

            services.AddAdMobBanner(options => options.UseTestAds = true);

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<AdMobOptions>().UseTestAds.Should().BeTrue();
        }

        [Test]
        public void AddAdMobBanner_WithConfiguration_AppliesTagForUnderAgeOfConsentToOptions()
        {
            var services = new ServiceCollection();

            services.AddAdMobBanner(options => options.TagForUnderAgeOfConsent = true);

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<AdMobOptions>().TagForUnderAgeOfConsent.Should().BeTrue();
        }

        [Test]
        public void AddAdMobBanner_WithLoggerFactory_AppliesConfigurationWithoutThrowing()
        {
            var services = new ServiceCollection();

            services.AddAdMobBanner(
                options => options.UseTestAds = true,
                NullLoggerFactory.Instance
            );

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<AdMobOptions>().UseTestAds.Should().BeTrue();
        }

        [Test]
        public void AddAdMobBanner_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobBanner();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMobInterstitial_WhenCalled_RegistersResolvableInterstitialService()
        {
            var services = new ServiceCollection();

            services.AddAdMobInterstitial();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IInterstitialAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobInterstitial_OnDesktop_ServiceReportsUnsupportedAndNotReady()
        {
            var services = new ServiceCollection();
            services.AddAdMobInterstitial();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IInterstitialAdService>();

            service.IsSupported.Should().BeFalse();
            service.IsReady.Should().BeFalse();
        }

        [Test]
        public async Task AddAdMobInterstitial_OnDesktop_LoadAndShowAreInert()
        {
            var services = new ServiceCollection();
            services.AddAdMobInterstitial();
            await using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IInterstitialAdService>();

            await service.LoadAsync();
            var shown = await service.ShowAsync();

            shown.Should().BeFalse();
        }

        [Test]
        public void AddAdMobInterstitial_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobInterstitial();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMobRewarded_WhenCalled_RegistersResolvableRewardedService()
        {
            var services = new ServiceCollection();

            services.AddAdMobRewarded();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IRewardedAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobRewarded_OnDesktop_ServiceReportsUnsupportedAndNotReady()
        {
            var services = new ServiceCollection();
            services.AddAdMobRewarded();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IRewardedAdService>();

            service.IsSupported.Should().BeFalse();
            service.IsReady.Should().BeFalse();
        }

        [Test]
        public async Task AddAdMobRewarded_OnDesktop_LoadAndShowAreInert()
        {
            var services = new ServiceCollection();
            services.AddAdMobRewarded();
            await using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IRewardedAdService>();

            await service.LoadAsync();
            var reward = await service.ShowAsync();

            reward.Should().BeNull();
        }

        [Test]
        public void AddAdMobRewarded_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobRewarded();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMobRewardedInterstitial_WhenCalled_RegistersResolvableService()
        {
            var services = new ServiceCollection();

            services.AddAdMobRewardedInterstitial();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IRewardedInterstitialAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobRewardedInterstitial_OnDesktop_ServiceReportsUnsupportedAndNotReady()
        {
            var services = new ServiceCollection();
            services.AddAdMobRewardedInterstitial();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IRewardedInterstitialAdService>();

            service.IsSupported.Should().BeFalse();
            service.IsReady.Should().BeFalse();
        }

        [Test]
        public async Task AddAdMobRewardedInterstitial_OnDesktop_LoadAndShowAreInert()
        {
            var services = new ServiceCollection();
            services.AddAdMobRewardedInterstitial();
            await using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IRewardedInterstitialAdService>();

            await service.LoadAsync();
            var reward = await service.ShowAsync();

            reward.Should().BeNull();
        }

        [Test]
        public void AddAdMobRewardedInterstitial_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobRewardedInterstitial();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMobAppOpen_WhenCalled_RegistersResolvableAppOpenService()
        {
            var services = new ServiceCollection();

            services.AddAdMobAppOpen();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IAppOpenAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobAppOpen_OnDesktop_ServiceReportsUnsupportedAndNotReady()
        {
            var services = new ServiceCollection();
            services.AddAdMobAppOpen();
            using var provider = services.BuildServiceProvider();

            var service = provider.GetRequiredService<IAppOpenAdService>();

            service.IsSupported.Should().BeFalse();
            service.IsReady.Should().BeFalse();
        }

        [Test]
        public async Task AddAdMobAppOpen_OnDesktop_LoadAndShowAreInert()
        {
            var services = new ServiceCollection();
            services.AddAdMobAppOpen();
            await using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<IAppOpenAdService>();

            await service.LoadAsync();
            var shown = await service.ShowAsync();

            shown.Should().BeFalse();
        }

        [Test]
        public void AddAdMobAppOpen_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobAppOpen();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMobNative_WhenCalled_RegistersResolvableOptions()
        {
            var services = new ServiceCollection();

            services.AddAdMobNative();

            using var provider = services.BuildServiceProvider();
            provider.GetService<AdMobOptions>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMobNative_WhenCalled_RegistersNoAdService()
        {
            var services = new ServiceCollection();

            services.AddAdMobNative();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IBannerAdService>().Should().BeNull();
            provider.GetService<IInterstitialAdService>().Should().BeNull();
        }

        [Test]
        public void AddAdMobNative_WithConfiguration_AppliesItToOptions()
        {
            var services = new ServiceCollection();

            services.AddAdMobNative(options => options.UseTestAds = true);

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<AdMobOptions>().UseTestAds.Should().BeTrue();
        }

        [Test]
        public void AddAdMobNative_WhenCalled_ReturnsSameCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddAdMobNative();

            result.Should().BeSameAs(services);
        }

        [Test]
        public void AddAdMob_WhenCalled_RegistersAllAdFormatServices()
        {
            var services = new ServiceCollection();

            services.AddAdMob();

            using var provider = services.BuildServiceProvider();
            provider.GetService<IBannerAdService>().Should().NotBeNull();
            provider.GetService<IInterstitialAdService>().Should().NotBeNull();
            provider.GetService<IRewardedAdService>().Should().NotBeNull();
            provider.GetService<IRewardedInterstitialAdService>().Should().NotBeNull();
            provider.GetService<IAppOpenAdService>().Should().NotBeNull();
        }

        [Test]
        public void AddAdMob_WithConfiguration_AppliesItToOptions()
        {
            var services = new ServiceCollection();

            services.AddAdMob(options => options.UseTestAds = true);

            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<AdMobOptions>().UseTestAds.Should().BeTrue();
        }
    }
}
