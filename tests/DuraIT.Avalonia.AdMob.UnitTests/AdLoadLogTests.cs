using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DuraIT.Avalonia.AdMob.UnitTests;

[TestFixture]
[TestOf(typeof(AdLoadLog))]
public class AdLoadLogTests
{
    [Test]
    public void Loaded_LogsDebugWithFormatAndAdUnit()
    {
        var logger = new CapturingLogger();

        AdLoadLog.Loaded(logger, "banner", "unit-loaded");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Debug);
        logger.Entries[0].Message.Should().Contain("banner").And.Contain("unit-loaded");
    }

    [Test]
    public void FailedToLoad_LogsWarningWithFormatAndErrorDetail()
    {
        var logger = new CapturingLogger();

        AdLoadLog.FailedToLoad(logger, "native ad", "unit-failed", 3, "No fill");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
        logger
            .Entries[0]
            .Message.Should()
            .Contain("native ad")
            .And.Contain("unit-failed")
            .And.Contain("3")
            .And.Contain("No fill");
    }

    [Test]
    public void BlockedByConsent_LogsInformationWithFormat()
    {
        var logger = new CapturingLogger();

        AdLoadLog.BlockedByConsent(logger, "native ad");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Level.Should().Be(LogLevel.Information);
        logger.Entries[0].Message.Should().Contain("native ad");
    }
}
