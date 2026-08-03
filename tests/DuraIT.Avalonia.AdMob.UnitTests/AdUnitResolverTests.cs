using System;
using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AdUnitResolver))]
    public class AdUnitResolverTests
    {
        private const string TestUnit = AdMobTestIds.Interstitial;
        private const string ExplicitId = "ca-app-pub-123/explicit";
        private const string ConfiguredId = "ca-app-pub-123/configured";

        [TearDown]
        public void TearDown() => AdMobRuntime.Options = new AdMobOptions();

        [Test]
        public void Resolve_WhenTestAdsEnabled_ReturnsTestUnitRegardlessOfProvidedIds()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = true };

            var result = AdUnitResolver.Resolve(ExplicitId, ConfiguredId, TestUnit);

            result.Should().Be(TestUnit);
        }

        [Test]
        public void Resolve_WhenExplicitIdProvided_PrefersExplicitOverConfigured()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.Resolve(ExplicitId, ConfiguredId, TestUnit);

            result.Should().Be(ExplicitId);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Resolve_WhenNoExplicitId_FallsBackToConfigured(string? explicitId)
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.Resolve(explicitId, ConfiguredId, TestUnit);

            result.Should().Be(ConfiguredId);
        }

        [Test]
        public void Resolve_WhenTestAdsDisabledAndNoIdAnywhere_ReturnsNull()
        {
            // The fix for the silent-sample-substitution bug: a production request with nothing configured
            // must not fall through to Google's sample unit.
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.Resolve(null, null, TestUnit);

            result.Should().BeNull();
        }

        [Test]
        public void ResolveOrThrow_WhenIdResolves_ReturnsResolvedId()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            var result = AdUnitResolver.ResolveOrThrow(
                null,
                ConfiguredId,
                TestUnit,
                "interstitial",
                "Android"
            );

            result.Should().Be(ConfiguredId);
        }

        [Test]
        public void ResolveOrThrow_WhenNoIdAnywhereAndTestAdsDisabled_ThrowsWithFormatAndPlatform()
        {
            AdMobRuntime.Options = new AdMobOptions { UseTestAds = false };

            Action act = () =>
                AdUnitResolver.ResolveOrThrow(null, null, TestUnit, "interstitial", "Android");

            act.Should().Throw<InvalidOperationException>().WithMessage("*interstitial*Android*");
        }
    }
}
