using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

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

        services.AddAdMobBanner(options => options.UseTestAds = true, NullLoggerFactory.Instance);

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
    public void AddAdMob_WhenCalled_RegistersBannerAndInterstitialServices()
    {
        var services = new ServiceCollection();

        services.AddAdMob();

        using var provider = services.BuildServiceProvider();
        provider.GetService<IBannerAdService>().Should().NotBeNull();
        provider.GetService<IInterstitialAdService>().Should().NotBeNull();
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
