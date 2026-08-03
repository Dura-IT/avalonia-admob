using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AdUnitId))]
    public class AdUnitIdTests
    {
        [Test]
        public void Constructor_MapsPlatformIdsToProperties()
        {
            var id = new AdUnitId("ca-app-pub-1/android", "ca-app-pub-1/ios");

            id.Android.Should().Be("ca-app-pub-1/android");
            id.IOS.Should().Be("ca-app-pub-1/ios");
        }

        [Test]
        public void Records_WithSamePlatformIds_AreEqual()
        {
            var a = new AdUnitId("android", "ios");
            var b = new AdUnitId("android", "ios");

            a.Should().Be(b);
        }
    }
}
