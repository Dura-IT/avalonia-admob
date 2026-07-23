using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

[TestFixture]
[TestOf(typeof(BannerAdUnitResolver))]
public class BannerAdUnitResolverTests
{
    [TearDown]
    public void TearDown() => AdMobRuntime.Options = new AdMobOptions();

    [Test]
    public void Resolve_WhenTestAdsDisabledAndIdConfigured_ReturnsConfiguredId()
    {
        AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

        var result = BannerAdUnitResolver.Resolve("ca-app-pub-123/456");

        result.Should().Be("ca-app-pub-123/456");
    }

    [Test]
    public void Resolve_WhenTestAdsEnabled_ReturnsTestIdRegardlessOfConfiguredId()
    {
        AdMobRuntime.Options = new AdMobOptions { UseTestAds = true };

        var result = BannerAdUnitResolver.Resolve("ca-app-pub-123/456");

        result.Should().Be(AdMobTestIds.Banner);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Resolve_WhenTestAdsDisabledAndIdIsNullOrWhitespace_ReturnsTestId(
        string? configuredAdUnitId
    )
    {
        AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

        var result = BannerAdUnitResolver.Resolve(configuredAdUnitId);

        result.Should().Be(AdMobTestIds.Banner);
    }
}
