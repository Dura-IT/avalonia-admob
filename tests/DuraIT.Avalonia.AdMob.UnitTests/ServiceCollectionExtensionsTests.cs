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
        using var provider = services.BuildServiceProvider();
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
}
