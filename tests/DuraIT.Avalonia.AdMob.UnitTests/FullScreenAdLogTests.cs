using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

[TestFixture]
[TestOf(typeof(FullScreenAdLog))]
public class FullScreenAdLogTests
{
    [Test]
    public void Loaded_LogsDebugWithFormatAndAdUnit()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.Loaded(logger, "interstitial", "unit-loaded");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain("interstitial").And.Contain("unit-loaded");
    }

    [Test]
    public void FailedToLoad_LogsWarningWithAdUnitAndErrorDetail()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.FailedToLoad(logger, "interstitial", "unit-failed", 3, "No fill");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
        logger
            .Entries[0]
            .Message.Should()
            .Contain("unit-failed")
            .And.Contain("3")
            .And.Contain("No fill");
    }

    [Test]
    public void BlockedByConsent_LogsInformationWithFormat()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.BlockedByConsent(logger, "interstitial");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
        logger.Entries[0].Message.Should().Contain("interstitial");
    }

    [Test]
    public void Showed_LogsDebugWithAdUnit()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.Showed(logger, "interstitial", "unit-shown");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain("unit-shown");
    }

    [Test]
    public void FailedToShow_LogsWarningWithErrorDetail()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.FailedToShow(logger, "interstitial", "unit-show-fail", 7, "Already used");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
        logger.Entries[0].Message.Should().Contain("7").And.Contain("Already used");
    }

    [Test]
    public void Dismissed_LogsDebugWithAdUnit()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.Dismissed(logger, "interstitial", "unit-dismissed");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain("unit-dismissed");
    }

    [Test]
    public void RewardEarned_LogsInformationWithRewardDetail()
    {
        var logger = new CapturingLogger();

        FullScreenAdLog.RewardEarned(logger, "rewarded", "unit-reward", "coins", 5);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
        logger
            .Entries[0]
            .Message.Should()
            .Contain("rewarded")
            .And.Contain("unit-reward")
            .And.Contain("coins")
            .And.Contain("5");
    }
}
