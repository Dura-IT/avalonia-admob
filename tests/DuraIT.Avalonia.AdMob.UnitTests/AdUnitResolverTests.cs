using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AdUnitResolver))]
    public class AdUnitResolverTests
    {
        private const string TestUnit = AdMobTestIds.Interstitial;

        [TearDown]
        public void TearDown() => AdMobRuntime.Options = new AdMobOptions();

        [Test]
        public void Resolve_WhenTestAdsDisabledAndIdConfigured_ReturnsConfiguredId()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.Resolve("ca-app-pub-123/456", TestUnit);

            result.Should().Be("ca-app-pub-123/456");
        }

        [Test]
        public void Resolve_WhenTestAdsEnabled_ReturnsTestUnitRegardlessOfConfiguredId()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = true };

            var result = AdUnitResolver.Resolve("ca-app-pub-123/456", TestUnit);

            result.Should().Be(TestUnit);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Resolve_WhenTestAdsDisabledAndIdIsNullOrWhitespace_ReturnsTestUnit(
            string? configuredAdUnitId
        )
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.Resolve(configuredAdUnitId, TestUnit);

            result.Should().Be(TestUnit);
        }
    }
}
