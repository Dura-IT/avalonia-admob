using Avalonia;
using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(NativeAdStyle))]
    public class NativeAdStyleTests
    {
        [Test]
        public void Default_ShowsEveryOptionalAsset()
        {
            var style = NativeAdStyle.Default;

            style.ShowIcon.Should().BeTrue();
            style.ShowMedia.Should().BeTrue();
            style.ShowBody.Should().BeTrue();
            style.ShowAdvertiser.Should().BeTrue();
            style.ShowStarRating.Should().BeTrue();
            style.ShowPrice.Should().BeTrue();
            style.ShowStore.Should().BeTrue();
        }

        [Test]
        public void Default_LeavesEveryColorNullToDeferToThePlatform()
        {
            var style = NativeAdStyle.Default;

            style.CardBackground.Should().BeNull();
            style.HeadlineForeground.Should().BeNull();
            style.BodyForeground.Should().BeNull();
            style.CallToActionBackground.Should().BeNull();
            style.CallToActionForeground.Should().BeNull();
        }

        [Test]
        public void Default_UsesModestTextSizesAndNoRoundingOrPadding()
        {
            var style = NativeAdStyle.Default;

            style.HeadlineFontSize.Should().Be(16);
            style.BodyFontSize.Should().Be(14);
            style.CornerRadius.Should().Be(0);
            style.Padding.Should().Be(new Thickness(0));
        }
    }
}
