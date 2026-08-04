using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests
{
    [TestFixture]
    [TestOf(typeof(AppIdValidator))]
    public class AppIdValidatorTests
    {
        private const string RealAppId = "ca-app-pub-1234567890123456~1234567890";

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Evaluate_WhenAppIdMissing_ReturnsMissing(string? appId)
        {
            AppIdValidator
                .Evaluate(appId, useTestAds: false)
                .Should()
                .Be(AppIdValidator.AppIdStatus.Missing);
        }

        [Test]
        public void Evaluate_WhenMissingAndTestAdsOn_StillReturnsMissing()
        {
            // A missing app id stops the SDK initializing regardless of test ads, so it always warns.
            AppIdValidator
                .Evaluate(null, useTestAds: true)
                .Should()
                .Be(AppIdValidator.AppIdStatus.Missing);
        }

        [TestCase(AppIdValidator.SampleAndroidAppId)]
        [TestCase(AppIdValidator.SampleIOSAppId)]
        public void Evaluate_WhenSampleIdAndTestAdsOff_ReturnsSample(string appId)
        {
            AppIdValidator
                .Evaluate(appId, useTestAds: false)
                .Should()
                .Be(AppIdValidator.AppIdStatus.Sample);
        }

        [TestCase(AppIdValidator.SampleAndroidAppId)]
        [TestCase(AppIdValidator.SampleIOSAppId)]
        public void Evaluate_WhenSampleIdAndTestAdsOn_ReturnsOk(string appId)
        {
            AppIdValidator
                .Evaluate(appId, useTestAds: true)
                .Should()
                .Be(AppIdValidator.AppIdStatus.Ok);
        }

        [Test]
        public void Evaluate_WhenRealIdAndTestAdsOff_ReturnsOk()
        {
            AppIdValidator
                .Evaluate(RealAppId, useTestAds: false)
                .Should()
                .Be(AppIdValidator.AppIdStatus.Ok);
        }

        [Test]
        public void Report_WhenMissing_LogsWarningWithPlatform()
        {
            var logger = new CapturingLogger();

            AppIdValidator.Report(logger, appId: null, useTestAds: false, platform: "Android");

            logger.Entries.Should().ContainSingle();
            logger.Entries[0].Level.Should().Be(LogLevel.Warning);
            logger.Entries[0].Message.Should().Contain("Android");
        }

        [Test]
        public void Report_WhenSampleAndTestAdsOff_LogsWarning()
        {
            var logger = new CapturingLogger();

            AppIdValidator.Report(
                logger,
                AppIdValidator.SampleIOSAppId,
                useTestAds: false,
                platform: "iOS"
            );

            logger.Entries.Should().ContainSingle();
            logger.Entries[0].Level.Should().Be(LogLevel.Warning);
            logger.Entries[0].Message.Should().Contain("iOS");
        }

        [Test]
        public void Report_WhenOk_LogsNothing()
        {
            var logger = new CapturingLogger();

            AppIdValidator.Report(logger, RealAppId, useTestAds: false, platform: "Android");

            logger.Entries.Should().BeEmpty();
        }
    }
}
