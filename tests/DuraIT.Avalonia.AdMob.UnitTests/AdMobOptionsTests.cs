using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AdMobOptions))]
    public class AdMobOptionsTests
    {
        [Test]
        public void AdUnitIdProperties_DefaultToNull()
        {
            var options = new AdMobOptions();

            options.BannerAdUnitId.Should().BeNull();
            options.NativeAdUnitId.Should().BeNull();
            options.InterstitialAdUnitId.Should().BeNull();
            options.RewardedAdUnitId.Should().BeNull();
            options.RewardedInterstitialAdUnitId.Should().BeNull();
            options.AppOpenAdUnitId.Should().BeNull();
        }

        [Test]
        public void AdUnitIdProperties_RoundTripTheAssignedValue()
        {
            var banner = new AdUnitId("b-android", "b-ios");
            var native = new AdUnitId("n-android", "n-ios");
            var interstitial = new AdUnitId("i-android", "i-ios");
            var rewarded = new AdUnitId("r-android", "r-ios");
            var rewardedInterstitial = new AdUnitId("ri-android", "ri-ios");
            var appOpen = new AdUnitId("ao-android", "ao-ios");

            var options = new AdMobOptions
            {
                BannerAdUnitId = banner,
                NativeAdUnitId = native,
                InterstitialAdUnitId = interstitial,
                RewardedAdUnitId = rewarded,
                RewardedInterstitialAdUnitId = rewardedInterstitial,
                AppOpenAdUnitId = appOpen,
            };

            options.BannerAdUnitId.Should().Be(banner);
            options.NativeAdUnitId.Should().Be(native);
            options.InterstitialAdUnitId.Should().Be(interstitial);
            options.RewardedAdUnitId.Should().Be(rewarded);
            options.RewardedInterstitialAdUnitId.Should().Be(rewardedInterstitial);
            options.AppOpenAdUnitId.Should().Be(appOpen);
        }
    }
}
