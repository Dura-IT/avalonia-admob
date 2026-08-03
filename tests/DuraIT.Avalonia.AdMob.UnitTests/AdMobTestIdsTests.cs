using AwesomeAssertions;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AdMobTestIds))]
    public class AdMobTestIdsTests
    {
        // Only the #else (non-Android, non-iOS) branch compiles for the net10.0 test run, so this guards
        // the desktop-fallback constant against a typo. The Android/iOS values are verified by hand
        // against Google's test-ads pages; the source XML doc points at them.
        [Test]
        public void Native_IsGooglesSampleNativeAdvancedUnit()
        {
            AdMobTestIds.Native.Should().Be("ca-app-pub-3940256099942544/2247696110");
        }
    }
}
